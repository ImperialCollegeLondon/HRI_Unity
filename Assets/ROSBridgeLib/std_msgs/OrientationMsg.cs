using System.Collections;
using System.Text;
using SimpleJSON;

/**
 * Define a Orientation message. These have been hand-crafted from the PoseStamped message file (the quaternion)
 * 
 * Version History
 * 
 * @author Eloise Matheson
 * @version 1.0
 */

namespace ROSBridgeLib {
	namespace std_msgs {
		public class OrientationMsg : ROSBridgeMsg {
			private float _x, _y, _z, _w;

			public OrientationMsg(JSONNode msg) {
				_x = float.Parse(msg["x"]);
				_y = float.Parse(msg["y"]);
				_z = float.Parse(msg["z"]);
				_w = float.Parse(msg["w"]);
			}

			public OrientationMsg(float x, float y, float z, float w) {
				_x = x;
				_y = y;
				_z = z;
				_w = w;
			}

			public static string GetMessageType() {
				return "std_msgs/Orientation";
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

			public float GetW() {
				return _w;
			}

			public override string ToString() {
				return "std_msgs/Orientation [x=" + _x + ",  y=" + _y + ", z=" + _z + ", w=" + _w + "]";
			}

			public override string ToYAMLString() {
				return "{\"x\": " + _x + ", \"y\": " + _y + ", \"z\": " + _z + ", \"w\": " + _w +"}";
			}
		}
	}
}
