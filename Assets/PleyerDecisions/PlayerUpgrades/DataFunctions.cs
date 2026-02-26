using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PlayerChoice.DataSets
{

public interface ISocialSitePost
{
	// If SocialPost_Type is NOT text, the SocialPost_Content is file path and SocialPost_Description is NULL
	public void Show_SocialPost(EnumStructs.E_PostType SocialPost_Type, string SocialPost_Content, string SocialPost_Description, string SocialPost_UserName, string SocialPost_PfP);
}

public class DataFunctions
{
	public delegate void D_SendSimulationInfo(EnumStructs.S_StatData[] StatAlterations);

	public static event D_SendSimulationInfo E_SendSimulationInfo;


	public delegate void D_DemocracyAlter(int DemocracyAlter);

	public static event D_DemocracyAlter E_DemocracyAlter;


	public delegate void D_SendSocialSitePost(EnumStructs.E_PostType SocialPost_Type, string SocialPost_Content, string SocialPost_Description, string SocialPost_UserName, string SocialPost_PfP);

	public static event D_SendSocialSitePost E_SendSocialSitePost;


	public static List<EventInfo> EventsList = new List<EventInfo>();


	//-----------------------------------------


	public static List<EventInfo> LoadEventsFromJSON(string PAR_FilePath)
	{
		StreamReader V_StrRead_EventsJSON	= new StreamReader(File.OpenRead(PAR_FilePath));
		string V_JsonContent				= V_StrRead_EventsJSON.ReadToEnd();
		V_StrRead_EventsJSON.Close();

		EventsJSON_Wrapper V_Wrapper		= JsonUtility.FromJson<EventsJSON_Wrapper>(V_JsonContent);

		EventsList.Clear();

		foreach (EventJSON V_RawEvent in V_Wrapper.Events)
		{
			EventInfo V_Event			= new EventInfo();
			V_Event.EventId				= V_RawEvent.EventId;
			V_Event.EventName			= V_RawEvent.EventName;
			V_Event.EventDescription	= V_RawEvent.EventDescription;
			//V_Event.EventPredisposition	= V_RawEvent.EventPredisposition;
			V_Event.OptionFree			= ConvertOption(V_RawEvent.OptionFree);
			V_Event.OptionMoney			= ConvertOption(V_RawEvent.OptionMoney);
			V_Event.OptionPerk			= ConvertOption(V_RawEvent.OptionPerk);

			EventsList.Add(V_Event);
		}

		Debug.Log("[DataFunctions] Loaded " + EventsList.Count + " events from JSON.");
		return EventsList;
	}

	private static EnumStructs.S_EventOption ConvertOption(EventOptionJSON PAR_Raw)
	{
		EnumStructs.S_EventOption V_Option	= new EnumStructs.S_EventOption();
		V_Option.OptionCost					= PAR_Raw.OptionCost;
		V_Option.OptionPerk					= null;
		V_Option.OptionName					= PAR_Raw.OptionName;
		V_Option.OptionEffect				= PAR_Raw.OptionEffect;
		V_Option.OptionEffectYoung			= ConvertStatData(PAR_Raw.OptionEffectYoung);
		V_Option.OptionEffectAdult			= ConvertStatData(PAR_Raw.OptionEffectAdult);
		V_Option.OptionEffectSenior			= ConvertStatData(PAR_Raw.OptionEffectSenior);
		return V_Option;
	}

	private static EnumStructs.S_StatData ConvertStatData(StatDataJSON PAR_Raw)
	{
		EnumStructs.S_StatData V_Stat	= new EnumStructs.S_StatData();
		V_Stat.AgeGroup					= (EnumStructs.E_Age)PAR_Raw.AgeGroup;
		V_Stat.Virality					= (sbyte)PAR_Raw.Virality;
		V_Stat.Impact					= (sbyte)PAR_Raw.Impact;
		V_Stat.Visibility				= (sbyte)PAR_Raw.Visibility;
		return V_Stat;
	}


	public static SocialPost_JSON LoadJSONFile(string PAR_FileName)
	{
		StreamReader V_StrRead_SocialJSON	= new StreamReader(File.OpenRead(PAR_FileName));

		SocialPost_JSON V_SocialJSON		= JsonUtility.FromJson<SocialPost_JSON>(V_StrRead_SocialJSON.ReadToEnd());

		return V_SocialJSON;
	}

	private static void SpawnSocialPost(SocialPost_JSON PAR_SocialPost)
	{
		EnumStructs.E_PostType V_PostType	= EnumStructs.E_PostType.Text;

		if(PAR_SocialPost.Type				= true)
		{
			V_PostType						=
				PAR_SocialPost.Content.EndsWith(".mp4")	== true ?
					EnumStructs.E_PostType.Video
					:
					EnumStructs.E_PostType.Image;
		}

		E_SendSocialSitePost.Invoke(V_PostType, PAR_SocialPost.Content, PAR_SocialPost.Description, PAR_SocialPost.UserName, PAR_SocialPost.UserImage);
	}

	public static void ShowWebButtonOnUI()
	{

	}

	public static void SendDataToSimulation(EnumStructs.S_StatData[] PAR_StatData)
	{
		DataFunctions.E_SendSimulationInfo.Invoke(PAR_StatData);
	}

	public static void InformChangeDemocracyMeter(int PAR_DemocracyStatAlter)
	{
		DataFunctions.E_DemocracyAlter.Invoke(PAR_DemocracyStatAlter);
	}

	public static void NotifyVictory()
	{
		
	}

	public static void StatAlteration(EnumStructs.S_StatData[] PAR_StatAlterations)
	{
		E_SendSimulationInfo.Invoke(PAR_StatAlterations);
	}
}
}