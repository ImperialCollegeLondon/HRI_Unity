using System.Collections;
using System.Text;
using SimpleJSON;

/**
 * Define a Position message. These have been hand-crafted from the PoseStamped message file (the position).
 * 
 * Version History
 * 
 * @author Eloise Matheson
 * @version 1.0
 */

namespace ROSBridgeLib {
	namespace std_msgs {
		public class PositionMsg : ROSBridgeMsg {
			private float _x, _y, _z;

			public PositionMsg(JSONNode msg) {
				_x = float.Parse(msg["x"]);
				_y = float.Parse(msg["y"]);
				_z = float.Parse(msg["z"]);
			}

			public PositionMsg(float x, float y, float z) {
				_x = x;
				_y = y;
				_z = z;
			}

			public static string GetMessageType() {
				return "std_msgs/Position";
			}

			public float GetX() {
				return _x;
			}

			public float GetY() {
				return _y;
			}

			public float GetZ() {
				return _z;
			}


			public override string ToString() {
				return "std_msgs/Position [x=" + _x + ",  y=" + _y + ", z=" + _z + "]";
			}

			public override string ToYAMLString() {
				return "{\"x\": " + _x + ", \"y\": " + _y + ", \"z\": " + _z + "}";
			}
		}
	}
}



