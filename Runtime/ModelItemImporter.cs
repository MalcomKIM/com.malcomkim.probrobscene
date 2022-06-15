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
		private static string MODELS_PARENT = "Models";
		
		public static void ImportPrefabs(string k_PrefabDirectory, string k_PrefabSuffix){
			GameObject Models = GameObject.Find(MODELS_PARENT);
			
			if (k_PrefabDirectory != ""){
				DirectoryInfo d = new DirectoryInfo(@k_PrefabDirectory);
				FileInfo[] Files = d.GetFiles("*"+ k_PrefabSuffix);
				
				foreach(FileInfo file in Files)
				{
					string _prefab = System.IO.Path.GetFileNameWithoutExtension(file.Name);
					string prefab = k_PrefabDirectory + "/"+ _prefab + k_PrefabSuffix;
					Debug.Log(prefab);
					GameObject go = Object.Instantiate(AssetDatabase.LoadAssetAtPath(prefab,typeof(GameObject))) as GameObject;
					
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
		
		public static void ImportRobot(Object UrdfObject, 
									float k_ControllerStiffness, 
									float k_ControllerDamping,
									float k_ControllerForceLimit, 
									float k_ControllerSpeed,
									float k_ControllerAcceleration,
									string k_BaseLinkName){
			GameObject Models = GameObject.Find(MODELS_PARENT);
			
			if (UrdfObject != null) {
				ImportSettings settings = new ImportSettings();
				string RobotName = AssetDatabase.GetAssetPath(UrdfObject);
				
				string _RobotName = System.IO.Path.GetFileNameWithoutExtension(RobotName);
			
				if (RobotName != ""){
					var robotImporter = UrdfRobotExtensions.Create(RobotName, settings);
					while (robotImporter.MoveNext()) { }
					
					// Add robot into the scene as a child node
					GameObject robot = GameObject.Find(_RobotName);
					robot.transform.parent = Models.transform;
					
					var controller = robot.GetComponent<Controller>();
					controller.stiffness = k_ControllerStiffness;
					controller.damping = k_ControllerDamping;
					controller.forceLimit = k_ControllerForceLimit;
					controller.speed = k_ControllerSpeed;
					controller.acceleration = k_ControllerAcceleration;
					GameObject.Find(k_BaseLinkName).GetComponent<ArticulationBody>().immovable = true;
				}
			}
			
		}
		
	}
}