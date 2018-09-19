using ROSBridgeLib;
using ROSBridgeLib.geometry_msgs;
using System.Collections;
using SimpleJSON;
using UnityEngine;

/*Class to hold the subscriber info for the tip pose */

public class TipPose : ROSBridgeSubscriber {
		
	public new static string GetMessageTopic() {
		return "/NDI_Aurora_sensor_blended";
	}  
	
	public new static string GetMessageType() {
		return "geometry_msgs/PoseStamped";
	}
	
	public new static ROSBridgeMsg ParseMessage(JSONNode msg) {
		return new PoseStampedMsg(msg);
	}
	
	public static void CallBack(ROSBridgeMsg msg) {

		//Process the received message
		//Debug.Log ("Tip Movement: " + GetMessageTopic () + " received");
		GameObject tip = GameObject.Find ("needle");
		if (tip == null)
			Debug.Log ("Can't find the needle in the scene");
		else {

			//Declare variables
			PoseStampedMsg pose = (PoseStampedMsg) msg;
			Vector3 globalStartPos;
			Quaternion globalStartQuat;

			//Determine the starting position of the needle
			GameObject startPoint = GameObject.Find ("insertionPoint");
			if (startPoint == null)
				Debug.Log ("Can't find the insertion point in the scene");
			else {

				//Determine starting transformation of the needle
				globalStartPos = startPoint.transform.position;
				globalStartQuat = startPoint.transform.rotation;

				//Quaternion idenQ = new Quaternion (0,0,0,1);
				//Vector3 scale = new Vector3 (1, 1, 1);

				//Read in needle transformation from ROS
				Vector3 deltaPosROS = new Vector3 (pose.getPoseMsg().GetPositionMsg ().GetX (), pose.getPoseMsg().GetPositionMsg ().GetY (), pose.getPoseMsg().GetPositionMsg ().GetZ ());
				//Quaternion deltaQuatROS = new Quaternion (pose.getPoseMsg().GetOrientationMsg ().GetX, pose.getPoseMsg().GetOrientationMsg ().GetY, pose.getPoseMsg().GetOrientationMsg ().GetZ, pose.getPoseMsg().GetOrientationMsg ().GetW);
				Quaternion deltaQuatROS = new Quaternion(0,0,0,1);

				//Convert ROS RH quaternion to ROS LH quaternion by mirroring the Z axis, and translation by negating z
				Vector3 deltaPosROSLH = new Vector3 (deltaPosROS.x, deltaPosROS.y, -1 * deltaPosROS.z);
				Quaternion deltaQuatROSLH = new Quaternion(-1*deltaQuatROS.x, -1*deltaQuatROS.y, deltaQuatROS.z, deltaQuatROS.w);

				//Define transformation between ROS LH and Unity LH Frames
				//Done by examination ROS(RH)[X Y Z] -> UNITY(LH)[Y -Z X], 
				Quaternion ros2unityQuat = Quaternion.Euler(0,-90,90);	//Order 
				Quaternion ros2unityQuatInverse = Quaternion.Inverse(ros2unityQuat);

				//Transform the needle in the local Unity LH frame
				Vector3 start2tipPos = ros2unityQuatInverse * (ros2unityQuat * deltaPosROSLH);
				Quaternion start2tipQuat = deltaQuatROSLH * ros2unityQuatInverse;

				//Change to global frame
				Vector3 moveTip = globalStartPos + (globalStartQuat * start2tipPos);	//Start2tipPos transferred to global frame first
				Quaternion rotateTip = globalStartQuat * start2tipQuat;					//Start2tipQuat is locally applied with this order of ops

				//Move the needle
				tip.transform.rotation = rotateTip;
				tip.transform.position = moveTip;

			}
		}
	}
}

