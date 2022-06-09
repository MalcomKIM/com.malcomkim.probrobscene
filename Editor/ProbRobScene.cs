using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;


namespace Unity.Robotics.UrdfImporter.Editor
{
	public class ProbRobScene : EditorWindow
	{
		// Absolute package paths
		string ABS_PACKAGE_PATH;
		string ABS_PACKAGE_EDITOR_PATH;
		
		// Relative package paths
		string REL_PACKAGE_PATH="Packages\\com.malcomkim.probrobscene";
		string REL_PACKAGE_MATERIALS_PATH;
		
		// INPUT: Prefabs
		string prefabs_folder_path = ""; // Example: Assets/Prefabs/Part1
		
		// INPUT: Robot
		GameObject Models;	// hold all the models in the world
		Object UrdfObject;	// urdf input placeholder
		string RobotName;	// robot name with extension
		string _RobotName;	// robot name without extension
		ImportSettings settings = new ImportSettings();
		float progress = 0.0f; // Robot Loading Progress, used for delay
		
		// INPUT: Python Path
		string PythonPath="";
		
		// INPUT: prs file
		TextAsset textPRS;
		

		[MenuItem("Tests/ProbRobScene")]
		public static void ShowWindow()
		{
			GetWindow<ProbRobScene>("ProbRobScene");
		}
		
	 
		private void OnInspectorUpdate()
		{
			Repaint();
		}
		
		public void OnEnable()
		{
			ABS_PACKAGE_PATH = getPackageAbsPath();
			ABS_PACKAGE_EDITOR_PATH = ABS_PACKAGE_PATH + "\\Editor";
			REL_PACKAGE_MATERIALS_PATH= REL_PACKAGE_PATH + "\\Materials";
			Debug.Log(ABS_PACKAGE_PATH);
			Debug.Log(ABS_PACKAGE_EDITOR_PATH);
			Debug.Log(REL_PACKAGE_MATERIALS_PATH);
		}

		void OnGUI ()
		{
			// GUI Style
			GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
			{
				alignment = TextAnchor.MiddleLeft,
				fontSize = 13
			};
			 
			GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButtonRight) { fixedWidth = 75 };
			
			
			//=============== Prefab part ===============
			GUILayout.Space(10);
			GUILayout.Label("Prefabs", titleStyle);
			prefabs_folder_path = EditorGUILayout.TextField("Prefabs path: ", prefabs_folder_path);
			
			
			//=============== Robot part ===============
			GUILayout.Space(10);
			GUILayout.Label("Robot", titleStyle);
			UrdfObject = EditorGUILayout.ObjectField("Input .urdf file", UrdfObject ,typeof(Object),true);
			
			//=============== Scene part ===============
			GUILayout.Space(10);
			GUILayout.Label("Scene Specifier", titleStyle);
			string defaultPlaceHolder = Utils.FindPython();
			PythonPath = EditorGUILayout.TextField("python.exe path: ", PythonPath == string.Empty ? defaultPlaceHolder : PythonPath);
			
			textPRS = EditorGUILayout.ObjectField("Input .prs file", textPRS ,typeof(TextAsset),true) as TextAsset;
			
			
			GUILayout.Space(20);
			if (GUILayout.Button("Generate Scene"))
			{
				Models = new GameObject("Models");
				
				//=============== Load prefabs ===============
				if (prefabs_folder_path != ""){
					//string BASE_PROJECT_PATH = Directory.GetCurrentDirectory();
					//Debug.Log(BASE_PROJECT_PATH);
					//string PREFABS_FOLDER_PATH =  prefabs_folder_path;
					Debug.Log(prefabs_folder_path);
					DirectoryInfo d = new DirectoryInfo(@prefabs_folder_path);
					FileInfo[] Files = d.GetFiles("*.prefab");
					
					foreach(FileInfo file in Files)
					{
						string _prefab = System.IO.Path.GetFileNameWithoutExtension(file.Name);
						string prefab = prefabs_folder_path + "\\"+ _prefab + ".prefab";
						Debug.Log(prefab);
						GameObject go = Instantiate(AssetDatabase.LoadAssetAtPath(prefab,typeof(GameObject))) as GameObject;
						
						if(go.transform.rotation == Quaternion.identity){
							go.name= _prefab;
							go.transform.parent = Models.transform;
						}
						else{
							GameObject wrapper = new GameObject(_prefab);
							go.name= _prefab+"_prefab";
							
							Bounds bounds = Utils.CaptureBounds(go);
							wrapper.transform.position = bounds.center;
							wrapper.transform.localScale = Utils.getBoundsSize(bounds);
							
							go.transform.parent = wrapper.transform;
							wrapper.transform.parent = Models.transform;
						}
						
					}
				}
				
				//=============== Load Robot ===============
				
				if (UrdfObject != null) {
					RobotName = AssetDatabase.GetAssetPath(UrdfObject);
					
					_RobotName = System.IO.Path.GetFileNameWithoutExtension(RobotName);
				
					if (RobotName != ""){
						EditorCoroutineUtility.StartCoroutine(UrdfRobotExtensions.Create(RobotName, settings,false), this);
					}
				}
				
				//=============== Generate Model.prs ===============
				EditorCoroutineUtility.StartCoroutine(BuildModels(),this);
				
			}
			
			
		}

