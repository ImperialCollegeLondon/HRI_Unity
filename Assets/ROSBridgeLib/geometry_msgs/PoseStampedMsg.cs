using System.Collections;
using System.Text;
using SimpleJSON;
using ROSBridgeLib.std_msgs;

/**
 * Define a PoseStamped message. These have been hand-crafted from the PoseStamped message file.
 * 
 * Version History
 * 
 * @author Riccardo Secoli
 * @version 1.0
 */

namespace ROSBridgeLib {
	namespace geometry_msgs {
		public class PoseStampedMsg : ROSBridgeMsg {
			private HeaderMsg _header;
			private PoseMsg _pose;
			
			public PoseStampedMsg(JSONNode msg) {
				_header = new HeaderMsg(msg["header"]);
				_pose = new PoseMsg(msg["pose"]);
			}

            public override string ToString()
            {
                return _pose.ToString();
            }

            public PoseMsg getPoseMsg()
            {
                return _pose;
            }
            public PositionMsg GetPositionMsg()
            {
                return _pose.GetPositionMsg();
            }

            public OrientationMsg GetOrientationMsg()
            {
                return _pose.GetOrientationMsg();
            }

            public override string ToYAMLString()
            {
                return _pose.ToYAMLString();
            }

        }
	}
}