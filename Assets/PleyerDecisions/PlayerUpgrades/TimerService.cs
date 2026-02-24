using System.Timers;
using PlayerChoice.DataSets;

namespace PlayerChoice.Timing;

public class TimerBase
{
	public static byte V_TimerCallColldown	= 2;
	private static byte V_SocialSpawnProb;
	public static byte V_SocialPostSpawnProb
	{
		get { return V_SocialSpawnProb; }
		set { V_SocialSpawnProb		= (byte)(value	> 100 ? 100 : value); }
	}

	private static byte V_SpecificSpawnProb;
	public static byte V_SpecificPostSpawnProb
	{
		get { return V_SpecificSpawnProb; }
		set { V_SpecificSpawnProb	= (byte)(value	> 100 ? 100 : value); }
	}
	

	private static Random V_Random			= new Random();

	public static string V_Str_ExePath	{get; private set;}
	private static string V_SocialBasePath	= "";


	public static void InvokeTimer()
	{
		V_Str_ExePath						= Environment.ProcessPath.Replace("\\", "/");
		V_Str_ExePath						= V_Str_ExePath.Substring(0, V_Str_ExePath.LastIndexOf("/"));

		System.Timers.Timer V_GeneralTimer	= new System.Timers.Timer(V_TimerCallColldown);

		V_GeneralTimer.Start();

        V_GeneralTimer.Elapsed				+= (object PAR_Sender, ElapsedEventArgs PAR_Args)	=>
		{
			ProcessTimerStep();
		};
	}

	public static void ProcessTimerStep()
	{

	}

	public static void SelectSocialPost()
	{

	}
}