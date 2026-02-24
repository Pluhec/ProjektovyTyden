using System.Runtime.InteropServices;

namespace PlayerChoice.DataSets;

public class PerkData
{
	public readonly EnumStructs.E_Perk PerkType;
	public readonly int PerkCost;
	public readonly EnumStructs.S_StatData YouthStat;
	public readonly EnumStructs.S_StatData AdultStat;
	public readonly EnumStructs.S_StatData SeniorStat;
	public readonly PerkData DependsOnPerk; // Can be null
	public bool IsBought;

	public int PerkPurchase(int PAR_CurrentMoney)
	{
		if(PAR_CurrentMoney		>= PerkCost && DependsOnPerk		== null)
		{
			IsBought			= true;
			return PerkCost;
		}
		else if(DependsOnPerk	!= null && DependsOnPerk.IsBought	== true)
		{
			IsBought			= true;
			return PerkCost;
		}
		else
		{
			return 0; // 0 means the purchase was refused
		}
	}
}

public class EnumStructs
{
	public struct S_StatData
	{
		E_Age AgeGroup;
		sbyte Virality;
		sbyte Impact;
		sbyte Visibility;
	}


	public enum E_Perk
	{
		Coms_ChainMail,
        Influencers_1,
        Influencers_2,
        Influencers_3
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
		Following	= 3
	}

	public enum E_PostType
	{
		Text,
		Image,
		Video
	}
}

// Influenceři 1
// PerkData influencers1 = new PerkData(
// 	EnumStructs.E_Perk.Influencers_1,
// 	200, // $$
// 	new EnumStructs.S_StatData(EnumStructs.E_Age.Young, 3, 0, 0),
// 	new EnumStructs.S_StatData(EnumStructs.E_Age.Adult, 2, 0, 0),
// 	new EnumStructs.S_StatData(EnumStructs.E_Age.Senior, 0, 0, 0)
// );