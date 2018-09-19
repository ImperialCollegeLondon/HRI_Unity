using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventTriggers : MonoBehaviour {

	//Function to handle start button
	public void startButtonFcn() {
		EventManager.TriggerEvent ("start");	
		//Debug.Log ("Start Button has been pressed");
	}

//	//Function to handle tunnel toggle
//	public void tunnelToggleFcn() {
//		EventManager.TriggerEvent ("tunnelToggle");	
//		Debug.Log ("Tunnel toggle has been pressed");
//	}
//
//	//Function to handle crosshair toggle
//	public void crosshairToggleFcn() {
//		EventManager.TriggerEvent ("crosshairTunnel");	
//		Debug.Log ("Crosshair Tunnel has been pressed");
//	}

	//Function to handle steering mode dropdown menu choice
	public void steeringModeDropdownFcn() {
		EventManager.TriggerEvent ("steeringMode");	
		//Debug.Log ("Steering Mode has changed.");
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown ("q")) {
			EventManager.TriggerEvent ("quit");	
			//Debug.Log ("Q Key has been pressed");
		}
	}
}
