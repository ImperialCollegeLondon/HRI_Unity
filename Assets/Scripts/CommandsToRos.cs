using ROSBridgeLib;
using ROSBridgeLib.std_msgs;

public class CommandsToRos : ROSBridgePublisher
{

	public static string GetMessageTopic()
	{
		return "/cmdFromUnity";
	}

	public static string GetMessageType()
	{
		return "std_msgs/UInt8Msg";
	}

	public static string ToYAMLString(UInt8Msg msg)
	{
		return msg.ToYAMLString();
	}
}