		IEnumerator BuildModels()
		{
			List<ModelItem> ModelItems = new List<ModelItem>();
			// Wait until .urdf is loaded
			while (progress<1){
				progress = (settings.totalLinks == 0) ? 0 : ((float)settings.linksLoaded / (float)settings.totalLinks);
				// Debug.Log(progress);
				yield return null;
			}
			
			// Add robot into the scene as a child node
			GameObject robot = GameObject.Find(_RobotName);
			robot.transform.parent = Models.transform;
			
			// reset progress
			progress = 0.0f;
			
			// Get boundaries
			foreach (Transform child in Models.transform)
			{
				// Debug.Log(child.name);
				GameObject go = GameObject.Find(Models.transform.name + "/" + child.name);

				Bounds bounds = Utils.CaptureBounds(go);
				Vector3 size = Utils.getBoundsSize(bounds);

				ModelItem mi = new ModelItem(go, size.x, size.y, size.z, child.name);
				ModelItems.Add(mi);
			}
			
			// Generate Model.prs
			string PrsPath = AssetDatabase.GetAssetPath(textPRS);
			string save_path = Path.GetDirectoryName(Path.GetFullPath(PrsPath));
			Utils.CreateModelPrs(ModelItems, save_path);
			
			// Start building the scene from give .prs
			EditorCoroutineUtility.StartCoroutine(BuildScene(),this);
		}
		
		
		IEnumerator BuildScene(){
			
			string PrsPath = AssetDatabase.GetAssetPath(textPRS);
			string PrsName = System.IO.Path.GetFileNameWithoutExtension(PrsPath);	
			string json_result = Utils.python(PythonPath, ABS_PACKAGE_EDITOR_PATH, PrsPath);
			
			Debug.Log(PythonPath);
			Debug.Log(PrsPath);
			
			//Debug.Log(PrsPath);
			Debug.Log(json_result);
			
			// Get information of items in the scene
			SceneItemList SceneItems = JsonUtility.FromJson<SceneItemList>(json_result);
			
			// Transparent Red boxes
			GameObject References = new GameObject("References"); 
			Material TransparentRed = (Material)AssetDatabase.LoadAssetAtPath(REL_PACKAGE_MATERIALS_PATH + "\\TransparentRed.mat", typeof(Material));
	 
			foreach (SceneItem o in SceneItems.objects)
			{	
				GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
				cube.name = o.model_name;
				cube.transform.localScale = new Vector3 (o.size_x, o.size_y, o.size_z);
				cube.transform.position = new Vector3(o.position_x, o.position_y, o.position_z);
				cube.transform.rotation = Quaternion.Euler(new Vector3(o.rotation_x * Mathf.Rad2Deg, o.rotation_y * Mathf.Rad2Deg, o.rotation_z * Mathf.Rad2Deg));
				cube.GetComponent<Renderer>().material = TransparentRed;
				cube.transform.parent = References.transform;
			}
			
			// Build the scene
			GameObject Scene = new GameObject(PrsName);
			foreach (SceneItem o in SceneItems.objects)
			{	
				// Clone from models
				GameObject clone = Instantiate(GameObject.Find(Models.transform.name + "/" + o.model_name));
				clone.name = o.model_name;
				
				
				Bounds bounds = Utils.CaptureBounds(clone);
				
				// Calculate scale from size
				Vector3 exp_size = new Vector3(o.size_x, o.size_y, o.size_z);
				Vector3 ori_size = Utils.getBoundsSize(bounds);
				
				Vector3 scale = Utils.CalculateScale(exp_size, ori_size);
				clone.transform.localScale = Vector3.Scale(clone.transform.localScale, scale);
				
				// Calculate displacement
				bounds = Utils.CaptureBounds(clone);
				Vector3 center = bounds.center;
				clone.transform.position += new Vector3(o.position_x, o.position_y, o.position_z) - center;
				
				
				clone.transform.Rotate(o.rotation_x * Mathf.Rad2Deg, o.rotation_y * Mathf.Rad2Deg, o.rotation_z * Mathf.Rad2Deg);
				
				
				clone.transform.parent = Scene.transform;
			}
			
			// Deactivate the Gameobject which contains all models
			Models.SetActive(false);
			//References.SetActive(false);
			
			yield return new WaitForSeconds(0.1f);
		}
		
		string getPackageAbsPath(){
			MonoScript ms = MonoScript.FromScriptableObject(this);
			string m_ScriptFilePath = AssetDatabase.GetAssetPath(ms);
			string editorPath = Path.GetDirectoryName(Path.GetFullPath(m_ScriptFilePath));
			string packagePath = Directory.GetParent(editorPath).FullName;
			return packagePath;
		}

	}
}
