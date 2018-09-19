using System;
using System.Collections.Generic;
using ROSBridgeLib;
using ROSBridgeLib.std_msgs;
using SimpleJSON;
using UnityEngine;

/*Class to hold the subscriber info for the tip pose */

public class NeedleOffsets : ROSBridgeSubscriber {

	//Offset information
	public static uint[] needleOffsets;
	public static bool offsetsRosReceived = false;

	public new static string GetMessageTopic() {
		return "/offsets";
	}  

	public new static string GetMessageType() {
		return "std_msgs/UInt8MultiArray";
	}

	public new static ROSBridgeMsg ParseMessage(JSONNode msg) {
		return new UInt8MultiArrayMsg(msg);
	}


	public static void CallBack(ROSBridgeMsg msg) {

		//Process the received message
		//Debug.Log (GetMessageTopic () + " received");
		offsetsRosReceived = true;

		//Declare variables
		UInt8MultiArrayMsg offsets = (UInt8MultiArrayMsg)msg;
		//needleOffsets [0] = offsets.GetData;
	}
}
