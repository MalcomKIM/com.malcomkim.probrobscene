using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public class DebugHelper : EditorWindow
{
    [MenuItem("ProbRobScene/Debug Helper")]
    static void Init()
    {
        EditorWindow window = GetWindow(typeof(DebugHelper));
        window.Show();
    }
	
	
	void OnGUI()
    {
		
		
		if (GUILayout.Button("Execute"))
		{
			//"/C python -c \"import sys; print(sys.executable)\""
			string OSPlatform = Utils.GetOperatingSystem();
			Debug.Log(Utils.cmd(OSPlatform, "/C python -c \"import sys; print(sys.executable)\""));
		}
		
    }
	
	private void OnInspectorUpdate()
	{
		Repaint(); 
	}
}
