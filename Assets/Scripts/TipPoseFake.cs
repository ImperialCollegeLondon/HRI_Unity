using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TipPoseFake : MonoBehaviour {

	private float nextActionTime = 0.0f;
	public float period = 0.05f;
	private static int lineCounter = 0;
	private string[] linesPose;
	private Vector3[] posInput;
	private Quaternion[] quatInput;

	// Use this for initialization
	void Start () {

		//Check setup started
		Debug.Log ("Reading in fake tip pose information from file");

		//Read in position and orientation information from file
		TextAsset myPoseData = Resources.Load ("orientInfo") as TextAsset;
		string textFilePose = myPoseData.text;
		linesPose = textFilePose.Split ("\n" [0]); // gets all lines into separate strings

		//Set up sizes
		posInput = new Vector3[linesPose.Length - 1];
		quatInput = new Quaternion[linesPose.Length - 1];

		//Note: Last line is empty
		for (var i = 1; i < linesPose.Length-1; i++) {
			var pt = linesPose [i].Split ("," [0]); // gets parts of the line into separate strings
			float px = float.Parse (pt [0]);
			float py = float.Parse (pt [1]);
			float pz = float.Parse (pt [2]);
			float qw = float.Parse (pt [3]);
			float qx = float.Parse (pt [4]);
			float qy = float.Parse (pt [5]);
			float qz = float.Parse (pt [6]);
			posInput [i - 1] = new Vector3 (px, py, pz);
			quatInput [i - 1] = new Quaternion (qx, qy, qz, qw);
		}
	}
	
	// Update is called once per frame
	void Update () {
		GameObject tip = GameObject.Find ("needle");
		if (tip == null)
			Debug.Log ("Can't find the needle in the scene");
		else {

			if (Time.time > nextActionTime) {

				//Execute timely code
				nextActionTime = Time.time + period;  

				//Declare variables
				Vector3 globalStartPos;
				Quaternion globalStartQuat;

				//Determine the starting position of the needle
				GameObject startPoint = GameObject.Find ("insertionPoint");
				if (startPoint == null) {
					//Testing positions to see needle movement when no insertionPoint is defined
					Debug.Log("Insertion Point could not be found");
					//globalStartPos = new Vector3(0,100,-150);
					globalStartPos = new Vector3(0,0,0);
					globalStartQuat = new Quaternion(0,0,0,1);
				} else {
					//Set the starting point of the needle with the same orientation and position as the insertion point
					globalStartPos = startPoint.transform.position;
					globalStartQuat = startPoint.transform.rotation;
				}

				//Determine the transformation from the global reference frame to the starting point of the needle
				//Vector3 scale = new Vector3 (1, 1, 1);
				//Vector3 zeroPos = new Vector3 (0, 0, 0);
				//Matrix4x4 global2start = Matrix4x4.TRS (globalStartPos, globalStartQuat, scale);


				/*
				//TRANSLATIONS - WORKING
				//Hardcode the TRANSLATIONS between ROS needle frame and Unity needle frame, assuming
				// x(ROS) = y(UNITY)
				// y(ROS) = -z(UNITY)
				// z(ROS) = x(UNITY)

				//Read in position and orientation information from file
				//Vector3 deltaPosLocal = new Vector3 (pose.getPoseMsg ().GetPositionMsg ().GetZ (), pose.getPoseMsg ().GetPositionMsg ().GetX (), -1 * pose.getPoseMsg ().GetPositionMsg ().GetY ());
				//Vector3 deltaPosLocal = new Vector3 (posInput[lineCounter].z, posInput[lineCounter].x, -1*posInput[lineCounter].y);
				Vector3 deltaPosLocal = new Vector3 (posInput[lineCounter].x, posInput[lineCounter].y, -1*posInput[lineCounter].z);
				Quaternion deltaRotLocal = new Quaternion (0, 0, 0, 1);	//Assume no rotations

				Matrix4x4 start2tip = Matrix4x4.TRS (deltaPosLocal, deltaRotLocal, scale);

				Matrix4x4 needleGlobal = global2start * start2tip;
				Vector3 moveTip = new Vector3 (needleGlobal.m03, needleGlobal.m13, needleGlobal.m23);
				//Translate the needle
				tip.transform.position = moveTip;
				*/

				/*
				//Test the right handed ROS Quaternion
				Matrix4x4 ros2unityRotation = Matrix4x4.TRS(zeroPos, Quaternion.Euler(0,-90,-90), scale);
				Quaternion testQuat = Quaternion.LookRotation(ros2unityRotation.GetColumn(2), ros2unityRotation.GetColumn(1));
				Quaternion testQuat2 = Quaternion.Euler(0,-90,-90);
				Vector3 testEuler2 = testQuat2.eulerAngles;
				Debug.Log ("With Euler Angles -> X: 0, Y:-90, Z: -90");
				Debug.Log ("From Matrix -> X: " + testQuat.x + " Y: " + testQuat.y + " Z: " +testQuat.z + " W: " + testQuat.w);
				Debug.Log ("With Euler Angles -> X: " + testEuler2.x + ", Y: " + testEuler2.y + ", Z: " + testEuler2.z);
				Debug.Log ("From Quaternion -> X: " + testQuat2.x + ", Y: " + testQuat2.y + ", Z: " +testQuat2.z + ", W: " + testQuat2.w);

				Quaternion quatRHRos = new Quaternion (quatRH [lineCounter].x, quatRH [lineCounter].y, quatRH [lineCounter].z, quatRH [lineCounter].w);
				Quaternion quatLHRos = new Quaternion (quatRH [lineCounter].x, -1 * quatRH [lineCounter].y, -1 * quatRH [lineCounter].z, quatRH [lineCounter].w);
				Matrix4x4 ros2unityValues = Matrix4x4.TRS(zeroPos, quatLHRos, scale);
				Matrix4x4 ros2unity = ros2unityValues * ros2unityRotation;
				//Take quaternion from the Matrix4x4
				Quaternion quatLHUnity = Quaternion.LookRotation(ros2unity.GetColumn(2), ros2unity.GetColumn(1));
				//Rotate the needle
				//tip.transform.rotation = quatLHUnity;

				*/


				//Read in ROS RH translation and quaternion
				Vector3 deltaPosROS = new Vector3 (posInput[lineCounter].x, posInput[lineCounter].y, posInput[lineCounter].z);
				//Debug.Log ("Ros translation RH: ");
				//Debug.Log (deltaPosROS.ToString("F4"));
				Quaternion deltaQuatROS = new Quaternion (quatInput [lineCounter].x, quatInput [lineCounter].y, quatInput [lineCounter].z, quatInput [lineCounter].w);
				//Debug.Log ("Ros Quat LH: ");
				//Debug.Log (deltaQuatROS.ToString("F4") );
				//Vector3 deltaNoPos = new Vector3 (0, 0, 0);
				//Vector3 deltaNoScale = new Vector3 (1, 1, 1);
				//Quaternion deltaNoQuat = new Quaternion(0, 0, 0, 1);

//				//Determine flipping matrix for mirroring in Z to change to LH  frame
//				Vector3 deltaNoPos = new Vector3 (0, 0, 0);
//				Vector3 deltaFliPZScale = new Vector3 (1, 1, -1);
//				Matrix4x4 flipZ = Matrix4x4.TRS (deltaNoPos, deltaNoQuat, deltaFliPZScale);

				//Transform ROS RH to Unity RH
				//Quaternion ros2unityQuat = Quaternion.Euler(0,-90,90);
				//Matrix4x4 ros2unity = Matrix4x4.TRS (deltaNoPos, ros2unityQuat, deltaNoScale);
				//Vector3 scaleFlipZ = new Vector3 (1, 1, -1);

//				Matrix4x4 start2tip = flipZ * ros2unity * Matrix4x4.TRS (deltaPosROS, deltaNoQuat, deltaNoScale)  * flipZ;
//				Matrix4x4 needleGlobal = global2start * start2tip;
//				Vector3 moveTip = new Vector3 (needleGlobal.m03, needleGlobal.m13, needleGlobal.m23);
//				Quaternion rotateTip = Quaternion.LookRotation (needleGlobal.GetColumn (2), needleGlobal.GetColumn (1));

				//Move the needle
//				tip.transform.position = moveTip;
//				tip.transform.rotation = rotateTip;

				//Convert ROS RH quaternion to ROS LH quaternion by mirroring the Z axis, and translation by negating z
				Vector3 deltaPosROSLH = new Vector3 (deltaPosROS.x, deltaPosROS.y, -1 * deltaPosROS.z);
				Quaternion deltaQuatROSLH = new Quaternion(-1*deltaQuatROS.x, -1*deltaQuatROS.y, deltaQuatROS.z, deltaQuatROS.w);
				//Debug.Log ("Ros Quat LH: ");
				//Debug.Log (deltaQuatROSLH.ToString("F4"));
					
				//Done by examination ROS(LH)[X Y Z] -> UNITY(LH)[Y -Z X]
				//Quaternion ros2unityQuat = new Quaternion(-0.5f, -0.5f, 0.5f, 0.5f);
				Quaternion ros2unityQuat = Quaternion.Euler(0,-90,90);	//Order 
				Quaternion ros2unityQuatInverse = Quaternion.Inverse(ros2unityQuat);
				//Matrix4x4 ros2unity = Matrix4x4.TRS (deltaNoPos, ros2unityQuat, deltaNoScale);
//				Debug.Log ("Ros to Unity Conversion ");
//				Debug.Log (ros2unityQuat.ToString("F4"));
//				Debug.Log ("Ros to Unity Conversion Inverse");
//				Debug.Log (ros2unityQuatInverse.ToString("F4"));
	
				//Transform the needle in the local Unity LH frame
				Vector3 start2tipPos = ros2unityQuatInverse * (ros2unityQuat * deltaPosROSLH);
				Quaternion start2tipQuat = deltaQuatROSLH * ros2unityQuatInverse;
//				Quaternion rotX, rotY, rotZ;
//				if (lineCounter % 2 == 0) {
//					rotX = Quaternion.Euler (60, 0, 0);
//					rotY = Quaternion.Euler(0, 60, 0);
//					rotZ = Quaternion.Euler(0, 0, 60);
//				} else {
//					rotX = Quaternion.Euler (30, 0, 0);
//					rotY = Quaternion.Euler(0, 30, 0);
//					rotZ = Quaternion.Euler(0, 0, 30);
//				}

				//Change to global frame
				Vector3 moveTip = globalStartPos + (globalStartQuat * start2tipPos);	//Start2tipPos transferred to global frame first
				Quaternion rotateTip = globalStartQuat * start2tipQuat;					//Start2tipQuat is locally applied with this order of ops

				//Move the needle
				tip.transform.rotation = rotateTip;
				tip.transform.position = moveTip;

				//Debug.Log ("Ros LH " + deltaQuatROSLH.ToString("F4"));
				//Debug.Log ("Unity LH " + start2tipQuat.ToString("F4"));

				//NOTE: Above is working but still might require random rotation about Z of either 90 or -90 (see quaternion output). 


				/*
				//TRANSLATIONS AND ROTATIONS - WORK IN PROGRESS (based on Option 2)
				//Ros RH -> Ros LH -> Unity LH
				//Create transformation matrix of needle pose in the ROS RH frame
				//Matrix4x4 transRH = Matrix4x4.TRS(posRH[lineCounter], quatRH[lineCounter],scale);
				//Create transformation matrix of needle pose in the ROS LH frame by reversing the x-axis
				Vector3 posLH = new Vector3(-1*posRH[lineCounter].x, posRH[lineCounter].y, posRH[lineCounter].z);
				Quaternion quatLH = new Quaternion (quatRH [lineCounter].x, -1 * quatRH [lineCounter].y, -1 * quatRH [lineCounter].z, quatRH [lineCounter].w);
				Matrix4x4 transLH = Matrix4x4.TRS (posLH, quatLH, scale);
				Vector3 posROS2UNITY = new Vector3 (0, 0, 0);				//No translation
				Quaternion quatROS2UNITY = Quaternion.Euler (0, -90, 90);	//Rotation from ROS needle frame to UNITY needle frame
				Matrix4x4 transROS2UNITY =  Matrix4x4.TRS (posROS2UNITY, quatROS2UNITY, scale);
				//Determine the transformation from the starting position of the tip to the current position of the tip
				Matrix4x4 start2tip = transLH * transROS2UNITY;
				//Determine the transformation of the current needle tip with respect to the global frame
				Matrix4x4 needleGlobal = global2start * start2tip;
				//Determine the translation and rotation parts of required movement for frame update
				Vector3 moveTip = new Vector3 (needleGlobal.m03, needleGlobal.m13, needleGlobal.m23);
				//Move the tip
				tip.transform.position = moveTip;
				tip.transform.rotation = quatLH;
				*/


				/*

				//OPTION 1 Ros RH -> Unity RH -> Unity LH
				//Transformation from Ros RH to Unity RH
				Quaternion ros2UnityRH =  Quaternion.Euler (0,-90,-90);
				//Current requested delta rotation and translation in ROS RH frame
				Quaternion rotRos = new Quaternion (pose._pose.GetOrientationMsg ().GetX (), pose._pose.GetOrientationMsg ().GetY (), pose._pose.GetOrientationMsg ().GetZ (), pose._pose.GetOrientationMsg ().GetW ());
				Quaternion rotUnity = Quaternion.Inverse (ros2UnityRH) * rotRos;
				//Requested delta rotation and translation in Unity RH frame
				Vector3 posRos = new Vector3 (pose._pose.GetPositionMsg ().GetX (), pose._pose.GetPositionMsg ().GetY (), pose._pose.GetPositionMsg ().GetZ ());
				Vector3 posUnity = Quaternion.Inverse (ros2UnityRH) *posRos;
				Vector3 posUnity2 = new Vector3 (pose._pose.GetPositionMsg().GetZ(), pose._pose.GetPositionMsg().GetX(), pose._pose.GetPositionMsg().GetY());
				Debug.Log ("Position in Unity method 1: " + posUnity + " method 2: " + posUnity2);
				//Note: posUnity and posUnity2 should be the same but they are NOT so one or both are wrong
				//TO DO: Change Unity RH to Unity LH
				//TO DO: Transform wrt global frame

				//Move the needle tip
				tip.transform.position = globalStartPos + posUnity;
				tip.transform.rotation = globalStartRot * rotUnity;


				//OPTION 2 Ros RH -> Ros LH -> Unity LH
				Vector3 deltaPos, deltaPosRH, deltaPosLH;
				Quaternion deltaRot, deltaRotRH, deltaRotLH;

				//delta change of position and orientation wrt ROS frame - right handed
				deltaPosRH = new Vector3 (pose._pose.GetPositionMsg().GetX(), pose._pose.GetPositionMsg().GetY(), pose._pose.GetPositionMsg().GetZ());
				deltaRotRH = new Quaternion (pose._pose.GetOrientationMsg ().GetX (), pose._pose.GetOrientationMsg ().GetY (), pose._pose.GetOrientationMsg ().GetZ (), pose._pose.GetOrientationMsg ().GetW ());

				//Change from ROS frame right handedness to ROS frame left handedness (reverse the X axis) as Unity is left-handed
				deltaPosLH = new Vector3(-1*deltaPosRH.x, deltaPosRH.y, deltaPosRH.z);
				deltaRotLH = new Quaternion (deltaRotRH.x, -1 * deltaRotRH.y, -1 * deltaRotRH.z, deltaRotRH.w);

				//Transform ROS needle left hand frame to Unity needle left hand frame
				deltaPos = new Vector3 (deltaPosLH.z, -1*deltaPosLH.x, -1*deltaPosLH.y); 
				Quaternion transformRot = Quaternion.Euler (0, -90, 90);
				//Debug.Log (transformRot);
				deltaRot = new Quaternion (deltaRotLH.x*transformRot.x, deltaRotLH.y*transformRot.y, deltaRotLH.z*transformRot.z, deltaRotLH.w*transformRot.w);

				//TO DO: Transform wrt global frame

				//Move the needle tip
				tip.transform.position = globalStartPos + deltaPos;
				tip.transform.rotation = globalStartRot * deltaRot;

				*/

				//Increment the counter to read from the file, loop back to beginning at end of each read
				lineCounter = lineCounter + 1;
				if (lineCounter > linesPose.Length-2) {
					lineCounter = 0;
				}
			}	
		}
	}
}
