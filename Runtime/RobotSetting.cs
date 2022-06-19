using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalcomKim.ProbRobScene{
	public class RobotSetting
	{
		public float k_ControllerStiffness;
		public float k_ControllerDamping;
		public float k_ControllerForceLimit;
		public float k_ControllerSpeed;
		public float k_ControllerAcceleration;
		public bool immovable;
		public string k_BaseLinkName;
		
		
		public RobotSetting(float k_ControllerStiffness,
							float k_ControllerDamping,
							float k_ControllerForceLimit,
							float k_ControllerSpeed,
							float k_ControllerAcceleration,
							bool immovable,
							string k_BaseLinkName){
			
			this.k_ControllerAcceleration = k_ControllerAcceleration;
			this.k_ControllerDamping = k_ControllerDamping;
			this.k_ControllerForceLimit = k_ControllerForceLimit;
			this.k_ControllerSpeed = k_ControllerSpeed;
			this.k_ControllerStiffness = k_ControllerStiffness;
			this.immovable = immovable;
			this.k_BaseLinkName = k_BaseLinkName;
		}
	}
}
