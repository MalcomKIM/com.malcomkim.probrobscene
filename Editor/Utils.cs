using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public static class Utils
{
	// Capture Bounds (width, height, depth) of a object
	public static Bounds CaptureBounds(GameObject gobj){
		Renderer[] renderers = gobj.GetComponentsInChildren<Renderer>();
		Bounds bounds = new Bounds();
		if (renderers.Length != 0){
			if (renderers.Length == 1)
			{
			  bounds = renderers[0].bounds;
			} else
			{
			  bounds = renderers[0].bounds;
			  foreach (Renderer ren in renderers)
			  {
				bounds.Encapsulate(ren.bounds);
			  }
			}
		}
		
		return bounds;
	}
	
	
	
	// Write to items' information to Model.prs
	public static string CreateModelPrs(List<ModelItem> items){
		string model_prs = @"import math

width = 5
length = 5
height = 5
workspace = Cuboid(Vector3D(0, 0, height / 2.0), Vector3D(0,0,0), width, length, height)
";
		foreach(ModelItem i in items){
			model_prs = model_prs + i.toString();
		}
		
		string BASE_PROJECT_PATH = Directory.GetCurrentDirectory();
		File.WriteAllText(@BASE_PROJECT_PATH + "/Assets/Scenarios/model.prs", model_prs);
		//yield return new WaitUntil(() => File.Exists(@"E:/ProbRobScene/scenarios/mymodel.prs"));
		return model_prs;
	}
	
	
	public static bool GameObjectExits(string GameObjectName){
		if (GameObject.Find(GameObjectName) != null){
			return true;
		}
		else{
			return false;
		}
	}
	
	public static string FindPython(){
		var path = Environment.GetEnvironmentVariable("PATH");
		string pythonPath = null;
		foreach (var p in path.Split(new char[] { ';' })) { 
		  var fullPath = Path.Combine(p, "python.exe");
		  if (File.Exists(fullPath)) { 
			pythonPath = fullPath;
			break;
		  }
		}

		if (pythonPath == null) { 
			throw new Exception("Couldn't find python on %PATH%");
		} 
		
		return pythonPath;
	}
	
	
	public static string python(string python_path, string prs_path){
		string BASE_PROJECT_PATH = Directory.GetCurrentDirectory();
		string SCRIPT_PATH = BASE_PROJECT_PATH + "/Assets/Scripts/runScenarioRaw.py";
		string SCENARIO_PATH = BASE_PROJECT_PATH + "/"+ prs_path;
		
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

	
}
