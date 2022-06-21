using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalcomKim.ProbRobScene{
	public class RobotController
	{
		public GameObject robot;
		public int k_NumRobotJoints;
		public float k_JointAssignmentWait;
		public string[] LinkNames;
		public ArticulationBody[] m_JointArticulationBodies;
		
		public RobotController(GameObject robot, int k_NumRobotJoints, float k_JointAssignmentWait, string[] LinkNames){
			this.robot = robot;
			this.k_NumRobotJoints = k_NumRobotJoints;
			this.k_JointAssignmentWait = k_JointAssignmentWait;
			this.LinkNames = LinkNames;
			
			m_JointArticulationBodies = new ArticulationBody[k_NumRobotJoints];
			var linkName = string.Empty;
			for (var i = 0; i < k_NumRobotJoints; i++)
			{
				linkName += LinkNames[i];
				m_JointArticulationBodies[i] = robot.transform.Find(linkName).GetComponent<ArticulationBody>();
			}
		}
		
		private bool ResetRobotToDefaultPosition()
		{
			bool isRotationFinished = true;
			var rotationSpeed = 180f;

			for (int i = 0; i < k_NumRobotJoints; i++)
			{
				var tempXDrive = m_JointArticulationBodies[i].xDrive;
				float currentRotation = tempXDrive.target;

				float rotationChange = rotationSpeed * Time.fixedDeltaTime;

				if (currentRotation > 0f) rotationChange *= -1;

				if (Mathf.Abs(currentRotation) < rotationChange)
					rotationChange = 0;
				else
					isRotationFinished = false;

				// the new xDrive target is the currentRotation summed with the desired change
				float rotationGoal = currentRotation + rotationChange;
				tempXDrive.target = rotationGoal;
				m_JointArticulationBodies[i].xDrive = tempXDrive;
			}
			return isRotationFinished;
		}
		
		public IEnumerator MoveToInitialPosition()
		{
			bool isRotationFinished = false;
			while (!isRotationFinished)
			{
				isRotationFinished = ResetRobotToDefaultPosition();
				yield return new WaitForSeconds(k_JointAssignmentWait);
			}
		}
		
		
	}
}