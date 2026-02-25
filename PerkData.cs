namespace PlayerChoice.DataSets;

public class PerkSet
{
	public static PerkInformation Coms_Influencers1	= new PerkInformation
	{
		YouthStat						= new EnumStructs.S_StatData
		{
			AgeGroup					= EnumStructs.E_Age.Young,
			Virality					= 9,
			Impact						= 0,
			Visibility					= 0,
		},
		AdultStat						= new EnumStructs.S_StatData
		{
			AgeGroup					= EnumStructs.E_Age.Adult,
			Virality					= 6,
			Impact						= 0,
			Visibility					= 0,
		},
		SeniorStat						= new EnumStructs.S_StatData
		{
			AgeGroup					= EnumStructs.E_Age.Senior,
			Virality					= 0,
			Impact						= 0,
			Visibility					= 0,
		},
		DependsOnPerk					= null,
		IsBoutght						= false,
		PerkCost						= 2,
	};

	public static PerkInformation Coms_Influencers2	= new PerkInformation
	{
		YouthStat						= new EnumStructs.S_StatData
		{
			AgeGroup					= EnumStructs.E_Age.Young,
			Virality					= 8,
			Impact						= 4,
			Visibility					= 4,
		},
		AdultStat						= new EnumStructs.S_StatData
		{
			AgeGroup					= EnumStructs.E_Age.Adult,
			Virality					= 4,
			Impact						= 2,
			Visibility					= 2,
		},
		SeniorStat						= new EnumStructs.S_StatData
		{
			AgeGroup					= EnumStructs.E_Age.Senior,
			Virality					= 2,
			Impact						= 1,
			Visibility					= 1,
		},
		DependsOnPerk					= Coms_Influencers1,
		IsBoutght						= false,
		PerkCost						= 3,
	};
}