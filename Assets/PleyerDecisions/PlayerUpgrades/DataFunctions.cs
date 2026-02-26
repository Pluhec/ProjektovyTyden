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
	private static EventDataReceiver V_EventDataReceiver;

	public static List<EventInfo> EventsList = new List<EventInfo>();

	//-----------------------------------------

	public static SetEventDataReceiver(EventDataReceiver PAR_EDR)
	{
		V_EventDataReceiver	= PAR_EDR;
	}

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

	public static SocialPost_JSON LoadJSONFile(string PAR_FileName)
	{
		StreamReader V_StrRead_SocialJSON	= new StreamReader(File.OpenRead(PAR_FileName));

		SocialPost_JSON V_SocialJSON		= JsonUtility.FromJson<SocialPost_JSON>(V_StrRead_SocialJSON.ReadToEnd());

		return V_SocialJSON;
	}

	public static void SendEvent(EventInfo PAR_Event)
	{
		V_EventDataReceiver.SetEventData(PAR_Event);
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