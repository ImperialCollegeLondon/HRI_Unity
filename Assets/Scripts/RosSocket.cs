using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using ROSBridgeLib;
using ROSBridgeLib.std_msgs;

public class RosSocket : MonoBehaviour {
	private ROSBridgeWebSocketConnection ros = null;
	private enum cmdToMaxon {HOMESWITCH, HOMESNOWITCH, STOP, POSITIONABS, POSITIONREL, VELOCITY, OFFSETS, GETPOSITION, START_TRACK,IDLE, RESETENCODERS};

	// Use this for initialization
	void Start () {
        //ros = new ROSBridgeWebSocketConnection ("ws://155.198.42.230", 9090); 	//Lab Computer
        //ros = new ROSBridgeWebSocketConnection("ws://155.198.43.20", 9090); 		//Eloise PC
        //ros = new ROSBridgeWebSocketConnection("ws://146.169.163.171", 9090); 	//Eloise Laptop
        ros = new ROSBridgeWebSocketConnection("ws://192.168.1.2", 9090); 			//GPU Computer
        ros.AddSubscriber (typeof(TipPose));
		ros.AddSubscriber (typeof(PathPoses));
		ros.AddSubscriber (typeof(CmdPathOverlay));
		ros.AddSubscriber (typeof(ActPathOverlay));
		ros.AddPublisher(typeof(CommandsToRos));
		ros.Connect ();		
	}

	void OnApplicationQuit() {
		if(ros!=null)
			ros.Disconnect ();
	}

	// Update is called once per frame
	void Update () {

        //Print the time between updates
        //Debug.Log("Update time :" + Time.deltaTime);   
            
		//Homeswitch 
        if (Input.GetKeyDown(KeyCode.H))
        {
			UInt8Msg cmdToRos = new UInt8Msg(0);
        	ros.Publish(CommandsToRos.GetMessageTopic(), cmdToRos);
    	}
        //Home Encoders
		if (Input.GetKeyDown(KeyCode.J))
        {
        	UInt8Msg cmdToRos = new UInt8Msg(10);
        	ros.Publish(CommandsToRos.GetMessageTopic(), cmdToRos);
    	}
		//Stop
        if (Input.GetKeyDown(KeyCode.S))
        {
        	UInt8Msg cmdToRos = new UInt8Msg(2);
        	ros.Publish(CommandsToRos.GetMessageTopic(), cmdToRos);
    	}
		//Go (Velocity Mode)
        if (Input.GetKeyDown(KeyCode.G))
        {
        	UInt8Msg cmdToRos = new UInt8Msg(5);
        	ros.Publish(CommandsToRos.GetMessageTopic(), cmdToRos);
        }
		//Increase speed
        if (Input.GetKeyDown(KeyCode.M))
        {
            UInt8Msg cmdToRos = new UInt8Msg(102);
            ros.Publish(CommandsToRos.GetMessageTopic(), cmdToRos);
        }
		//Reset Offset
        if (Input.GetKeyDown(KeyCode.R))
        {
            UInt8Msg cmdToRos = new UInt8Msg(114);
            ros.Publish(CommandsToRos.GetMessageTopic(), cmdToRos);
        }
		//Decrease speed
        if (Input.GetKeyDown(KeyCode.N))
        {
            UInt8Msg cmdToRos = new UInt8Msg(118);
            ros.Publish(CommandsToRos.GetMessageTopic(), cmdToRos);
        }
			
        //Reads sockets and applies subscriber callbacks
        ros.Render();	
	}
}
