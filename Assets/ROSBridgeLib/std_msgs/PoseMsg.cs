using System.Collections;
using System.Text;
using SimpleJSON;
using UnityEngine;

/**
 * Define a poseStamped message. These have been hand-crafted from the corresponding msg file.
 * 
 * Version History
 * 
 * @author Eloise Matheson
 * @version 1.0
 */

namespace ROSBridgeLib {
	namespace std_msgs {
		public class PoseMsg : ROSBridgeMsg {
			private PositionMsg _position;
			private OrientationMsg _orientation;

			public PoseMsg(JSONNode msg) {
				//Debug.Log ("PoseMsg with " + msg.ToString ());
				_position = new PositionMsg (msg ["position"]);
				_orientation = new OrientationMsg (msg["orientation"]);
				//Debug.Log ("PoseMsg done ");
				//Debug.Log (" and it looks like " + this.ToString ());
			}

			public PoseMsg(PositionMsg position, OrientationMsg orientation) {
				_position = position;
				_orientation = orientation;
			}

			public static string GetMessageType() {
				return "std_msgs/PoseMsg";
			}

			public PositionMsg GetPositionMsg() {
				return _position;
			}

			public OrientationMsg GetOrientationMsg() {
				return _orientation;
			}
			
			public override string ToString() {
				return "Pose [position=" + _position + ",  orientation=" + _orientation + "]";
			}

			public override string ToYAMLString() {
				return "{\"position\" : " + _position + ", \"orientation\" : " + _orientation.ToYAMLString () + "}";
			}
		}
	}
}
