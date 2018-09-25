using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayActualOffsets : MonoBehaviour {

	//Overlay information for the commanded path (points sent in via ROS)
	private List<Vector3> actPath = new List<Vector3>();

	// Use this for initialization
	void Start() {

		//Create a repeating invoked function after 5.0 seconds at a set period of 0.5 seconds
		InvokeRepeating("DrawOverlay",5.0f, 0.5f);
	}

	void DrawOverlay () {

		//First Check Tunnel Steering has been chosen by the user
		if (SetupScene.TunnelSteeringBool & SetupScene.StartProcedure) {

			//Check information has been received over ros
			if (ActPathOverlay.overlayActRosReceived) {

				//Determine current position of the needle
				GameObject needleCurrent = GameObject.Find ("needle");
				if (needleCurrent == null)
					Debug.Log ("Fcn NeedleOffsets Callback: Can't find the needle object in the scene");
				else {

					//Get the line rendered from the game object (defined in Unity interface)
					LineRenderer actOverlayRenderer = gameObject.GetComponent<LineRenderer> ();
					//Debug.Log ("Number of positions in actual overlay from ROS is: " + ActPathOverlay.overlayActRosPos.GetLength (0));

					//Define transformation between ROS LH and Unity LH Frames
					//Done by examination ROS(RH)[X Y Z] -> UNITY(LH)[Y -Z X], 
					Quaternion ros2unityQuat = Quaternion.Euler (0, -90, 90);	//Order
					Quaternion ros2unityQuatInverse = Quaternion.Inverse (ros2unityQuat);

					for (int i = 0; i < ActPathOverlay.overlayActRosPos.GetLength (0); i++) {

						//Get starting point as the current pose of the needle tip
						Vector3 globalNeedlePos = GameObject.Find ("needle").transform.position;
						Quaternion globalNeedleQuat = GameObject.Find ("needle").transform.localRotation;

						//Get overlay path points from the array
						Vector3 posInt = ActPathOverlay.overlayActRosPos [i];
						Quaternion quatInt = ActPathOverlay.overlayActRosQuat [i];
						//Debug.Log ("Actual Overlay Time step: " + i + ", ROS pos is: " + posInt);

						//Convert ROS RH quaternion to ROS LH quaternion by mirroring the Z axis, and translation by negating z
						Vector3 deltaPosLocalROS = new Vector3 (posInt.x, posInt.y, -1 * posInt.z);
						Quaternion deltaQuatLocalROS = new Quaternion (-1 * quatInt.x, -1 * quatInt.y, quatInt.z, quatInt.w);

						//Transform the overlay position in the local Unity LH frame
						//Vector3 deltaPosLocal = ros2unityQuatInverse * (ros2unityQuat * deltaPosLocalROS);
						Quaternion deltaQuatLocal = deltaQuatLocalROS * ros2unityQuatInverse;

						//TEST - direct transformatin ROS(RH)[X Y Z] -> UNITY(LH)[Y -Z X]
						Vector3 deltaPosLocal = new Vector3 (posInt.z, posInt.x, -1*posInt.y);
						deltaPosLocal = Quaternion.Euler(0, 180, 0) * deltaPosLocal; //Seems to be required for the transparent shader
						//Debug.Log ("And Local Unity Act Pos is: " + deltaPosLocal);

						//Change to global frame
						Vector3 deltaPosGlobal = globalNeedlePos + (globalNeedleQuat * deltaPosLocal);		//deltaPosLocal transferred to global frame first
						Quaternion deltaQuatGlobal = globalNeedleQuat * deltaQuatLocal;						//deltaQuatLocal is locally applied with this order of ops

						//Add the vector to the rendered list (currently orientation ignored)
						actPath.Add (deltaPosLocal);
					}

					//Change number of points to match that in the list
					actOverlayRenderer.positionCount = actPath.Count;
					//Debug.Log ("Number of positions in overlay is: " + actPath.Count);

					//Draw the actual path the needle will follow with respect to the needle tip
					for (int j = 0; j < actPath.Count; j++) {
						//Change the postion of the lines
						actOverlayRenderer.SetPosition (j, actPath [j]);
						//Debug.Log ("Setting overlay position " + j + " as: " + actPath [j]);
					}

					//Clear the list
					actPath.Clear ();
				}
			}
		}
	}
}
