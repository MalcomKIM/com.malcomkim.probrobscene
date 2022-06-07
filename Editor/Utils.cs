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
	private static float MIN_VALUE = 0.000001f;
	
	
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
	
	public static Vector3 getBoundsSize(Bounds bounds){
		float width = bounds.extents.x * 2;
		float height = bounds.extents.y * 2;
		float length = bounds.extents.z * 2;
		
		return new Vector3(width, height, length);
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
	
	public static Vector3 CalculateScale(Vector3 exp_size, Vector3 ori_size){
		ori_size.x = Math.Max(ori_size.x, MIN_VALUE);
		ori_size.y = Math.Max(ori_size.y, MIN_VALUE);
		ori_size.z = Math.Max(ori_size.z, MIN_VALUE);
		
		return new Vector3(exp_size.x / ori_size.x, exp_size.y / ori_size.y, exp_size.z / ori_size.z);
		
	}

	
}
