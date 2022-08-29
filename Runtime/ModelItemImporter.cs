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

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;



namespace MalcomKim.ProbRobScene{
	public static class ModelItemImporter
	{
		
		// Import prefabs from the given folder
		public static void ImportPrefabs(string k_PrefabDirectory, string k_PrefabSuffix, GameObject Models){
			
			if (k_PrefabDirectory != ""){
				
				DirectoryInfo d = new DirectoryInfo(@k_PrefabDirectory);
				FileInfo[] Files = d.GetFiles("*"+ k_PrefabSuffix);
				
				// iterate all the prefabs
				foreach(FileInfo file in Files)
				{
					string _prefab = System.IO.Path.GetFileNameWithoutExtension(file.Name);
					string prefab = k_PrefabDirectory + "/"+ _prefab + k_PrefabSuffix;
					Debug.Log(prefab);
					GameObject go = Object.Instantiate(AssetDatabase.LoadAssetAtPath(prefab,typeof(GameObject))) as GameObject;
					
					// default rotation check
					if(go.transform.rotation == Quaternion.identity){
						go.name= _prefab;
						go.transform.parent = Models.transform;
					}
					else{
						GameObject wrapper = new GameObject(_prefab);
						go.name= _prefab+"_prefab";
						
						Bounds bounds = SizeHelper.CaptureBounds(go);
						wrapper.transform.position = bounds.center;
						wrapper.transform.localScale = SizeHelper.getBoundsSize(bounds);
						
						go.transform.parent = wrapper.transform;
						wrapper.transform.parent = Models.transform;
					}
				}
			}
		}
		
		// import robot from drag-and-drop
		public static GameObject ImportRobot(Object UrdfObject, GameObject Models){
			GameObject robot = null;
			
			if (UrdfObject != null) {
				ImportSettings settings = new ImportSettings();
				string RobotName = AssetDatabase.GetAssetPath(UrdfObject);
				
				string _RobotName = System.IO.Path.GetFileNameWithoutExtension(RobotName);
			
				if (RobotName != ""){
					var robotImporter = UrdfRobotExtensions.Create(RobotName, settings);
					while (robotImporter.MoveNext()) { }
					
					// Add robot into the scene as a child node
					robot = GameObject.Find(_RobotName);
					robot.tag = "robot";
					robot.transform.parent = Models.transform;
					
				}
			}
			return robot;
		}
		
		// import robot from the path of urdf
		public static GameObject ImportRobot(string UrdfPath, GameObject Models){
			GameObject robot = null;
			
			if (UrdfPath != null) {
				ImportSettings settings = new ImportSettings();
				
				string _RobotName = System.IO.Path.GetFileNameWithoutExtension(UrdfPath);
			
				if (UrdfPath != ""){
					var robotImporter = UrdfRobotExtensions.Create(UrdfPath, settings);
					while (robotImporter.MoveNext()) { }
					
					// Add robot into the scene as a child node
					robot = GameObject.Find(_RobotName);
					robot.tag = "robot";
					robot.transform.parent = Models.transform;
					
				}
			}
			return robot;
		}
		
	}
}