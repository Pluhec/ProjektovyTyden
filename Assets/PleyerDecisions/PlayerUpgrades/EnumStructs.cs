using System.Runtime.InteropServices;

namespace PlayerChoice.DataSets;

public class PerkInformation
{
	public int PerkCost;
	public EnumStructs.S_StatData YouthStat;
	public EnumStructs.S_StatData AdultStat;
	public EnumStructs.S_StatData SeniorStat;
	public PerkInformation[] DependsOnPerks; // Can be null or empty — ALL must be bought
	public bool IsBought;
	public string SpecialEffect; // Description of special effects (e.g. "Removes 20% immune people")

	public int PerkPurchase(int PAR_CurrentMoney)
	{
		if (PAR_CurrentMoney < PerkCost)
			return 0;

		if (DependsOnPerks == null || DependsOnPerks.Length == 0)
		{
			IsBought = true;
			return PerkCost;
		}

		foreach (var dep in DependsOnPerks)
		{
			if (!dep.IsBought)
				return 0; // A required dependency has not been purchased
		}

		IsBought = true;
		return PerkCost;
	}
}

public class EnumStructs
{
	public struct S_StatData
	{
		public E_Age AgeGroup;
		public sbyte Virality;
		public sbyte Impact;
		public sbyte Visibility;
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