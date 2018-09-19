using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrossHairSteering : MonoBehaviour {

	private float nextActionTime = 0.0f;
	public float period = 1f;
	private static int counter = 0;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {

		//Ensure that crossHair Steering mode is active
		if (SetupScene.CrosshairSteeringBool == true) {

			if (Time.time > nextActionTime) {

				//Execute timely code
				nextActionTime = Time.time + period;  
				counter = counter + 5;

				//Determine current position of the needle with respect to the target i.e. the error in X and Y

				float needleErrorX = counter;
				float needleErrorY = -250;

				//Check limits of the background image haven't been reached
				if (needleErrorX > 200) {
					needleErrorX = 200;
				} else if (needleErrorX < -200) {
					needleErrorX = -200;
				}

				if (needleErrorY > 200) {
					needleErrorY = 200;
				} else if (needleErrorY < -200) {
					needleErrorY = -200;
				}

				//Move the needle crosshair to the new position
				GameObject.Find ("needleCrosshair").GetComponent<RectTransform> ().anchoredPosition = new Vector2 (needleErrorX, needleErrorY);
				Debug.Log (GameObject.Find ("needleCrosshair").GetComponent<RectTransform> ().position);
			}
		}
	}		
}