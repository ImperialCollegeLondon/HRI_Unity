using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayActualOffsets : MonoBehaviour {

	//Overlay information for the actual path from the motor offsets
	private List<Vector3> actualPath = new List<Vector3>();
	//private LineRenderer actOverlayRenderer;
	//Material actMaterial;					//Get the line renderer

	// Use this for initialization
	void Start() {
//		Color actualPathColour = Color.blue;
//		actOverlayRenderer = gameObject.AddComponent<LineRenderer> ();
//		actOverlayRenderer.material = GetComponent<Renderer>().material;
//		//actOverlayRenderer.material = new Material (Shader.Find ("Standard"));
//		actOverlayRenderer.startColor = actualPathColour;
//		actOverlayRenderer.endColor = actualPathColour;
//		actOverlayRenderer.startWidth = 1.0f;
//		actOverlayRenderer.endWidth = 1.0f;

		//Create a repeating invoked function after 5.0 seconds at a set period of 1.0 seconds
		InvokeRepeating("DrawOverlay",5.0f, 1.0f);
	}

	// Update is called once per frame
	void DrawOverlay () {

		//First Check Tunnel Steering has been chosen by the user
		if (SetupScene.TunnelSteeringBool & SetupScene.StartProcedure) {

			//Check information has been received over ros
			if (!NeedleOffsets.offsetsRosReceived) {

				//Determine current position of the needle
				GameObject needleCurrent = GameObject.Find ("needle");
				if (needleCurrent == null)
					Debug.Log ("Fcn NeedleOffsets Callback: Can't find the needle object in the scene");
				else {

					//Get the line rendered from the game object (defined in Unity interface)
					LineRenderer actOverlayRenderer = gameObject.GetComponent<LineRenderer>();

					//Declare variables
					float[] offsets = {0.2f, 0f, 0.4f, 0.7f};
					//Debug.Log (NeedleOffsets.needleOffsets);

					//Determine the needle curvature (magnitude and angle) based on the offsets. 
					//Plane references are in the local frame of the needle
					float planeX = (float)offsets [0] + (float)offsets [1] - (float)offsets [2] - (float)offsets [3];
					float planeZ = (float)offsets [1] + (float)offsets [2] - (float)offsets [0] - (float)offsets [3];
					float planeAngleRaw = (Mathf.Atan2 (planeZ, planeX) * Mathf.Rad2Deg);
					//Put in correction for unity frame + 180 planer change of angle to desired curvature
					float planeAngle = (Mathf.Sign(planeAngleRaw)*180)-(Mathf.Sign(planeAngleRaw)*Mathf.Sign(planeAngleRaw)*planeAngleRaw);
					float planeOffset = Mathf.Sqrt ((1/Mathf.Sqrt(2)*planeX * planeX) + (1/Mathf.Sqrt(2) * planeZ * planeZ));
					float planeRadius = 1/(0.0008f * planeOffset);		//This is assuming a 25mm offset results in a radius of curvature of 50mm
					float arcAngle = 0;	//Start at 90 degrees
					float arcLength = 90;	//End angle - start angle (90 here would be a quarter of a circle)
					int segments = 30;
					float z_dash = 0f;
					//Debug.Log ("Offsets: " + offsets + " PlaneX: " + planeX + " PlaneZ: " + planeZ);
					//Debug.Log ("The original plane angle is: " + planeAngleRaw + " corrected planeAngle is: " + planeAngle + " and the planeRadius is: " + planeRadius);

					//Prepare the vector of points defining the arc, and add this to the renderer list
					for (int i = 0; i <= segments; i++) {
						float x_dash = planeRadius - Mathf.Cos (Mathf.Deg2Rad * arcAngle) * planeRadius;
						float y_dash = Mathf.Sin (Mathf.Deg2Rad * arcAngle) * planeRadius;

						//Collect the point of the arc into a vector
						Vector3 rawVector = new Vector3 (x_dash, y_dash, z_dash);
						//Vector3 rawVector = new Vector3 (0, 1, i);

						//Rotate this vector according the curvature plane (rotation about y)
						//Vector3 baseOverlayVector = Quaternion.Euler (0, planeAngle, 0) * rawVector;
						Vector3 baseOverlayVector = Quaternion.Euler (0, planeAngle, 0) * rawVector;

						//Determine global position of the needle 
						Vector3 needlePos = needleCurrent.transform.position;
						Quaternion needleRot = needleCurrent.transform.localRotation;
						Vector3 overlayVector = needlePos + (needleRot * baseOverlayVector);		//Vector transferred to global frame first

						//Add the vector to the rendered list
						actualPath.Add (overlayVector);

						//Increment the angle
						arcAngle += (arcLength / segments);
					}

					//Change number of points to match that in the list
					actOverlayRenderer.positionCount = actualPath.Count;

					//Draw the actual path the needle will follow with respect to the needle tip
					for (int i = 0; i < actualPath.Count; i++)
					{
						//Change the postion of the lines
						actOverlayRenderer.SetPosition(i, actualPath[i]);
					}

					//Clear the list
					actualPath.Clear();
				}
			}
		}
	}

	void OnDestroy()
	{
		//Destroy the instance
		//Destroy(actMaterial);
	}
}