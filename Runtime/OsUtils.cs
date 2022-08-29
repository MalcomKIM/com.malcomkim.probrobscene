using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace MalcomKim.ProbRobScene{
	public static class OsUtils
	{
		
		
		// Write to items' information to Model.prs
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
		
		// Check whether the gameObject exists
		public static bool GameObjectExits(string GameObjectName){
			if (GameObject.Find(GameObjectName) != null){
				return true;
			}
			else{
				return false;
			}
		}
		
		// Get Inactive gameObject
		public static GameObject FindInActiveObjectByName(string name)
		{
			Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
			for (int i = 0; i < objs.Length; i++)
			{
				if (objs[i].hideFlags == HideFlags.None)
				{
					if (objs[i].name == name)
					{
						return objs[i].gameObject;
					}
				}
			}
			return null;
		}
		
		// Destroy GameObject
		public static T SafeDestroyGameObject<T>(T obj) where T : Object
		{
			if (Application.isEditor)
				Object.DestroyImmediate(obj);
			else
				Object.Destroy(obj);

			return null;
		}
		
 
		// Find python executable on system
		public static string FindPython(){
			string OSPlatform = GetOperatingSystem();
			string PythonPath = "";
			try{
				if(OSPlatform == "Windows"){
					PythonPath = cmd(OSPlatform, "/C python -c \"import sys; print(sys.executable)\"");
				}
				else if(OSPlatform == "OSX" || OSPlatform == "Linux"){
					PythonPath = cmd(OSPlatform, "which python3");
				}
				else{
					return null;
				}
			}
			catch(Exception e){
				Debug.Log("Exception Message: " + e.Message);
				return null;
			}
			
			if(File.Exists(PythonPath)){
				return PythonPath;
			}
			
			return null;
		}
		
		// start python process
		public static string python(string python_path, string script_path, string prs_path){
			string BASE_PROJECT_PATH = Directory.GetCurrentDirectory();
			
			string SCRIPT_PATH = "\"" + script_path + "/runScenarioRaw.py" + "\"";
			string SCENARIO_PATH = "\"" + BASE_PROJECT_PATH + "/"+ prs_path  + "\"";
			
			Debug.Log(SCRIPT_PATH);
			Debug.Log(SCENARIO_PATH);
			
			Process process = new Process();
			
			process = Process.Start(new ProcessStartInfo
								{
									WorkingDirectory = BASE_PROJECT_PATH,
									FileName= python_path.Replace(@"\",@"\\"),
									Arguments = SCRIPT_PATH + " "+ SCENARIO_PATH + " 1",
									CreateNoWindow = true,
									UseShellExecute = false,
									RedirectStandardOutput = true
								});     
								
			string output = process.StandardOutput.ReadToEnd();
			
			string toBeSearched = "json_objects:";
			string json_result = "";
			int ix = output.IndexOf(toBeSearched);
			if (ix != -1) 
			{
				json_result = output.Substring(ix + toBeSearched.Length);
			}
			
			process.WaitForExit();
			process.Close();
			
			return json_result;
		}	
		
		// start cmd process
		public static string cmd(string OSPlatform, string args){
			string filename = "";
			
			if(OSPlatform == "Windows"){
				filename = "cmd.exe";
			}
			else if(OSPlatform == "OSX" || OSPlatform == "Linux"){
				filename = "/bin/bash";
			}
			else{
				return null;
			}
			
			Process process = new Process();
			process = Process.Start(new ProcessStartInfo
								{
									FileName= filename,
									Arguments = args,
									CreateNoWindow = true,
									UseShellExecute = false,
									RedirectStandardOutput = true
								}); 
			
			string output = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			process.Close();
			
			
			using (var reader = new StringReader(output))
			{
				output = reader.ReadLine();
			}
			
			return output;
		}
		
		// get the name of os
		public static string GetOperatingSystem()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				return "OSX";
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				return "Linux";
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return "Windows";
			}

			return null;
		}
		
		// get the installing path of the package
		public static string getPackageAbsPath(){
			return Path.GetFullPath("Packages/com.malcomkim.probrobscene");
		}
		
		// get the runtime path in the package
		public static string getPackageRuntimeAbsPath(){
			return getPackageAbsPath() + "/Runtime";
		}

		// get the editor path in the package
		public static string getPackageEditorAbsPath(){
			return getPackageAbsPath() + "/Editor";
		}
		


		
		
	}
}