using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using PlayerChoice.DataSets;
using UnityEngine;

namespace PlayerChoice.Timing
{
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
	
	public static EnumStructs.E_GameStage V_GameStage = EnumStructs.E_GameStage.Start;
	private static System.Random V_Random			= new System.Random();

	public static string V_Str_ExePath	{get; private set;}
	private static string V_SocialBasePath	= "";


	public static void InvokeTimer()
	{
        V_Str_ExePath = Application.dataPath;
        if (Application.platform == RuntimePlatform.OSXPlayer)
        {
            V_Str_ExePath += "/../../";
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            V_Str_ExePath += "/../";
        }

            Debug.Log(V_Str_ExePath);
		V_Str_ExePath						= V_Str_ExePath.Substring(0, V_Str_ExePath.LastIndexOf("/"));
        V_SocialBasePath                    = Path.Combine(V_Str_ExePath, "Assets/PleyerDecisions/SocialMessagesJSON/");

		System.Timers.Timer V_GeneralTimer	= new System.Timers.Timer(V_TimerCallColldown);

		V_GeneralTimer.Start();

        V_GeneralTimer.Elapsed				+= (object PAR_Sender, ElapsedEventArgs PAR_Args)	=>
		{
			ProcessTimerStep();
		};
	}

	public static void ProcessTimerStep()
	{
        StrtRandPost();
	}

	public static void SelectSocialPost()
	{

	}
	
	public static void StrtRandPost()
	{
        /*if (V_Random.Next(0, 101) > V_SocialPostSpawnProb)
        {
            var V_IndependentMessagesFile   = Path.Combine(V_SocialBasePath, "IndependentMessages.json");
            var V_IndependentMessages       = DataFunctions.LoadIndependentMessages(V_IndependentMessagesFile);
            var V_MessageToPost             = V_IndependentMessages.IndependentZpravy[V_Random.Next(0, V_IndependentMessages.IndependentZpravy.Count)];
            
            DataFunctions.SpawnSocialPost(V_MessageToPost);
        }
        else
        {
            var V_UnlockedTopics = new List<EnumStructs.E_CampaignTopic>();
            if (PerkSet.Campaign_AntiImmigrants.IsBought) { V_UnlockedTopics.Add(EnumStructs.E_CampaignTopic.Imigrants); }
            if (PerkSet.Campaign_AntiNationalMinorities.IsBought) { V_UnlockedTopics.Add(EnumStructs.E_CampaignTopic.Naionality); }
            if (PerkSet.Campaign_AntiSexualMinorities.IsBought) { V_UnlockedTopics.Add(EnumStructs.E_CampaignTopic.Sexuality); }
            if (PerkSet.Campaign_AntiReligiousMinorities.IsBought) { V_UnlockedTopics.Add(EnumStructs.E_CampaignTopic.Religion); }
            if (PerkSet.Campaign_AntiElites.IsBought) { V_UnlockedTopics.Add(EnumStructs.E_CampaignTopic.Elites); }

            if (V_UnlockedTopics.Count > 0)
            {
                var V_SelectedTopic         = V_UnlockedTopics[V_Random.Next(0, V_UnlockedTopics.Count)];
                var V_DependentMessageFile  = Path.Combine(V_SocialBasePath, "DependentMessages", $"T{(int)V_SelectedTopic}_{V_SelectedTopic}Messages.json");
                var V_DependentMessages     = DataFunctions.LoadDependentMessages(V_DependentMessageFile);
                
                var V_StageKey = ((int)V_GameStage).ToString();

                if (V_DependentMessages.TryGetValue(V_StageKey, out var V_MessagesForStage))
                {
                    var V_MessageToPost = V_MessagesForStage[V_Random.Next(0, V_MessagesForStage.Count)];
                    DataFunctions.SpawnSocialPost(V_MessageToPost);
                }
            }
        }*/
	}
}
}