using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

using Newtonsoft.Json;

namespace PlayerChoice.DataSets
{

public interface ISocialSitePost
{
	// If SocialPost_Type is NOT text, the SocialPost_Content is file path and SocialPost_Description is NULL
	public void Show_SocialPost(EnumStructs.E_PostType SocialPost_Type, string SocialPost_Content, string SocialPost_Description, string SocialPost_UserName, string SocialPost_PfP);
}

public class DataFunctions
{
	private static EventDataReceiver V_EventDataReceiver;
	private static SimulationHandler V_SimulationHandler;

	public static List<EventInfo> EventsList = new List<EventInfo>();
	public static List<PerkInformation> PurchasedPerk	= new List<PerkInformation>();

	public static List<string> OwnedPerks = new List<string>();
	public static int CurrentCollaborators = 0;

	//-----------------------------------------

	public static void SetEventDataReceiver(EventDataReceiver PAR_EDR)
	{
		V_EventDataReceiver	= PAR_EDR;
	}

	public static void SetSimulationHandler(SimulationHandler PAR_Handler)
	{
		V_SimulationHandler	= PAR_Handler;
	}

	public static void LoadEventsFromJSON(string PAR_FilePath)
	{
		StreamReader V_StrRead_EventsJSON	= new StreamReader(File.OpenRead(PAR_FilePath));
		string V_JsonContent				= V_StrRead_EventsJSON.ReadToEnd();
		V_StrRead_EventsJSON.Close();

		EventsList.Clear();

		foreach (EventInfo V_RawEvent in JsonConvert.DeserializeObject<EventInfo[]>(V_JsonContent))
		{
			EventsList.Add(V_RawEvent);
		}

		Debug.Log("[DataFunctions] Loaded " + EventsList.Count + " events from JSON.");
	}

	public static SocialPost_JSON LoadJSONFile(string PAR_FileName)
	{
		StreamReader V_StrRead_SocialJSON	= new StreamReader(File.OpenRead(PAR_FileName));

		SocialPost_JSON V_SocialJSON		= JsonConvert.DeserializeObject<SocialPost_JSON>(V_StrRead_SocialJSON.ReadToEnd());

		return V_SocialJSON;
	}

	public static bool CheckRequirements(EventInfo PAR_Event)
	{
		if(PurchasedPerk.Contains(PAR_Event.RequiredPerk)	== null)
		{
			return false;
		}

		if(PAR_Event.CollaboratorsRequired > 0 && CurrentCollaborators < PAR_Event.CollaboratorsRequired)
		{
			return false;
		}

		return true;
	}

	public static void SendEvent(EventInfo PAR_Event)
	{
		V_EventDataReceiver.SetEventData(PAR_Event.VideoPath, PAR_Event.EventDescription);
	}

	public static void ShowWebButtonOnUI()
	{

	}

	// Data changing the stats for young, adult, senior
	public static void SendDataToSimulation(EnumStructs.S_StatData[] PAR_StatData)
	{
		V_SimulationHandler?.ApplyStatData(PAR_StatData);
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