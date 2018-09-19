using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SetupScene : MonoBehaviour {
	
	public static bool StartProcedure;
	public static bool TunnelSteeringBool;
	public static bool CrosshairSteeringBool;

	public Dropdown steeringModeDropdown;

	//Register the listener
	void OnEnable()
	{
		//Register the listeners
		EventManager.StartListening ("start", StartFcn);
		//EventManager.StartListening ("tunnelToggle", TunnelFcn);
		//EventManager.StartListening ("crosshairTunnel", CrosshairFcn);
		EventManager.StartListening ("steeringMode", SteeringModeFcn);
		EventManager.StartListening ("quit", QuitFcn);

		//Initialise all bool references to zero
		StartProcedure = false;
		CrosshairSteeringBool = false;
		TunnelSteeringBool = false;
	}

	//De-register the listener (necessary to avoid a memory leak)
	void OnDisable()
	{
		//Deregister the listeners
		EventManager.StopListening ("start", StartFcn);
		//EventManager.StopListening ("tunnelToggle", TunnelFcn);
		//EventManager.StopListening ("crosshairTunnel", CrosshairFcn);
		EventManager.StartListening ("steeringMode", SteeringModeFcn);
		EventManager.StopListening ("quit", QuitFcn);
	}

	//Listener to manage the start button
	void StartFcn()
	{
		StartProcedure = true;
		//Debug.Log ("Start Button processed");
	}

//	//Listener to manage the tunnel toggle
//	void TunnelFcn()
//	{
//		TunnelSteeringBool = !TunnelSteeringBool;
//		//Debug.Log ("Tunnel Toggle processed, value is: " + TunnelSteeringBool + ", Crosshair Value is: " + CrosshairSteeringBool);
//	}
//
//	//Listener to manage the crosshair toggle
//	void CrosshairFcn()
//	{
//		CrosshairSteeringBool = !CrosshairSteeringBool;
//		//Debug.Log ("Crosshair Toggle processed, value is: " + CrosshairSteeringBool + ", Tunnel Value is: " + TunnelSteeringBool);
//	}

	void SteeringModeFcn()
	{
		//Set default values for steering modes
		TunnelSteeringBool = false;
		CrosshairSteeringBool = false;

		//Update bools to reflect user choice
		if (steeringModeDropdown.value == 1) {
			TunnelSteeringBool = true;
		} else if (steeringModeDropdown.value == 2) {
			CrosshairSteeringBool = true;
		}
			
		//Debug.Log ("Dropdown menu value is: " + steeringModeDropdown.value);
	}

    //Listener to manage the quit button
    void QuitFcn()
	{
		Debug.Log ("Quit (q) request processed");
		Application.Quit ();	//Only stops for compiled + build games
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;		//Quits the editor
		#endif
	}
}
