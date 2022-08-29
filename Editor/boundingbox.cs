using UnityEngine;
using UnityEditor;
using System.Collections;


namespace MalcomKim.ProbRobScene.Editor{
	class boudingbox : EditorWindow
	{
	  //private float radius = 0;
	  private Bounds bounds;
	  private float width;
	  private float height;
	  private float depth;

	  [MenuItem("ProbRobScene/Capture Bounds")]

	  public static void ShowWindow()
	  {
		EditorWindow.GetWindow(typeof(boudingbox));
	  }

	  void OnGUI()
	  {
		GUILayout.Label("Select a mesh in the Hierarchy view \nand click 'Capture Bounds'\n");
		EditorGUILayout.BeginVertical();
		bounds = EditorGUILayout.BoundsField("Renderer bounds:", bounds);
		width = EditorGUILayout.FloatField("    Width (X)", bounds.extents.x * 2);
		height = EditorGUILayout.FloatField("    Height (Y)", bounds.extents.y * 2);
		depth = EditorGUILayout.FloatField("    Depth (Z)", bounds.extents.z * 2);

		if (GUILayout.Button("Capture Bounds") ) {
		  //Renderer ren = Selection.activeTransform.GetComponentInChildren<Renderer>();
		  Renderer[] renderers = Selection.activeTransform.GetComponentsInChildren<Renderer>();
		  bounds = new Bounds();
		  if (renderers.Length != 0)
		  {
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
		  
		}
		EditorGUILayout.EndVertical();

	  }
	}
}