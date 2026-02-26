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
	public static List<EventInfo> EventsList = new List<EventInfo>();

	//-----------------------------------------


	public static List<EventInfo> LoadEventsFromJSON(string PAR_FilePath)
	{
		StreamReader V_StrRead_EventsJSON	= new StreamReader(File.OpenRead(PAR_FilePath));
		string V_JsonContent				= V_StrRead_EventsJSON.ReadToEnd();
		V_StrRead_EventsJSON.Close();

		EventsList.Clear();

		foreach (EventInfo V_RawEvent in JsonUtility.FromJson<EventInfo[]>(V_JsonContent))
		{
			EventsList.Add(V_RawEvent);
		}

		Debug.Log("[DataFunctions] Loaded " + EventsList.Count + " events from JSON.");
		return EventsList;
	}

    /*private static EnumStructs.S_EventOption ConvertOption(EventOptionJSON PAR_Raw)
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
	}*/


	public static SocialPost_JSON LoadJSONFile(string PAR_FileName)
	{
		StreamReader V_StrRead_SocialJSON	= new StreamReader(File.OpenRead(PAR_FileName));

		SocialPost_JSON V_SocialJSON		= JsonUtility.FromJson<SocialPost_JSON>(V_StrRead_SocialJSON.ReadToEnd());

		return V_SocialJSON;
	}

	public static void ShowWebButtonOnUI()
	{

	}

	// Data changing the stats for young, adult, senior
	public static void SendDataToSimulation(EnumStructs.S_StatData[] PAR_StatData)
	{
		
	}

	// Data changing democracy meter
	public static void InformChangeDemocracyMeter(int PAR_DemocracyStatAlter)
	{
		
	}

	public static void NotifyVictory()
	{
		
	}
}
}