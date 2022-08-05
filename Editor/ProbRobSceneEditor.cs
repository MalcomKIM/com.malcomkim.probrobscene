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

using MalcomKim.ProbRobScene;

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;


namespace MalcomKim.ProbRobScene.Editor{
	public class ProbRobSceneEditor : EditorWindow
	{
		// Absolute package paths
		string ABS_PACKAGE_RUNTIME_PATH;
		
		// Relative package paths
		string REL_PACKAGE_PATH="Packages/com.malcomkim.probrobscene";
		string REL_PACKAGE_MATERIALS_PATH;
		
		// INPUT: Prefabs
		string k_PrefabDirectory = ""; // Example: Assets/Prefabs/Part1
		string k_PrefabSuffix = ".prefab";
		
		// INPUT: Robot
		Object UrdfObject;	// urdf input placeholder
		string RobotName;	// robot name with extension
		string _RobotName;	// robot name without extension
		
		bool advancedSetting = false;
		float k_ControllerAcceleration = 10;
		float k_ControllerDamping = 100;
		float k_ControllerForceLimit = 1000;
		float k_ControllerSpeed = 30;
		float k_ControllerStiffness = 10000;
		bool immovable = true;
		string k_BaseLinkName = "world/base_link";
		
		
		// INPUT: Python Path
		string PythonPath="";
		string defaultPlaceHolder = "";
		
		// INPUT: prs file
		TextAsset textPRS;
		
		GameObject Models;	// hold all the models in the world
		GameObject RealScene;
		bool debugMode = false;
		string DebugMaterialPath;
		

		[MenuItem("ProbRobScene/ProbRobScene")]
		public static void ShowWindow()
		{
			GetWindow<ProbRobSceneEditor>("ProbRobScene");
		}
		
	 
		private void OnInspectorUpdate()
		{
			Repaint();
		}
		
		public void OnEnable()
		{
			ABS_PACKAGE_RUNTIME_PATH = OsUtils.getPackageRuntimeAbsPath();
			if(! Directory.Exists(ABS_PACKAGE_RUNTIME_PATH)){
				ABS_PACKAGE_RUNTIME_PATH = getPackageAbsPath()+ "/Runtime";
			}
			
			REL_PACKAGE_MATERIALS_PATH= REL_PACKAGE_PATH + "/Materials";
			Debug.Log(ABS_PACKAGE_RUNTIME_PATH);
			Debug.Log(REL_PACKAGE_MATERIALS_PATH);
			
			defaultPlaceHolder = OsUtils.FindPython();
			Debug.Log(defaultPlaceHolder);
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
			k_PrefabDirectory = EditorGUILayout.TextField("Prefabs path: ", k_PrefabDirectory);
			
			
			//=============== Robot part ===============
			GUILayout.Space(10);
			GUILayout.Label("Robot", titleStyle);
			UrdfObject = EditorGUILayout.ObjectField("Input .urdf file", UrdfObject ,typeof(Object),true);
			advancedSetting = EditorGUILayout.Toggle("Advanced Settings", advancedSetting);
			
			if(advancedSetting){
				k_ControllerStiffness = EditorGUILayout.FloatField("Stiffness", k_ControllerStiffness);
				k_ControllerDamping = EditorGUILayout.FloatField("Damping", k_ControllerDamping);
				k_ControllerForceLimit = EditorGUILayout.FloatField("Force Limit", k_ControllerDamping);
				k_ControllerSpeed = EditorGUILayout.FloatField("Speed", k_ControllerSpeed);;
				k_ControllerAcceleration = EditorGUILayout.FloatField("Acceleration", k_ControllerAcceleration);
				immovable = EditorGUILayout.Toggle("Immovable", immovable);
				if(immovable){
					k_BaseLinkName = EditorGUILayout.TextField("Base Link Name", k_BaseLinkName);;
				}
			}
			
			
			//=============== Scene part ===============
			GUILayout.Space(10);
			GUILayout.Label("Scene Specifier", titleStyle);
			
			PythonPath = EditorGUILayout.TextField("python path: ", (PythonPath == string.Empty && defaultPlaceHolder != null) ? defaultPlaceHolder : PythonPath);
			
			textPRS = EditorGUILayout.ObjectField("Input .prs file", textPRS ,typeof(TextAsset),true) as TextAsset;
			
			GUILayout.Space(10);
			
			debugMode = EditorGUILayout.Toggle("Debug Mode", debugMode);
			
			GUILayout.Space(10);
			if (GUILayout.Button("Build Scene"))
			{	
				
				if (!Directory.Exists(k_PrefabDirectory)){
					Debug.LogError("Prefab path not found or missing");
					return;
				}
				
				if(!File.Exists(PythonPath)){
					Debug.LogError("Python path not found or missing");
					return;
				}
				
				if(textPRS == null){
					Debug.LogError(".prs file not found or missing");
					return;
				}
				
				
				
				Models = new GameObject("Models");
				//=============== Load prefabs ===============
				ModelItemImporter.ImportPrefabs(k_PrefabDirectory, k_PrefabSuffix, Models);
				
				//=============== Load Robot ===============
				RobotSetting rs = new RobotSetting(k_ControllerStiffness,
												k_ControllerDamping,
												k_ControllerForceLimit,
												k_ControllerSpeed,
												k_ControllerAcceleration,
												immovable,
												k_BaseLinkName);
												
				ModelItemImporter.ImportRobot(UrdfObject, Models);
				
				//=============== Generate Model.prs ===============
				SceneBuilder.BuildModels(textPRS, Models);
				
				//=============== Setup the Scene ===============
				if(debugMode){
					RealScene = SceneBuilder.BuildScene(
											textPRS,
											ABS_PACKAGE_RUNTIME_PATH,
											PythonPath,
											"RealScene",
											"DebugScene",
											rs,
											Models);
				}
				else{
					RealScene = SceneBuilder.BuildScene(
											textPRS,
											ABS_PACKAGE_RUNTIME_PATH,
											PythonPath,
											"RealScene",
											rs,
											Models);
				}
				
				
							
				// Deactivate the Gameobject which contains all models
				Models.SetActive(false);
				
			}
			
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
