using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TunnelSteering : MonoBehaviour {

	//Error GUI box
	public Text ErrorText;
	private float error;

	//Ros vs Hardcoded Text file for path info
	private bool useHardCodedPath = true;
	private bool useLocalRosPath = false;

	//Flag to indicate the first initial path has been received
	private bool pathReceived = false;

	//Visible vs Invisible Waypoint Rings
	private bool useRings = true;

	//Visibility of the needle trace
	private bool showNeedlePath = false;

	//Waypoint information
	//Variables for local path reading (ROS RH frame)
	private Vector3[] wayPointPos;
	private Quaternion[] wayPointQuat;
	private Vector3[] wayPointPosGlobal;
	private Quaternion[] wayPointQuatGlobal;

	//Variables for global path reading (UNITY LH frame)
	private Vector3[] wayPointPosGLOBAL;
	private Quaternion[] wayPointQuatGLOBAL;
	//private List<GameObject> path_ = new List<GameObject>();
    private List<GameObject> path_;

    //Transformation between ROS RH Frame and Unity LH Frame
    //Done by examination ROS(RH)[X Y Z] -> UNITY(LH)[Y -Z X], 
    private Quaternion unitQuaternion = new Quaternion(0, 0, 0, 1);
	private Quaternion ros2unityQuat = Quaternion.Euler (0, -90, 90);

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


        //ONLY REQUIRED IF NEED TO MESS WITH MRI STLs
		//Note the obstacle map/container is best if centered at origin for the main camera. Modulartiy TBD
		//Obstacle map e.g. arterial tree
		GameObject obstacles = GameObject.Find ("PTNT01_s3DI_MC_HR_Obstacles");
		if (obstacles == null) {
			Debug.Log ("Can't find the obstacle map in the scene");
		} else {
			
			//Note, obj files defind in a RH frame are automatically converted to a LH frame by negating the x axis
			//Done by examination ROS(RH)[X Y Z] -> UNITY(LH)[Y -Z X],
			//obstacles.transform.position = new Vector3 (0, 0, 0);
			//obstacles.transform.rotation = ros2unityQuat * unitQuaternion * Quaternion.Inverse (ros2unityQuat);
		    //obstacles.transform.rotation = Quaternion.Euler(0, -90, 90);

			//Vector3 posRosLH = new Vector3 (obstacles.transform.position.x, obstacles.transform.position.y, -1 * obstacles.transform.position.z);
			//Quaternion quatRosLH = new Quaternion (-1 * obstacles.transform.rotation.x, -1 * obstacles.transform.rotation.y, obstacles.transform.rotation.z, obstacles.transform.rotation.w);
			//Transform the needle in the local Unity LH frame
			//obstacles.transform.position = Quaternion.Inverse (ros2unityQuat) * (ros2unityQuat * posRosLH);
			//obstacles.transform.rotation = quatRosLH * Quaternion.Inverse (ros2unityQuat);

		}

		//Container e.g. brain
		GameObject container = GameObject.Find ("PTNT01_3D_FLAIR_Tra_GreyMatter");
		if (container == null) {
			Debug.Log ("Can't find the container object in the scene");
		} else {
			//container.transform.position = new Vector3 (0, 0, 0);
			//Change to LH frame
			//container.transform.rotation = ros2unityQuat * unitQuaternion * Quaternion.Inverse (ros2unityQuat);
			//container.transform.rotation = Quaternion.Euler(0, -90, 90);

			//Vector3 posRosLH = new Vector3 (container.transform.position.x, container.transform.position.y, -1 * container.transform.position.z);
			//Quaternion quatRosLH = new Quaternion (-1 * container.transform.rotation.x, -1 * container.transform.rotation.y, container.transform.rotation.z, container.transform.rotation.w);
			//Transform the needle in the local Unity LH frame
			//container.transform.position = Quaternion.Inverse (ros2unityQuat) * (ros2unityQuat * posRosLH);
			//container.transform.rotation = quatRosLH * Quaternion.Inverse (ros2unityQuat);

		}

		//Container e.g. brain
		GameObject target = GameObject.Find ("PTNT01_3D_FLAIR_Tra_Target");
		if (target == null) {
			Debug.Log ("Can't find the target object in the scene");
		} else {
			//target.transform.position = new Vector3 (0, 0, 0);
			//Change to LH frame
			//container.transform.rotation = ros2unityQuat * unitQuaternion * Quaternion.Inverse (ros2unityQuat);
			//target.transform.rotation = Quaternion.Euler(0, -90, 90);

			//Vector3 posRosLH = new Vector3 (target.transform.position.x, target.transform.position.y, -1 * target.transform.position.z);
			//Quaternion quatRosLH = new Quaternion (-1 * target.transform.rotation.x, -1 * target.transform.rotation.y, target.transform.rotation.z, target.transform.rotation.w);
			//Transform the needle in the local Unity LH frame
			//target.transform.position = Quaternion.Inverse (ros2unityQuat) * (ros2unityQuat * posRosLH);
			//target.transform.rotation = quatRosLH * Quaternion.Inverse (ros2unityQuat);
		}
        

		//Create a repeating invoked function after 5.0 seconds at a set period of 0.5 seconds
		if (showNeedlePath)
		{
			InvokeRepeating("DrawTrajectories", 5.0f, 0.5f);
		}

		//Draw the path at repeated intervals
		InvokeRepeating("DrawThePath", 5.0f, 1.0f);
	}
	
	void DrawPath() {
		Debug.Log ("In DrawPath (Local)");
        path_ = new List<GameObject>();
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
			//Quaternion ros2unityQuat = Quaternion.Euler (0, -90, 90);	//Order
			//Quaternion ros2unityQuatInverse = Quaternion.Inverse (ros2unityQuat);

			//Transform the needle in the local Unity LH frame
			Vector3 start2tipPos = Quaternion.Inverse (ros2unityQuat) * (ros2unityQuat * deltaPosLocal);
			Quaternion start2tipQuat = deltaQuatLocal * Quaternion.Inverse (ros2unityQuat);

			//Change to global frame
			wayPointPosGlobal [i] = globalStartPos + (globalStartQuat * start2tipPos);	//Start2tipPos transferred to global frame first
			wayPointQuatGlobal [i] = globalStartQuat * start2tipQuat;					//Start2tipQuat is locally applied with this order of ops
			//Debug.Log ("Global UNITY Quaternion: " + wayPointQuatGlobal [i].ToString("F4"));

			if (useRings) {

				//Plot a new ring at every data point
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

	//Function to draw rings on the path, when the path is defined in the global Unity frame
	void DrawPathGlobal() {

        //The following quaternion is a correction to the ring instantiation orientation
        //Quaternion fixRing = new Quaternion (0.707f, 0f, 0f, 0.707f); //Rotation x by 90 degrees
        //Quaternion fixRing = new Quaternion (0f, 0f, 0.707f, 0.707f); //Rotation z by 90 degrees
        Quaternion fixRing = new Quaternion (0f, 0.707f, 0f, 0.707f); //Rotation y by 90 degrees
        //Quaternion fixRing = new Quaternion(0f, 0f, 0f, 1f); //No rotation
        int numRings = 5;

        //Delete the existing path if one has been drawn
        //NOTE - This should be changed such that we only delete/re-draw when the data changes i.e. is replanned
        //NOTE2 - If all path messages are the same size, then we can just reset the pose information of the rings, rather than deleting and re-creating
        if (pathReceived)
        {
            //Debug.Log("Deleting the path");
            for (int i = 0; i < path_.Count; i++)
            {
                Destroy(path_[i]);
            }
        }

        // Draw the path
        //Debug.Log("Creating the path in plan (Global) space");
        //Re-initialise the path
        path_ = new List<GameObject>();
        //Set the flag
        pathReceived = true;

        //Create the rings with respect to the starting insertion point
        for (int i = 0; i < wayPointPosGLOBAL.GetLength (0); i++) {

			if (useRings) {
				//Plot a ring at every 5th data point
				if ((i % numRings) == 0) {

					//print("loop: " + i + "of " + wayPointPos.GetLength);
					//replace the waypoints with those from the message

					GameObject tmp = (GameObject)GameObject.Instantiate (Resources.Load ("RefPath"), wayPointPosGLOBAL [i], wayPointQuatGLOBAL [i] * fixRing);	//Instead of reference the resource RefPath, get it from the message
					//GameObject tmp = (GameObject)GameObject.Instantiate (Resources.Load ("RefPath"), wayPointPosGLOBAL [i], wayPointQuatGLOBAL [i]);	//Instead of reference the resource RefPath, get it from the message
					tmp.name = "ring_" + i;
                    //tmp.name = "ring_";
                    path_.Add (tmp);
				}
			}
		}

		//create the KDTree from all the points in the path
		tree = KDTree.MakeFromPoints (wayPointPosGLOBAL);
		Debug.Log ("Tree should have been created using " + wayPointPosGLOBAL.GetLength (0) + " points");
	}

	// Periodic update of the path
	void DrawThePath () {

		//Check Tunnel Steering has been chosen by the user
		if (SetupScene.TunnelSteeringBool & SetupScene.StartProcedure) {

			//HARD CODED PATH
			if (useHardCodedPath) {
				//Check if the Path has already been drawn
				if (GameObject.Find ("ring_0") == null) {
							
					//Info for user
					Debug.Log ("Generating the hard coded path");
					//Read in the array of path positions and orientations from a file
					TextAsset mytxtData = (TextAsset)Resources.Load ("pathHardCoded_PlanSpace");
					string textFile = mytxtData.text;
					var lines = textFile.Split ("\n" [0]); // gets all lines into separate strings
					//First line is reserved for the description of the text
					wayPointPosGLOBAL = new Vector3[lines.Length - 1];
					wayPointQuatGLOBAL = new Quaternion[lines.Length - 1];
					//Quaternion wayPointIntQuat;
					for (var i = 1; i < lines.Length - 1; i++) {
						var pt = lines [i].Split ("," [0]); // gets parts of the line into separate strings
						var px = float.Parse (pt [0]);
						var py = float.Parse (pt [1]);
						var pz = float.Parse (pt [2]);
						var qw = float.Parse (pt [3]);
						var qx = float.Parse (pt [4]);
						var qy = float.Parse (pt [5]);
						var qz = float.Parse (pt [6]);

						//Define the path point in Global Unity LH frame
						wayPointPosGLOBAL [i - 1] = new Vector3 (px, -1*py, pz);				//Negate the y-axis to change to LH frame 
						wayPointQuatGLOBAL [i - 1] = new Quaternion (-1*qx, qy, -1*qz, qw);		//Mirror the y-axis to change to LH frame 
					}

					//Insertion Point N.B. This generally would map to the starting position of the needle but is kept separate
					//TBC if necessary
					GameObject startPos = GameObject.Find ("insertionPoint");
					if (startPos == null)
						Debug.Log ("Can't find the insertion point/starting point in the scene");
					else {
						//Starting position wrt global frame - first pose of the path
						startPos.transform.position = wayPointPosGLOBAL [0]; 
						startPos.transform.rotation = wayPointQuatGLOBAL [0]; 
					}

					//Needle - set at same position as the insertion point at the beginning
					GameObject tip = GameObject.Find ("needle");
					if (tip == null)
						Debug.Log ("Can't find the needle in the scene");
					else {
						//Starting position wrt global frame - first pose of the path
						tip.transform.position = wayPointPosGLOBAL [0];
						tip.transform.rotation = wayPointQuatGLOBAL [0];
						//Change from ROS LH needle frame to Unity LH needle frame
						//tip.transform.Rotate (0.0f, 0.0f, -90.0f);		//Rotate z axis by -90 degrees [Note: Spear is forward in Y, up in Z and left in X]....this fixes the position but likely not the rotation)

						//To be used when rendering the path the needle has taken
						lastPos = tip.transform.position;	

						//Setup the path renderer, needle trajectory, properties
						Color needleColour = Color.yellow;
						pathRenderer = tip.AddComponent<LineRenderer> ();
						pathRenderer.material = new Material (Shader.Find ("Legacy Shaders/Diffuse"));
						pathRenderer.startColor = needleColour;
						pathRenderer.endColor = needleColour;
						pathRenderer.startWidth = 1.0f;
						pathRenderer.endWidth = 1.0f;
					}

					//Draw the path and create the KD Tree
					DrawPathGlobal ();
				}
			} else if (PathPoses.pathRosReceived) { //ROS MESSAGE PATH
				//Info for user
				Debug.Log ("Generating the path from ROS");

				//Handle the message according to if it's sent in global or local frame
				if (useLocalRosPath) {

					//Read in the ROS path information
					wayPointPos = PathPoses.pathRosPos;
					wayPointQuat = PathPoses.pathRosQuat;
					wayPointPosGlobal = new Vector3[wayPointPos.GetLength (0)];
					wayPointQuatGlobal = new Quaternion[wayPointQuat.GetLength (0)];

					//Only do the following the first time the path is received (i.e. initial path)
					if (!pathReceived) {
						//Set the flag
						pathReceived = true;
					
						//Insertion Point N.B. This generally would map to the starting position of the needle but is kept separate
						//TBC if necessary
						GameObject startPos = GameObject.Find ("insertionPoint");
						if (startPos == null)
							Debug.Log ("Can't find the insertion point/starting point in the scene");
						else {
							//Arbitrarily define the insertion point
							startPos.transform.position = new Vector3 (22, 75, 0); //Top part of the brain 
							startPos.transform.rotation = Quaternion.Euler (0, 0, -90); //Top part of the brain 
						}

						//Needle - set at same position as the insertion point at the beginning
						GameObject tip = GameObject.Find ("needle");
						if (tip == null)
							Debug.Log ("Can't find the needle in the scene");
						else {
							//Define tip at the insertion point
							tip.transform.position = startPos.transform.position;
							tip.transform.rotation = startPos.transform.rotation;
							tip.transform.rotation = Quaternion.Euler (0, 0, 180);

							//To be used when rendering the path the needle has taken
							lastPos = tip.transform.position;	

							//Setup the path renderer, needle trajectory, properties
							Color needleColour = Color.yellow;
							pathRenderer = tip.AddComponent<LineRenderer> ();
							pathRenderer.material = new Material (Shader.Find ("Legacy Shaders/Diffuse"));
							pathRenderer.startColor = needleColour;
							pathRenderer.endColor = needleColour;
							pathRenderer.startWidth = 1.0f;
							pathRenderer.endWidth = 1.0f;
						}
					}

					//Draw the local path and create the KD Tree
					DrawPath ();
				} else {

					//Read in the ROS path information
					wayPointPos = PathPoses.pathRosPos;
					wayPointQuat = PathPoses.pathRosQuat;
					wayPointPosGLOBAL = new Vector3[wayPointPos.GetLength (0)];
					wayPointQuatGLOBAL = new Quaternion[wayPointQuat.GetLength (0)];
					Vector3 wayPointInt;
					Quaternion wayPointIntQuat;

					for (int i = 0; i < wayPointPos.GetLength (0); i++) {

						//Get waypoint from the Global Path RH frame
						wayPointInt = wayPointPos [i];
						wayPointIntQuat = wayPointQuat [i];

						//Define the path point in Global Unity LH frame (i.e. convert to LH frame)
						wayPointPosGLOBAL [i] = new Vector3 (wayPointInt.x, -1*wayPointInt.y, wayPointInt.z);											//Negate the y-axis to change to LH frame 
						wayPointQuatGLOBAL [i] = new Quaternion (-1*wayPointIntQuat.x, wayPointIntQuat.y, -1*wayPointIntQuat.z, wayPointIntQuat.w);		//Mirror the y-axis to change to LH frame
					}

					//Only do the following the first time the path is received (i.e. initial path)
					if (!pathReceived) {


						//Insertion Point N.B. This generally would map to the starting position of the needle but is kept separate
						//TBC if necessary
						GameObject startPos = GameObject.Find ("insertionPoint");
						if (startPos == null)
							Debug.Log ("Can't find the insertion point/starting point in the scene");
						else {
							//Starting position wrt global frame - first pose of the path
							startPos.transform.position = wayPointPosGLOBAL [0]; 
							startPos.transform.rotation = wayPointQuatGLOBAL [0]; 
						}

						//Needle - set at same position as the insertion point at the beginning
						GameObject tip = GameObject.Find ("needle");
						if (tip == null)
							Debug.Log ("Can't find the needle in the scene");
						else {
							//Starting position wrt global frame - first pose of the path
							tip.transform.position = wayPointPosGLOBAL [0];
							tip.transform.rotation = wayPointQuatGLOBAL [0];
							//Change from ROS LH needle frame to Unity LH needle frame
							//tip.transform.Rotate (0.0f, 0.0f, -90.0f);		//Rotate z axis by -90 degrees [Note: Spear is forward in Y, up in Z and left in X]....this fixes the position but likely not the rotation)

							//To be used when rendering the path the needle has taken
							lastPos = tip.transform.position;	

							//Setup the path renderer, needle trajectory, properties
							Color needleColour = Color.yellow;
							pathRenderer = tip.AddComponent<LineRenderer> ();
							pathRenderer.material = new Material (Shader.Find ("Legacy Shaders/Diffuse"));
							pathRenderer.startColor = needleColour;
							pathRenderer.endColor = needleColour;
							pathRenderer.startWidth = 1.0f;
							pathRenderer.endWidth = 1.0f;
						}
					}

					//Draw the global path and create the KD Tree
					DrawPathGlobal ();
				}
			} else {
				//Info for user
				Debug.Log ("Waiting for the path information from ROS...");
			}

			//If the path has been drawn, determine the error and handle user input
			if (pathReceived) {
				//Check if the needle path waypoints should be shown 
				if (useRings) {
					if (Input.GetKeyDown (KeyCode.P)) {
						Debug.Log ("Toggling the waypoint rings");
						foreach (GameObject obj in path_) {
							obj.GetComponent<Renderer> ().enabled = !obj.GetComponent<Renderer> ().enabled;
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

				if (useLocalRosPath) {
					//Find the distance between tip and nearest neighbour
					float deltaX = Mathf.Abs (needlePos.x - wayPointPosGlobal [wayPointRef].x);			//Local path reference over ROS
					float deltaY = Mathf.Abs (needlePos.y - wayPointPosGlobal [wayPointRef].y);
					float deltaZ = Mathf.Abs (needlePos.z - wayPointPosGlobal [wayPointRef].z);
					error = (float)Mathf.Sqrt (deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
				} else {
					//Find the distance between tip and nearest neighbour
					float deltaX = Mathf.Abs (needlePos.x - wayPointPosGLOBAL [wayPointRef].x);			//Global path reference over ROS
					float deltaY = Mathf.Abs (needlePos.y - wayPointPosGLOBAL [wayPointRef].y);
					float deltaZ = Mathf.Abs (needlePos.z - wayPointPosGLOBAL [wayPointRef].z);
					error = (float)Mathf.Sqrt (deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
				}

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