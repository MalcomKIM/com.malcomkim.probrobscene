using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalcomKim.ProbRobScene{
	public class ModelItem
	{
		private float MIN_VALUE = 0.000001f;
		
		private GameObject obj;
		private float width;
		private float height;
		private float length;
		private string model_name;
		
		public ModelItem(GameObject obj, float width, float height, float length, string model_name){
			this.obj = obj;
			this.width = Math.Max(width, MIN_VALUE);
			this.height = Math.Max(height, MIN_VALUE);
			this.length = Math.Max(length, MIN_VALUE);
			this.model_name = model_name;
		}
		
		public string toString(){
			return string.Format(
			"\nclass {0}:\n\twidth: {1:0.000000}\n\tlength: {3:0.000000}\n\theight: {2:0.000000}\n\tmodel_name: \"{4}\"\n",
			model_name, width, height, length, model_name
			);
		}
	}
}
