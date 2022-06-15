using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace MalcomKim.ProbRobScene{
	public static class SizeHelper
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
		
		public static Vector3 CalculateScale(Vector3 exp_size, Vector3 ori_size){
			ori_size.x = Math.Max(ori_size.x, MIN_VALUE);
			ori_size.y = Math.Max(ori_size.y, MIN_VALUE);
			ori_size.z = Math.Max(ori_size.z, MIN_VALUE);
			
			return new Vector3(exp_size.x / ori_size.x, exp_size.y / ori_size.y, exp_size.z / ori_size.z);
			
		}
	}
}