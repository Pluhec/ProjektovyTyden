using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PlayerChoice.DataSets
{
public class PlayerStats
{
	public static int Money = 10;
}

public class PerkInformation
{
	public string PerkName;
	public string PerkDescription;
	public int PerkCost;
	public EnumStructs.S_StatData YouthStat;
	public EnumStructs.S_StatData AdultStat;
	public EnumStructs.S_StatData SeniorStat;
	public PerkInformation[] DependsOnPerks; // Can be null or empty — ALL must be bought
	public bool IsBought;
	public EnumStructs.S_PerkSpecialEffect[] SpecialEffect; // NULL if there is no special effect

	
	public bool PerkPurchase()
	{
		if(PlayerStats.Money < PerkCost)
		{
			return false;
		}

		if(DependsOnPerks == null || DependsOnPerks.Length == 0)
		{
			return PurchaseChangeNotify();
		}

		foreach (var dep in DependsOnPerks)
		{
			if(!dep.IsBought)
			{
				return false; // A required dependency has not been purchased
			}
		}

		return PurchaseChangeNotify();
	}

	private bool PurchaseChangeNotify()
	{
		if(PerkName.Equals("Změna ústavy"))
		{
			DataFunctions.NotifyVictory();
		}

		if(PerkName.Equals("Založení politické strany"))
		{
			DataFunctions.ShowWebButtonOnUI();
		}
		PlayerStats.Money	-= PerkCost;
		IsBought = true;

		DataFunctions.SendDataToSimulation(new EnumStructs.S_StatData[] {YouthStat, AdultStat, SeniorStat});


		if(SpecialEffect	!= null)
		{
			foreach(var SEffect in SpecialEffect)
			{
				if(SEffect.EffectsType	== EnumStructs.E_PerkSpecialType.Democracy)
				{
        	         DataFunctions.InformChangeDemocracyMeter((int)SEffect.EffectAmmount);
				}
			}
		}

		return true;
	}
}

[Serializable]
public class EventInfo
{
	public int EventId;
	public string EventName;
	public string EventDescription;
	public string VideoPath;
	public EnumStructs.S_EventOption OptionFree;
	public EnumStructs.S_EventOption OptionMoney;
	public EnumStructs.S_EventOption OptionPerk;

	public PerkInformation? RequiredPerk;
	public byte? CollaboratorsRequired;

	public bool EventOption(int PAR_Option)
		{
			switch(PAR_Option)
			{
				case 0:
				    DataFunctions.SendDataToSimulation(new EnumStructs.S_StatData[] {OptionFree.OptionEffectYoung, OptionFree.OptionEffectAdult, OptionFree.OptionEffectSenior});
					return true;
				case 1:

					if(PlayerStats.Money < OptionMoney.OptionCost)
					{
						return false;
					}
					DataFunctions.SendDataToSimulation(new EnumStructs.S_StatData[] {OptionFree.OptionEffectYoung, OptionFree.OptionEffectAdult, OptionFree.OptionEffectSenior});
					return true;
				case 2:
					if(!OptionPerk.OptionPerk.IsBought)
					{
						return false;
					}

					DataFunctions.SendDataToSimulation(new EnumStructs.S_StatData[] {OptionFree.OptionEffectYoung, OptionFree.OptionEffectAdult, OptionFree.OptionEffectSenior});
					return false;
				default:
					return false;
			}
		}

		public EnumStructs.S_EventOption? GetEventOptionInfo(int PAR_Option)
		{
			switch(PAR_Option)
			{
				case 0:
					return OptionFree;
				case 1:
					return OptionMoney;
				case 2:
					return OptionPerk;
				default:
					return null;
			}
		}
}

public class EnumStructs
{
	public struct S_PerkSpecialEffect
	{
		public E_PerkSpecialType EffectsType;
		public E_Manipulatable? EffectsGroup; // IF EffectType is School, Democracy, FinancePopUp this is NULL
		public E_Education? EffectsEducation; // IF EffectType is SocialGroup, Democracy, FinancePopUp this is NULL
		public sbyte? EffectAmmount; // IF EffectType FinancePopUp this is NULL
	}

	[Serializable]
	public struct S_EventOption
	{
		public int OptionCost; // ZERO/NULL for free and Perk option
		public PerkInformation OptionPerk; // NULL for any option that Perk
		public string OptionName;
		public string OptionEffectdescription;
		public S_PerkSpecialEffect OptionSpecialEffect; // NULL if there is no special effect
		public S_StatData OptionEffectYoung;
		public S_StatData OptionEffectAdult;
		public S_StatData OptionEffectSenior;
	}

	[Serializable]
	public struct S_StatData
	{
		public E_Age AgeGroup;
		public sbyte Virality;
		public sbyte Impact;
		public sbyte Visibility;
	}

	public struct S_EventPredisposition
	{
		public sbyte Collaborator; // If not zero, it shall be considered as requirement
		public PerkInformation Perk; // If Class, it shall be considered as requirement
	}

	public enum E_PerkSpecialType
	{
		School,
		Democracy,
		FinancePopUp,
		SocialGroup,
		Visibility,
	}

	public enum E_CampaignTopic
	{
		Generic		= 0,
		Imigrants	= 1,
		Naionality	= 2,
		Sexuality	= 3,
		Religion	= 4,
		Elites		= 5,
	}
	public enum E_GameStage
	{
		None		= 0,
		Start		= 1,
		Middle		= 2,
		End			= 3
	}

	public enum E_Age
	{
		Young		= 0,
		Adult		= 1,
		Senior		= 2
	}

	public enum E_Education
	{
		Basic		= 0,
		Middle		= 1,
		High		= 2
	}

	public enum E_Manipulatable
	{
		Immune		= 0,
		Neutral		= 1,
		Sympathizing= 2,
		Collaborator= 3
	}

	public enum E_PostType
	{
		Text,
		Image,
		Video
	}
}
}