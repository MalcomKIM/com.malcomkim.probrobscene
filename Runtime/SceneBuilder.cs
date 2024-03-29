using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Unity.Robotics.UrdfImporter;
using Unity.Robotics.UrdfImporter.Control;
using UnityEngine.Rendering;

using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace MalcomKim.ProbRobScene{
	public static class SceneBuilder
	{
		// private static string MODELS_PARENT = "Models";
		static Vector3 PreviousRobotCenter;
		
		public static void BuildModels(TextAsset textPRS, GameObject Models)
		{
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
		
		
		public static void BuildModels(string PrsPath, GameObject Models)
		{
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
		
		
		public static GameObject BuildDebugScene(SceneItemList SceneItems, string SceneName){
			GameObject DebugScene = new GameObject(SceneName); 
			
	 
			foreach (SceneItem o in SceneItems.objects)
			{	
			
				Material mat;
				if (GraphicsSettings.currentRenderPipeline.name == "UniversalRP-HighQuality")
				{
					mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
				}
				else{
					mat = new Material(Shader.Find("Standard"));
				}
				Color color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f); 
				color.a = 0.5f;
				mat.color = color;
				mat.SetFloat("_Surface", 1f);
			
				GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
				cube.GetComponent<BoxCollider>().enabled = false;
				cube.name = o.model_name;
				cube.transform.localScale = new Vector3 (o.size_x, o.size_y, o.size_z);
				cube.transform.position = new Vector3(o.position_x, o.position_y, o.position_z);
				cube.transform.rotation = Quaternion.Euler(new Vector3(o.rotation_x * Mathf.Rad2Deg, o.rotation_y * Mathf.Rad2Deg, o.rotation_z * Mathf.Rad2Deg));
				cube.GetComponent<Renderer>().material = mat;
				cube.transform.parent = DebugScene.transform;
			}
			
			return DebugScene;
		}
		
		public static GameObject BuildRealScene(SceneItemList SceneItems, string SceneName, RobotSetting rs, GameObject Models){
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
				
				if(!EditorApplication.isPlaying){
					if(clone.tag == "robot"){
						Transform base_link_transform = clone.transform.Find("world/base_link");
						ArticulationBody ab = base_link_transform.GetComponent<ArticulationBody>();
						
						var controller = clone.GetComponent<Controller>();
						controller.stiffness = rs.k_ControllerStiffness;
						controller.damping = rs.k_ControllerDamping;
						controller.forceLimit = rs.k_ControllerForceLimit;
						controller.speed = rs.k_ControllerSpeed;
						controller.acceleration = rs.k_ControllerAcceleration;
						if(rs.immovable){
							ab.immovable = true; 
						}						
					}
					clone.transform.position += new Vector3(o.position_x, o.position_y, o.position_z) - center;
					clone.transform.Rotate(o.rotation_x * Mathf.Rad2Deg, o.rotation_y * Mathf.Rad2Deg, o.rotation_z * Mathf.Rad2Deg);
				}
				else{
					// Fix robot position
					if(clone.tag == "robot"){
						Transform base_link_transform = clone.transform.Find("world/base_link");
						ArticulationBody ab = base_link_transform.GetComponent<ArticulationBody>();
						Vector3 exp_position = base_link_transform.position + new Vector3(o.position_x, o.position_y, o.position_z) - center;
						PreviousRobotCenter = new Vector3(o.position_x, o.position_y, o.position_z);
						ab.TeleportRoot(exp_position, Quaternion.identity);
						
						var controller = clone.GetComponent<Controller>();
						controller.stiffness = rs.k_ControllerStiffness;
						controller.damping = rs.k_ControllerDamping;
						controller.forceLimit = rs.k_ControllerForceLimit;
						controller.speed = rs.k_ControllerSpeed;
						controller.acceleration = rs.k_ControllerAcceleration;
						if(rs.immovable){
							ab.immovable = true; 
						}						
					}
					else{
						clone.transform.position += new Vector3(o.position_x, o.position_y, o.position_z) - center;
						clone.transform.Rotate(o.rotation_x * Mathf.Rad2Deg, o.rotation_y * Mathf.Rad2Deg, o.rotation_z * Mathf.Rad2Deg);
					}
				}
				
				
				clone.transform.parent = RealScene.transform;
			}
			return RealScene;
		}
		
		
		public static GameObject BuildScene(TextAsset textPRS,
									string RuntimePath,
									string PythonPath,
									string RealSceneName,
									string DebugSceneName,
									RobotSetting rs,
									GameObject Models){
			
			string PrsPath = AssetDatabase.GetAssetPath(textPRS);
			string PrsName = System.IO.Path.GetFileNameWithoutExtension(PrsPath);	

			SceneItemList SceneItems = SceneBuilder.getSceneItemList(PrsPath, PrsName, PythonPath, RuntimePath);

			BuildDebugScene(SceneItems, DebugSceneName);
			
			GameObject RealScene = BuildRealScene(SceneItems,RealSceneName,rs,Models);
			
			return RealScene;
		}
		
		
		
		public static GameObject BuildScene(TextAsset textPRS,
									string RuntimePath,
									string PythonPath,
									string RealSceneName,
									RobotSetting rs,
									GameObject Models){
			
			string PrsPath = AssetDatabase.GetAssetPath(textPRS);
			string PrsName = System.IO.Path.GetFileNameWithoutExtension(PrsPath);	

			SceneItemList SceneItems = SceneBuilder.getSceneItemList(PrsPath, PrsName, PythonPath, RuntimePath);
			
			GameObject RealScene = BuildRealScene(SceneItems,RealSceneName,rs, Models);
			
			return RealScene;
		}
		
		
		public static GameObject UpdateRealScene(SceneItemList SceneItems, string SceneName, RobotSetting rs){
			GameObject RealScene = GameObject.Find(SceneName);
			
			foreach (SceneItem o in SceneItems.objects)
			{	
				// Clone from models
				GameObject clone = GameObject.Find(RealScene.transform.name + "/" + o.model_name);	

				if(clone.tag == "robot"){
					Transform base_link_transform = clone.transform.Find("world/base_link");
					ArticulationBody ab = base_link_transform.GetComponent<ArticulationBody>();
					Vector3 exp_position = base_link_transform.position + new Vector3(o.position_x, o.position_y, o.position_z) - PreviousRobotCenter;
					PreviousRobotCenter = new Vector3(o.position_x, o.position_y, o.position_z);
					ab.TeleportRoot(exp_position, Quaternion.identity);
				}
				else{
					
					
					Bounds bounds = SizeHelper.CaptureBounds(clone);
					Vector3 center = bounds.center;
					// Calculate displacement
					clone.transform.position += new Vector3(o.position_x, o.position_y, o.position_z) - center;
					// Reset rotation
					clone.transform.rotation = Quaternion.identity;
					// Update rotation
					clone.transform.Rotate(o.rotation_x * Mathf.Rad2Deg, o.rotation_y * Mathf.Rad2Deg, o.rotation_z * Mathf.Rad2Deg);
				}
				
				
			}
			return RealScene;
		}
		
		public static GameObject UpdateScene(TextAsset textPRS,
									string RuntimePath,
									string PythonPath,
									string RealSceneName,
									RobotSetting rs){
			
			string PrsPath = AssetDatabase.GetAssetPath(textPRS);
			string PrsName = System.IO.Path.GetFileNameWithoutExtension(PrsPath);	

			SceneItemList SceneItems = SceneBuilder.getSceneItemList(PrsPath, PrsName, PythonPath, RuntimePath);
			
			GameObject RealScene = UpdateRealScene(SceneItems,RealSceneName,rs);
			
			return RealScene;
		}
	}
		
		
		
	
}
