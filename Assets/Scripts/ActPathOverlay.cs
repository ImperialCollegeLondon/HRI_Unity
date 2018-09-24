using System;
using System.Collections.Generic;
using ROSBridgeLib;
using ROSBridgeLib.geometry_msgs;
using ROSBridgeLib.nav_msgs;
using SimpleJSON;
using UnityEngine;
//using UnityEngine.UI;

/*Class to hold the subscriber info for the tip pose */
public class ActPathOverlay : ROSBridgeSubscriber {

	//Path information
	public static Vector3[] overlayActRosPos;
	public static Quaternion[] overlayActRosQuat;
	public static bool overlayActRosReceived = false;

	public new static string GetMessageTopic() {
		return "/actualOverlayUnity";
	}  

	public new static string GetMessageType() {
		return "nav_msgs/Path";
	}

	public new static ROSBridgeMsg ParseMessage(JSONNode msg) {
		return new PathMsg(msg);
	}

	public static Quaternion ExtractRotation(Matrix4x4 matrix)
	{
		Vector3 forward;
		forward.x = matrix.m02;
		forward.y = matrix.m12;
		forward.z = matrix.m22;

		Vector3 upwards;
		upwards.x = matrix.m01;
		upwards.y = matrix.m11;
		upwards.z = matrix.m21;

		return Quaternion.LookRotation(forward, upwards);
	}

	public static Vector3 ExtractPosition(Matrix4x4 matrix)
	{
		Vector3 position;
		position.x = matrix.m03;
		position.y = matrix.m13;
		position.z = matrix.m23;
		return position;
	}

	public static Vector3 ExtractScale(Matrix4x4 matrix)
	{
		Vector3 scale;
		scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
		scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
		scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
		return scale;
	}

	public static void CallBack(ROSBridgeMsg msg) {

		//Handle the message callback and save the path
		//Debug.Log (GetMessageTopic () + " received");
		overlayActRosReceived = true;

		//Declare variables
		PathMsg pathmsg_ = (PathMsg)msg;

		//Initialise length based on number of points in the path
		overlayActRosPos = new Vector3[pathmsg_.getPathLength ()];
		overlayActRosQuat = new Quaternion[pathmsg_.getPathLength ()];

		//Read in the array of path positions and orientations
		for (var i = 0; i < pathmsg_.getPathLength (); i++) {
			var px = pathmsg_._poses [i].getPoseMsg ().GetPositionMsg ().GetX ();
			var py = pathmsg_._poses [i].getPoseMsg ().GetPositionMsg ().GetY ();
			var pz = pathmsg_._poses [i].getPoseMsg ().GetPositionMsg ().GetZ ();
			var qw = pathmsg_._poses [i].getPoseMsg ().GetOrientationMsg ().GetW ();
			var qx = pathmsg_._poses [i].getPoseMsg ().GetOrientationMsg ().GetX ();
			var qy = pathmsg_._poses [i].getPoseMsg ().GetOrientationMsg ().GetY ();
			var qz = pathmsg_._poses [i].getPoseMsg ().GetOrientationMsg ().GetZ ();
			overlayActRosPos [i] = new Vector3 (px, py, pz);
			overlayActRosQuat [i] = new Quaternion (qx, qy, qz, qw);
			//Debug.Log ("Overlay Actual Path " + i + "Position: " + wayPointPos [i] + "Quaternion: " + wayPointRot [i]);
		}
	}
}
