using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TunnelSteering : MonoBehaviour {

	//Error GUI box
	public Text ErrorText;
	private float error;

	//Ros vs Hardcoded Text file for path info
	private bool useHardCodedPath = false;

	//Visible vs Invisible Waypoint Rings
	private bool useRings = true;

	//Visibility of the needle trace
	private bool showNeedlePath = false;

	//Waypoint information
	//Variables
	private Vector3[] wayPointPos;
	private Quaternion[] wayPointQuat;
	private Vector3[] wayPointPosGlobal;
	private Quaternion[] wayPointQuatGlobal;
	private List<GameObject> path_ = new List<GameObject>();

	//Trajectory of the needle
	private List<Vector3> needlePath = new List<Vector3>();
	private LineRenderer pathRenderer;		//Could use a TrailRenderer instead?
	private Vector3 lastPos;

	//KD Tree
	private KDTree tree;

	//Function to write to the UI element
	void SetErrorText() {
		ErrorText.text = "Euclidean Error: " + error.ToString ("F2");
	}

	// Initialise the starting transformations of the objects in the scene. Modify this script according the the objects you would like to show
	//Can use quaternion representation directly, or the Quaternion.Euler function for Euler angle input
	void Start () {

		//Check setup started
		Debug.Log ("Setting the scene");

		//Initialise the UI element
		error = 0.00f;
		SetErrorText();

		//IF WE ARE USING A HARDCODED PATH
		if (useHardCodedPath == false) {
			Debug.Log ("The path shall be read in from ROS");
		} else {
			Debug.Log ("Using a hard coded path read in from file");
		}

		//Insertion Point N.B. This generally would map to the starting position of the needle but is kept separate
		//TBC if necessary
		GameObject startPos = GameObject.Find ("insertionPoint");
		if (startPos == null)
			Debug.Log ("Can't find the insertion point/starting point in the scene");
		else {
			//Starting position wrt global frame
			//startPos.transform.position = new Vector3 (0, 0, 0);
			startPos.transform.position = new Vector3 (12, 100, 0); //Top part of the brain
			//startPos.transform.position = new Vector3 (70,20,0);
			//Starting orientation wrt global frame
			startPos.transform.rotation = Quaternion.Euler(0, 0, -90); //Top part of the brain
			//startPos.transform.rotation = Quaternion.Euler(90, 0, -90);
		}

		//Needle - set at same position as the insertion point at the beginning
		GameObject tip = GameObject.Find ("needle");
		if (tip == null)
			Debug.Log ("Can't find the needle in the scene");
		else {
			//Starting position as the global [0,0,0] frame
			tip.transform.position = startPos.transform.position;
			//Starting orientation wrt global frame
			tip.transform.rotation = startPos.transform.rotation;
			tip.transform.rotation = Quaternion.Euler(0, 0, 180);
			lastPos = tip.transform.position;	//To be used when rendering the path the needle has taken

			//Setup the path renderer, needle trajectory, properties
			Color needleColour = Color.yellow;
			pathRenderer = tip.AddComponent<LineRenderer> ();
			pathRenderer.material = new Material (Shader.Find ("Legacy Shaders/Diffuse"));
			pathRenderer.startColor = needleColour;
			pathRenderer.endColor = needleColour;
			pathRenderer.startWidth = 1.0f;
			pathRenderer.endWidth = 1.0f;
		}

		//Note the obstacle map/container is best if centered at origin for the main camera. Modulartiy TBD
		//Obstacle map e.g. arterial tree
		GameObject obstacles = GameObject.Find ("arterialTree");
		if (obstacles == null)
			Debug.Log ("Can't find the obstacle map in the scene");
		else {
			obstacles.transform.position = new Vector3 (0, 0, 0);
			//obstacles.transform.rotation = new Quaternion (0, 0, 0, 0);
			obstacles.transform.rotation = Quaternion.Euler(0, 180, 180);
			//obstacles.transform.rotation = Quaternion.Euler(0, 0, 0);
		}

		//Container e.g. brain
		GameObject container = GameObject.Find ("brainTissue");
		if (container == null)
			Debug.Log ("Can't find the container object in the scene");
		else {
			container.transform.position = new Vector3 (0, 0, 0);
			//container.transform.rotation = new Quaternion (0, 0, 0, 0);
			container.transform.rotation = Quaternion.Euler(0, 180, 180);
			//container.transform.rotation = Quaternion.Euler(0, 0, 0);
		}

		//Create a repeating invoked function after 5.0 seconds at a set period of 1.0 seconds
		if (showNeedlePath)
		{
			InvokeRepeating("DrawTrajectories", 5.0f, 0.5f);
		}
	}

	void DrawPath() {

		// Draw the path and create the KDTree from all the points in the path

		//Create the rings with respect to the starting insertion point
		Vector3 globalStartPos = GameObject.Find ("insertionPoint").transform.position;
		Quaternion globalStartQuat = GameObject.Find ("insertionPoint").transform.localRotation;
		//Quaternion idenQ = new Quaternion (0f, 0f, 0f, 1f);
		//Vector3 scale = new Vector3 (1f, 1f, 1f);
		//Vector3 wayPointRingPos;
		//Quaternion wayPointRingQuat;
		//Matrix4x4 global2start = Matrix4x4.TRS (globalStartPos, globalStartQuat, scale);
		Vector3 wayPointInt;
		Quaternion wayPointIntQuat;

		for (int i = 0; i < wayPointPos.GetLength (0); i++) {

			//Get waypoint from the array
			wayPointInt = wayPointPos [i];
			wayPointIntQuat = wayPointQuat [i];

			//Convert ROS RH quaternion to ROS LH quaternion by mirroring the Z axis, and translation by negating z
			Vector3 deltaPosLocal = new Vector3 (wayPointInt.x, wayPointInt.y, -1 * wayPointInt.z);
			Quaternion deltaQuatLocal = new Quaternion (-1 * wayPointIntQuat.x, -1 * wayPointIntQuat.y, wayPointIntQuat.z, wayPointIntQuat.w);

			//Define transformation between ROS LH and Unity LH Frames
			//Done by examination ROS(RH)[X Y Z] -> UNITY(LH)[Y -Z X], 
			Quaternion ros2unityQuat = Quaternion.Euler (0, -90, 90);	//Order
			Quaternion ros2unityQuatInverse = Quaternion.Inverse (ros2unityQuat);

			//Transform the needle in the local Unity LH frame
			Vector3 start2tipPos = ros2unityQuatInverse * (ros2unityQuat * deltaPosLocal);
			Quaternion start2tipQuat = deltaQuatLocal * ros2unityQuatInverse;

			//Change to global frame
			wayPointPosGlobal [i] = globalStartPos + (globalStartQuat * start2tipPos);	//Start2tipPos transferred to global frame first
			wayPointQuatGlobal [i] = globalStartQuat * start2tipQuat;					//Start2tipQuat is locally applied with this order of ops
			//Debug.Log ("Global UNITY Quaternion: " + wayPointQuatGlobal [i].ToString("F4"));

			if (useRings) {
				//Plot a ring at every data point
				if ((i % 1) == 0) {

					//print("loop: " + i + "of " + wayPointPos.GetLength);
					//replace the waypoints with those from the message
					//The following quaternion is a correction to the ring instantiation orientation
					Quaternion fixRing = new Quaternion (0.707f, 0f, 0f, 0.707f); //Rotation x by 90 degrees
					GameObject tmp = (GameObject)GameObject.Instantiate (Resources.Load ("RefPath"), wayPointPosGlobal [i], wayPointQuatGlobal [i] * fixRing);	//Instead of reference the resource RefPath, get it from the message
					tmp.name = "ring_" + i;
					path_.Add (tmp);
				}
			}
		}

		//Create the tree using the path points
		tree = KDTree.MakeFromPoints (wayPointPosGlobal);
		Debug.Log ("Tree should have been created using " + wayPointPos.GetLength (0) + " points");

		//Draw sphere at target position to represent the e.g. tumor
		GameObject targetSphere = new GameObject ();
		targetSphere = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		targetSphere.transform.localScale = new Vector3 (10, 10, 10);
		Vector3 targetPos = new Vector3 ();
		//Quaternion targetQuat = new Quaternion ();
		targetPos = wayPointPosGlobal [wayPointPosGlobal.GetLength (0) - 1];
		//targetQuat = wayPointQuatGlobal[wayPointQuatGlobal.GetLength (0)-2];
		targetSphere.transform.position = targetPos;
		//targetSphere.transform.rotation = targetQuat;
		targetSphere.GetComponent<Renderer> ().material.color = new Vector4 (102, 0, 0, 1);
	}

	// Update is called once per frame
	void Update () {

		//Check Tunnel Steering has been chosen by the user
		if (SetupScene.TunnelSteeringBool & SetupScene.StartProcedure) {

			//Check if the Path has already been drawn
			if (GameObject.Find ("ring_0") == null) {

				//HARD CODED PATH
				if (useHardCodedPath) {
					//Info for user
					Debug.Log ("Generating the hard coded path");
					//Read in the array of path positions and orientations
					//e.g. Read in from a file if we do not use the JSON message
					//var inputFile = Resources.Load ("pathHardCoded.txt", typeof(TextAsset));
					TextAsset mytxtData = (TextAsset)Resources.Load ("pathHardCoded");
					string textFile = mytxtData.text;
					var lines = textFile.Split ("\n" [0]); // gets all lines into separate strings
					//First line is reserved for the description of the text
					wayPointPos = new Vector3[lines.Length - 1];
					wayPointQuat = new Quaternion[lines.Length - 1];
					wayPointPosGlobal = new Vector3[lines.Length - 1];
					wayPointQuatGlobal = new Quaternion[lines.Length - 1];
					for (var i = 1; i < lines.Length - 1; i++) {
						var pt = lines [i].Split ("," [0]); // gets parts of the line into separate strings
						var px = float.Parse (pt [0]);
						var py = float.Parse (pt [1]);
						var pz = float.Parse (pt [2]);
						var qw = float.Parse (pt [3]);
						var qx = float.Parse (pt [4]);
						var qy = float.Parse (pt [5]);
						var qz = float.Parse (pt [6]);
						wayPointPos [i - 1] = new Vector3 (px, py, pz);
						wayPointQuat [i - 1] = new Quaternion (qx, qy, qz, qw);
						//Debug.Log ("Path " + i + "Position: " + wayPointPos [i] + "Quaternion: " + wayPointRot [i]);
					}

					//Draw the path and create the KD Tree
					DrawPath ();
				} 
				//ROS MESSAGE PATH
				else if (PathPoses.pathRosReceived) {
					//Info for user
					Debug.Log ("Generating the path from ROS");
					//Read in the ROS path information
					wayPointPos = PathPoses.pathRosPos;
					wayPointQuat = PathPoses.pathRosQuat;
					wayPointPosGlobal = new Vector3[wayPointPos.GetLength (0)];
					wayPointQuatGlobal = new Quaternion[wayPointQuat.GetLength (0)];

					//Draw the path and create the KD Tree
					DrawPath ();
				} else {
					//Info for user
					Debug.Log ("Waiting for the path information from ROS...");
				}
			} else {
				//By this point the path should have been drawn either with hardcoded information or from the ROS message

				//Check if the needle path waypoints should be shown 
				if (useRings)
				{
					if (Input.GetKeyDown(KeyCode.P))
					{
						Debug.Log("Toggling the waypoint rings");
						foreach (GameObject obj in path_)
						{
							obj.GetComponent<Renderer>().enabled = !obj.GetComponent<Renderer>().enabled;
						}
					}
				}

				//Determine the error of the needle to the path
				//Use KD Tree method (KDTree.cs downloaded from https://forum.unity3d.com/threads/point-nearest-neighbour-search-class.29923/)
				int wayPointRef;
				//float error;
				Vector3 needlePos;

				//Determine current position of the needle
				GameObject needleCurrent = GameObject.Find ("needle");
				needlePos = needleCurrent.transform.position;

				//Find the point which is the nearest neighbour
				wayPointRef = tree.FindNearest (needlePos);

				//Find the distance between tip and nearest neighbour
				float deltaX = Mathf.Abs (needlePos.x - wayPointPosGlobal [wayPointRef].x);
				float deltaY = Mathf.Abs (needlePos.y - wayPointPosGlobal [wayPointRef].y);
				float deltaZ = Mathf.Abs (needlePos.z - wayPointPosGlobal [wayPointRef].z);
				error = (float)Mathf.Sqrt (deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);

				//If NaN set to 0
				if (float.IsNaN (error)) {
					error = 0.00f;
				}

				//Update the UI text element
				SetErrorText();
			}
		}
	}

	//Periodic function
	//Note there is something called a 'Trail Renderer' that might be better to use than this
	void DrawTrajectories()
	{
		//Check Tunnel Steering has been chosen by the user
		if (SetupScene.TunnelSteeringBool & SetupScene.StartProcedure) {

			//Determine current position of the needle
			GameObject needleCurrent = GameObject.Find ("needle");
			if (needleCurrent == null)
				Debug.Log ("Fcn DrawTrajectories: Can't find the needle object in the scene");
			else {
				//Check to see if the needle has moved
				Vector3 newPos = needleCurrent.transform.position;

				if (lastPos != newPos) {

					//Determine the position of the starting point
					GameObject insertPoint = GameObject.Find("insertionPoint");
					if (insertPoint == null)
						Debug.Log("Fcn DrawTrajectories: Can't find the insertion point object in the scene");
					else
					{
						Vector3 startPos = insertPoint.transform.position;

						//Find the distance between the starting position and the old position
						float diffX = Mathf.Abs(lastPos.x - startPos.x);
						float diffY = Mathf.Abs(lastPos.y - startPos.y);
						float diffZ = Mathf.Abs(lastPos.z - startPos.z);
						float fwdMotionOld = (float)Mathf.Sqrt(diffX * diffX + diffY * diffY + diffZ * diffZ);

						//Find the distance between the starting position and the new position
						diffX = Mathf.Abs(newPos.x - startPos.x);
						diffY = Mathf.Abs(newPos.y - startPos.y);
						diffZ = Mathf.Abs(newPos.z - startPos.z);
						float fwdMotionNew = (float)Mathf.Sqrt(diffX * diffX + diffY * diffY + diffZ * diffZ);

						//Only draw the path if the needle has moved away from the starting position by at least 0.5mm
						if (fwdMotionNew - fwdMotionOld > 0.5)
						{
							Debug.Log("Needle position has moved forwards - drawing the trajectory");

							//Add the position of the needle to the trajectory list
							needlePath.Add(needleCurrent.transform.position);

							//Change number of points to match that in the list
							//pathRenderer.SetVertexCount(needlePath.Count);
							pathRenderer.positionCount = needlePath.Count;

							//Draw the path
							for (int i = 0; i < needlePath.Count; i++)
							{
								//Change the postion of the lines
								pathRenderer.SetPosition(i, needlePath[i]);
							}
						}
					}
				}

				//Update position reference
				lastPos = newPos;
			}
		}
	}
}