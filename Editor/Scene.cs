using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneItem
{
	public string model_name;
	public bool requireVisible;
	public bool allowCollisions;
	public float visibleDistance;
	public string color;
	
	public float position_x;
	public float position_y;
	public float position_z;
	
	public float rotation_x;
	public float rotation_y;
	public float rotation_z;
	
	public float scale_x;
	public float scale_y;
	public float scale_z;
}

[System.Serializable]
public class SceneItemList
{
	public SceneItem[] objects;
}
