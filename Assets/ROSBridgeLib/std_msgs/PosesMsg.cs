using System.Collections;
using System.Text;
using SimpleJSON;
using ROSBridgeLib.std_msgs;
using UnityEngine;

/**
 * Define a PoseStamped message. These have been hand-crafted from the PoseStamped message file.
 * 
 * Version History
 * 
 * @author Eloise Matheson
 * @version 1.0
 */

namespace ROSBridgeLib {
	namespace nav_msgs
    {
		public class PosesMsg : ROSBridgeMsg {
			public HeaderMsg _header;
			public PoseMsg[] _pose;
			
			public PosesMsg(JSONNode msg) {
                _header = new HeaderMsg(msg["header"]);
                
                JSONArray poses = (JSONArray)msg.AsArray["pose"];
                _pose = new PoseMsg[poses.Count];
                Debug.Log("path length: " + poses.Count);
                for (int index_ = 0; index_ < poses.Count; index_++){
                    _pose[index_] = new PoseMsg(poses[index_]);
                    Debug.Log("Path at " + index_ + " is:" + _pose[index_].ToString());
                }
            
                
                /*Debug.Log ("Inside the posesMsg class with the JSON node" + msg);

				//Work out how to iterate over the array 
				//The msg contains a string of an array of PoseStamped messages
				//int i = 1;
				Debug.Log("Poses message looks like: " + msg);
                
				foreach (var record in routes_list)
				{
					Console.WriteLine(record);
				}
				foreach(var header in msg)
				{
					//_header = new HeaderMsg(msg["header"]);
					//_pose = new PoseMsg(msg["poses"]);
					Debug.Log("header: " + i);
					i=i+1;
				}*/

            }
        }
	}
}