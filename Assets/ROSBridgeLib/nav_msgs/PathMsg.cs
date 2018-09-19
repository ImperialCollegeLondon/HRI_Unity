using System.Collections;
using System.Text;
using SimpleJSON;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.geometry_msgs;
using UnityEngine;
using System;
// Need geometry_msgs/PoseStamped[]

/**
 * Define a Path message, which should be an array of PoseStamped messages. These have been hand-crafted from the PoseStamped message file.
 * 
 * Version History
 * 
 * @author Riccardo Secoli
 * @version 1.0
 */

namespace ROSBridgeLib {
	namespace nav_msgs {
		public class PathMsg : ROSBridgeMsg {
			public HeaderMsg _header;
			public PoseStampedMsg[] _poses;
			
			public PathMsg(JSONNode msg) {

                _header = new HeaderMsg(msg["header"]);
                JSONNode poses = (JSONNode)msg["poses"];
                Array.Resize(ref _poses, poses.Count);
                
                //Debug.Log("path length: " + _poses.Length);

                for (int index_ = 0; index_ < _poses.Length; index_++)
                {
                    PoseStampedMsg tmp = new PoseStampedMsg(poses[index_]);
                   _poses.SetValue(tmp, index_);
                    //Debug.Log("Path at " + index_ + " is:" + tmp.ToString());
                }
            }

            public int getPathLength()
            {
                return _poses.Length;
            }

		}
	}
}