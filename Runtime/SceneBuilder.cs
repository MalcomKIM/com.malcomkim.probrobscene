using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace MalcomKim.ProbRobScene{
	public static class SceneBuilder
	{
		private static string MODELS_PARENT = "Models";
		
		public static void BuildModels(TextAsset textPRS)
		{
			GameObject Models = GameObject.Find(MODELS_PARENT);
			List<ModelItem> ModelItems = new List<ModelItem>();
			
			// Get boundaries
			foreach (Transform child in Models.transform)
			{
				// Debug.Log(child.name);
				GameObject go = GameObject.Find(Models.transform.name + "/" + child.name);

				Bounds bounds = SizeHelper.CaptureBounds(go);
				Vector3 size = SizeHelper.getBoundsSize(bounds);

				ModelItem mi = new ModelItem(go, size.x, size.y, size.z, child.name);
				ModelItems.Add(mi);
			}
			
			// Generate Model.prs
			string PrsPath = AssetDatabase.GetAssetPath(textPRS);
			string save_path = Path.GetDirectoryName(Path.GetFullPath(PrsPath));
			CreateModelPrs(ModelItems, save_path);
		}
		
		public static string CreateModelPrs(List<ModelItem> items, string save_path){
			string model_prs = @"import math

width = 5
length = 5
height = 5
workspace = Cuboid(Vector3D(0, 0, height / 2.0), Vector3D(0,0,0), width, length, height)
";
			foreach(ModelItem i in items){
				model_prs = model_prs + i.toString();
			}
			
			File.WriteAllText(save_path+"/model.prs", model_prs);
			//yield return new WaitUntil(() => File.Exists(@"E:/ProbRobScene/scenarios/mymodel.prs"));
			return model_prs;
		}
		
		
		public static SceneItemList getSceneItemList(string PrsPath, string PrsName, string PythonPath, string RuntimePath){
			string json_result = OsUtils.python(PythonPath, RuntimePath, PrsPath);
			
			Debug.Log(PythonPath);
			Debug.Log(PrsPath);
			Debug.Log(json_result);
			
			// Get information of items in the scene
			SceneItemList SceneItems = JsonUtility.FromJson<SceneItemList>(json_result);
			
			return SceneItems;
		}
		
		
		public static void BuildDebugScene(string MaterialPath, SceneItemList SceneItems, string SceneName){
			GameObject DebugScene = new GameObject(SceneName); 
			Material TransparentRed = (Material)AssetDatabase.LoadAssetAtPath(MaterialPath + "/TransparentRed.mat", typeof(Material));
	 
			foreach (SceneItem o in SceneItems.objects)
			{	
				GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
				cube.name = o.model_name;
				cube.transform.localScale = new Vector3 (o.size_x, o.size_y, o.size_z);
				cube.transform.position = new Vector3(o.position_x, o.position_y, o.position_z);
				cube.transform.rotation = Quaternion.Euler(new Vector3(o.rotation_x * Mathf.Rad2Deg, o.rotation_y * Mathf.Rad2Deg, o.rotation_z * Mathf.Rad2Deg));
				cube.GetComponent<Renderer>().material = TransparentRed;
				cube.transform.parent = DebugScene.transform;
			}
		}
		
		public static void BuildRealScene(SceneItemList SceneItems, string SceneName){
			GameObject Models = GameObject.Find(MODELS_PARENT);
			GameObject RealScene = new GameObject(SceneName);
			
			foreach (SceneItem o in SceneItems.objects)
			{	
				// Clone from models
				GameObject clone = Object.Instantiate(GameObject.Find(Models.transform.name + "/" + o.model_name));
				clone.name = o.model_name;
				
				
				Bounds bounds = SizeHelper.CaptureBounds(clone);
				
				// Calculate scale from size
				Vector3 exp_size = new Vector3(o.size_x, o.size_y, o.size_z);
				Vector3 ori_size = SizeHelper.getBoundsSize(bounds);
				
				Vector3 scale = SizeHelper.CalculateScale(exp_size, ori_size);
				clone.transform.localScale = Vector3.Scale(clone.transform.localScale, scale);
				
				// Calculate displacement
				bounds = SizeHelper.CaptureBounds(clone);
				Vector3 center = bounds.center;
				clone.transform.position += new Vector3(o.position_x, o.position_y, o.position_z) - center;
				
				
				clone.transform.Rotate(o.rotation_x * Mathf.Rad2Deg, o.rotation_y * Mathf.Rad2Deg, o.rotation_z * Mathf.Rad2Deg);
				
				clone.transform.parent = RealScene.transform;
			}
			
		}
		
		
		public static void BuildScene(TextAsset textPRS,
									string RuntimePath,
									string MaterialPath,
									string PythonPath,
									bool debugMode){
			GameObject Models = GameObject.Find(MODELS_PARENT);
			
			string PrsPath = AssetDatabase.GetAssetPath(textPRS);
			string PrsName = System.IO.Path.GetFileNameWithoutExtension(PrsPath);	

			SceneItemList SceneItems = SceneBuilder.getSceneItemList(PrsPath, PrsName, PythonPath, RuntimePath);
			// Transparent Red boxes
			if(debugMode){
				BuildDebugScene(MaterialPath, SceneItems, "DebugScene");
				//SceneBuilder.BuildDebugScene("Assets/Samples/Prefabs/Materials", SceneItems);
			}
			BuildRealScene(SceneItems,"RealScene");
			
			// Deactivate the Gameobject which contains all models
			Models.SetActive(false);
		}
		
		
		
		
		
	}
}
