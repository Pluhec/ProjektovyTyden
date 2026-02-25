using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace PlayerChoice.DataSets
{
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

	public int PerkPurchase(int PAR_CurrentMoney)
	{
		if(PAR_CurrentMoney < PerkCost)
		{
			return 0;
		}

		if(DependsOnPerks == null || DependsOnPerks.Length == 0)
		{
			IsBought = true;
			return PerkCost;
		}

		foreach (var dep in DependsOnPerks)
		{
			if(!dep.IsBought)
			{
				return 0; // A required dependency has not been purchased
			}
		}

		IsBought = true;
		return PerkCost;
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
		public E_Manipulatable EffectsGroup; // IF EffectType is School, Democracy, FinancePopUp this is NULL
		public E_Education EffectsEducation; // IF EffectType is SocialGroup, Democracy, FinancePopUp this is NULL
		public sbyte EffectAmmount; // IF EffectType FinancePopUp this is NULL
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