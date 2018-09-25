using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public Camera overheadCamera;
	public Camera needleCamera;
	public Camera crosshairCamera;
	public Light overheadLight;
	public Light needleLight;
	public Light crosshairLight;
	//public GameObject overheadBackground;
	//public GameObject needleBackground;
	//public GameObject crosshairBackground;

	private Vector3 resetOverheadViewPos, origin, difference;
	private Quaternion resetOverheadViewRot;

	private bool drag=false;
	private bool overheadView, catchButton;
	private float rotateLeftRight;

	//Function to change to overhead view
	public void ShowOverheadView() {
		//needleBackground.GetComponent<MeshRenderer> ().enabled = false;
		needleLight.enabled = false;
		needleCamera.enabled = false;
		//crosshairBackground.GetComponent<MeshRenderer> ().enabled = false;
		crosshairLight.enabled = false;
		crosshairCamera.enabled = false;
		//overheadBackground.GetComponent<MeshRenderer>().enabled = true;
		overheadLight.enabled = true;
		overheadCamera.enabled = true;
	}

	//Function to change to the needle view
	public void ShowNeedleView() {
		//overheadBackground.GetComponent<MeshRenderer>().enabled = false;
		overheadLight.enabled = false;
		overheadCamera.enabled = false;

		//Change to correct steering mode camera view
		if (SetupScene.CrosshairSteeringBool == true & SetupScene.TunnelSteeringBool == false) {
			//crosshairBackground.GetComponent<MeshRenderer> ().enabled = true;
			crosshairLight.enabled = true;
			crosshairCamera.enabled = true;
		} else if (SetupScene.TunnelSteeringBool == true & SetupScene.CrosshairSteeringBool == false) {
			//needleBackground.GetComponent<MeshRenderer> ().enabled = true;
			needleLight.enabled = true;
			needleCamera.enabled = true;
		}
	}

	//Toggle visibility of the brain
	void ToggleVisibility() {
		// toggles the visibility of this gameobject and all it's children
		GameObject brain = GameObject.Find ("PTNT01_3D_FLAIR_Tra_GreyMatter");
		if (brain == null) {
		}
		else {
			Renderer[] rs = brain.GetComponentsInChildren<Renderer> ();
			foreach (Renderer r in rs)
				r.enabled = !r.enabled;
		}
	}

	// Use this for initialization
	void Start () {

		//Set overhead camera
		overheadView = true;
		ShowOverheadView();

		//Save starting position of the overhead camera
		resetOverheadViewPos = overheadCamera.transform.position;
		resetOverheadViewRot = overheadCamera.transform.rotation;

		//Initialise amount of rotation
		rotateLeftRight = 0.00f;

	}

	// Update is called once per frame
	void LateUpdate () {

		//Check if the start button has been pressed
		if (SetupScene.StartProcedure == true) {

			//Check if user has entered insertion mode i.e. left overheadView by pressing the space button
			if (Input.GetKeyDown (KeyCode.Space) && catchButton == false) {
				overheadView = !overheadView;

				//Hide the brain by default
				GameObject brain = GameObject.Find ("PTNT01_3D_FLAIR_Tra_GreyMatter");
				if (brain == null) {
				}
				else {
					Renderer[] rs = brain.GetComponentsInChildren<Renderer> ();
					foreach (Renderer r in rs)
						r.enabled = false;
				}

				catchButton = true;
			}
			if (Input.GetKeyUp (KeyCode.Space)) {
				catchButton = false;
			}

			//Show the needle perspective and activate needle movement 
			if (overheadView == false) {
				//Change to the needle view
				ShowNeedleView ();

				//Toggle the brainTissue to show on and off with different key press when in overhead view
				if (Input.GetKeyDown(KeyCode.Z)) {
					//toggle visibility:
					ToggleVisibility();
				}

			} else {
				//Change to overhead view
				ShowOverheadView ();

				//Toggle the brainTissue to show on and off with different key press when in overhead view
				if (Input.GetKeyDown(KeyCode.Z)) {
					//toggle visibility:
					ToggleVisibility();
				}

				//Set that the scroll button on the mouse will move the camera towards and away from the brain
				int zoomSpeed = 0;
				if (Input.GetAxis ("Mouse ScrollWheel") > 0) {
					zoomSpeed = 50;
				} else if (Input.GetAxis ("Mouse ScrollWheel") < 0) {
					zoomSpeed = -50;
				}

				//Set camera position
				overheadCamera.transform.position += overheadCamera.transform.forward * Time.deltaTime * zoomSpeed;

				//Pan with the mouse N.B. Not working yet
				/*difference = new Vector3 ();
				origin = new Vector3 ();

				//Check left button for rotation
				int rotateSpeed = 2;
				if (Input.GetMouseButton (0)) {

					rotateLeftRight += Time.deltaTime * rotateSpeed;
				}
				if (Input.GetMouseButton (1)) {

					rotateLeftRight -= Time.deltaTime * rotateSpeed;
				}

				Debug.Log ("Rotating Amount Euler: " + rotateLeftRight);
				Vector3 rotateAmount = new Vector3 (0,rotateLeftRight,0);
				overheadCamera.transform.Rotate (rotateAmount);
				*/

				//Reset camera with right click
				if (Input.GetMouseButton (1)) {
					overheadCamera.transform.position=resetOverheadViewPos;
					overheadCamera.transform.rotation = resetOverheadViewRot;
				}
			}
		}
	}
}