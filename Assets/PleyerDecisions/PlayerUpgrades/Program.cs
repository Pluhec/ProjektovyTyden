//MARK: This is for testing outside of unity - Important are the other scripts files not this one
//MARK: God fuck Unity
using PlayerChoice.DataSets;

namespace PlayerChoice;

public class Program
{
	public static void Main()
	{
		string V_Str_ExePath		= Environment.ProcessPath.Replace("\\", "/");
		V_Str_ExePath				= V_Str_ExePath.Substring(0, V_Str_ExePath.LastIndexOf("/"));

		SocialPost_JSON V_SocJSON	= DataFunctions.LoadJSONFile(V_Str_ExePath+"/Test.json");

		Console.ForegroundColor		= ConsoleColor.DarkCyan;
		Console.WriteLine(V_SocJSON.Topic);



		// !! NO CODE BELOW THIS POINT!!
		Console.ForegroundColor		= ConsoleColor.White;
	}
}