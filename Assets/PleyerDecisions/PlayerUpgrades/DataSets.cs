using System;

namespace PlayerChoice.DataSets
{
	[Serializable]
	public class SocialPost_JSON
	{
		public bool Type { get; set; }
		public EnumStructs.E_CampaignTopic Topic { get; set; }
		public EnumStructs.E_GameStage Agression { get; set; }
		public string Content { get; set; }
		public string Description { get; set; }
		public string UserImage { get; set; }
		public string UserName { get; set; }
	}

    /*[Serializable]
	public class EventsJSON_Wrapper
	{
		public EventJSON[] Events;
	}

	[Serializable]
	public class EventJSON
	{
		public int EventId;
		public string EventName;
		public string EventDescription;
		public string EventPredisposition;
		public EventOptionJSON OptionFree;
		public EventOptionJSON OptionMoney;
		public EventOptionJSON OptionPerk;
	}

	[Serializable]
	public class EventOptionJSON
	{
		public int OptionCost;
		public string OptionPerk;
		public string OptionName;
		public string OptionEffect;
		public StatDataJSON OptionEffectYoung;
		public StatDataJSON OptionEffectAdult;
		public StatDataJSON OptionEffectSenior;
	}

	[Serializable]
	public class StatDataJSON
	{
		public int AgeGroup;
		public int Virality;
		public int Impact;
		public int Visibility;
	}*/
}