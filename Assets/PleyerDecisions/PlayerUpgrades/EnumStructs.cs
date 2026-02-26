using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PlayerChoice.DataSets
{
public class PlayerStats
{
	public static int Money;
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
		if(PAR_CurrentMoney < PerkCost)
		{
			return false;
		}

		if(DependsOnPerks == null || DependsOnPerks.Length == 0)
		{
			return SimulationChangeNotify();
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
			ShowWebButtonOnUI();
		}
		PlayerStats.Money	-= PerkCost;
		IsBought = true;

		DataFunctions.SendDataToSimulation(new EnumStructs.S_StatData[] {YouthStat, AdultStat, SeniorStat});

		foreach(var SEffect in SpecialEffect)
		{
			if(SEffect.EffectsType	== EnumStructs.E_PerkSpecialType.Democracy)
			{
				InformChangeDemocracyMeter(SEffect.EffectAmmount);
			}
		}

		return true;
	}
}

public class EventInfo
{
	public string EventName;
	public string EventDescription;
	public EnumStructs.S_EventOption OptionFree;
	public EnumStructs.S_EventOption OptionMoney;
	public EnumStructs.S_EventOption OptionPerk;

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

	public struct S_EventOption
	{
		public int OptionCost; // ZERO/NULL for free and Perk option
		public PerkInformation OptionPerk; // NULL for any option that Perk
		public string OptionName;
		public string OptionEffect;
		public S_StatData OptionEffectYoung;
		public S_StatData OptionEffectAdult;
		public S_StatData OptionEffectSenior;
	}

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