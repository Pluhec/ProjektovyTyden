namespace PlayerChoice.DataSets;

public class PerkSet
{
	// =====================================================================
	// 1. COMMUNICATION CHANNELS
	// =====================================================================

	// 1a. Influencers 1 (no dependency, cost $$)
	// Base stats: Virality +++, Impact 0, Visibility 0
	// Targeting: Young +++, Adult ++, Senior 0
	public static PerkInformation Coms_Influencers1 = new PerkInformation
	{
		PerkCost		= 2,
		DependsOnPerks = null,
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 9,  // 3 * 3
			Impact		= 0,  // 0 * 3
			Visibility = 0,  // 0 * 3
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 6,  // 3 * 2
			Impact		= 0,
			Visibility = 0,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
	};

	// 1a-i. Influencers 2 (depends on Influencers 1, cost $$$)
	// Base stats: Virality ++, Impact +, Visibility +
	// Targeting: Young ++++, Adult ++, Senior +
	public static PerkInformation Coms_Influencers2 = new PerkInformation
	{
		PerkCost		= 3,
		DependsOnPerks = new[] { Coms_Influencers1 },
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 8,  // 2 * 4
			Impact		= 4,  // 1 * 4
			Visibility = 4,  // 1 * 4
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 4,  // 2 * 2
			Impact		= 2,  // 1 * 2
			Visibility = 2,  // 1 * 2
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 2,  // 2 * 1
			Impact		= 1,  // 1 * 1
			Visibility = 1,  // 1 * 1
		},
	};

	// 1a-ii. Influencers 3 (depends on Influencers 2, cost $$$$)
	// Base stats: Virality +++, Impact ++, Visibility ++
	// Targeting: Young +++++, Adult +++, Senior +
	public static PerkInformation Coms_Influencers3 = new PerkInformation
	{
		PerkCost		= 4,
		DependsOnPerks = new[] { Coms_Influencers2 },
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 15, // 3 * 5
			Impact		= 10, // 2 * 5
			Visibility = 10, // 2 * 5
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 9,  // 3 * 3
			Impact		= 6,  // 2 * 3
			Visibility = 6,  // 2 * 3
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 3,  // 3 * 1
			Impact		= 2,  // 2 * 1
			Visibility = 2,  // 2 * 1
		},
	};

	// 1b. Chain Emails 1 (no dependency, cost $)
	// Base stats: Virality ++, Impact +, Visibility 0
	// Targeting: Young 0, Adult +, Senior +++
	public static PerkInformation Coms_ChainEmails1 = new PerkInformation
	{
		PerkCost		= 1,
		DependsOnPerks = null,
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 2,  // 2 * 1
			Impact		= 1,  // 1 * 1
			Visibility = 0,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 6,  // 2 * 3
			Impact		= 3,  // 1 * 3
			Visibility = 0,
		},
	};

	// 1b-i. Chain Emails 2 (depends on Chain Emails 1, cost $)
	// Base stats: Virality ++, Impact +, Visibility 0
	// Targeting: Young 0, Adult +, Senior +++
	public static PerkInformation Coms_ChainEmails2 = new PerkInformation
	{
		PerkCost		= 1,
		DependsOnPerks = new[] { Coms_ChainEmails1 },
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 2,  // 2 * 1
			Impact		= 1,  // 1 * 1
			Visibility = 0,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 6,  // 2 * 3
			Impact		= 3,  // 1 * 3
			Visibility = 0,
		},
	};

	// 1c. TV Channel (no dependency, cost $$$$)
	// Base stats: Virality ++, Impact ++, Visibility ++
	// Targeting: Young 0, Adult ++, Senior ++
	// Special: Generates clickable money bubbles on the map
	public static PerkInformation Coms_TVChannel = new PerkInformation
	{
		PerkCost		= 4,
		DependsOnPerks = null,
		IsBought		= false,
		SpecialEffect  = "Occasionally generates a clickable bubble on the map that grants money",
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 4,  // 2 * 2
			Impact		= 4,  // 2 * 2
			Visibility = 4,  // 2 * 2
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 4,  // 2 * 2
			Impact		= 4,  // 2 * 2
			Visibility = 4,  // 2 * 2
		},
	};

	// 1d. Radio (no dependency, cost $$$)
	// Base stats: Virality +, Impact +, Visibility ++
	// Targeting: Young 0, Adult ++, Senior ++
	public static PerkInformation Coms_Radio = new PerkInformation
	{
		PerkCost		= 3,
		DependsOnPerks = null,
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 2,  // 1 * 2
			Impact		= 2,  // 1 * 2
			Visibility = 4,  // 2 * 2
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 2,  // 1 * 2
			Impact		= 2,  // 1 * 2
			Visibility = 4,  // 2 * 2
		},
	};

	// 1e. Newspapers and Flyers (no dependency, cost $$)
	// Base stats: Virality 0, Impact +, Visibility +
	// Targeting: Young 0, Adult +, Senior ++
	public static PerkInformation Coms_NewspapersFlyers = new PerkInformation
	{
		PerkCost		= 2,
		DependsOnPerks = null,
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 0,  // 0 * 1
			Impact		= 1,  // 1 * 1
			Visibility = 1,  // 1 * 1
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 0,  // 0 * 2
			Impact		= 2,  // 1 * 2
			Visibility = 2,  // 1 * 2
		},
	};

	// 1f. Social Media (no dependency, cost 0)
	// Base stats: Virality 0, Impact 0, Visibility 0
	// Targeting: Young ++, Adult ++, Senior +
	// Note: All stats are 0, serves as gateway perk for sub-perks
	public static PerkInformation Coms_SocialMedia = new PerkInformation
	{
		PerkCost		= 0,
		DependsOnPerks = null,
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
	};

	// 1f-i. Internet TV (depends on Social Media, cost $$)
	// Base stats: Virality 1, Impact 2, Visibility 1
	// Targeting: Young ++, Adult ++, Senior +
	public static PerkInformation Coms_InternetTV = new PerkInformation
	{
		PerkCost		= 2,
		DependsOnPerks = new[] { Coms_SocialMedia },
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 2,  // 1 * 2
			Impact		= 4,  // 2 * 2
			Visibility = 2,  // 1 * 2
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 2,  // 1 * 2
			Impact		= 4,  // 2 * 2
			Visibility = 2,  // 1 * 2
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 1,  // 1 * 1
			Impact		= 2,  // 2 * 1
			Visibility = 1,  // 1 * 1
		},
	};

	// 1f-ii. Fake News and Conspiracy Theories (depends on Social Media, cost $$)
	// Base stats: Virality 2, Impact 1, Visibility 1
	// Targeting: Young +, Adult ++, Senior ++
	public static PerkInformation Coms_FakeNews = new PerkInformation
	{
		PerkCost		= 2,
		DependsOnPerks = new[] { Coms_SocialMedia },
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 2,  // 2 * 1
			Impact		= 1,  // 1 * 1
			Visibility = 1,  // 1 * 1
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 4,  // 2 * 2
			Impact		= 2,  // 1 * 2
			Visibility = 2,  // 1 * 2
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 4,  // 2 * 2
			Impact		= 2,  // 1 * 2
			Visibility = 2,  // 1 * 2
		},
	};

	// 1f-iii. Bots 1 (depends on Social Media, cost $$)
	// Base stats: Virality +++, Impact +, Visibility +
	// Targeting: Young +, Adult ++, Senior ++
	public static PerkInformation Coms_Bots1 = new PerkInformation
	{
		PerkCost		= 2,
		DependsOnPerks = new[] { Coms_SocialMedia },
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 3,  // 3 * 1
			Impact		= 1,  // 1 * 1
			Visibility = 1,  // 1 * 1
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 6,  // 3 * 2
			Impact		= 2,  // 1 * 2
			Visibility = 2,  // 1 * 2
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 6,  // 3 * 2
			Impact		= 2,  // 1 * 2
			Visibility = 2,  // 1 * 2
		},
	};

	// 1f-iv. Bots 2 (depends on Bots 1, cost $$$)
	// Base stats: Virality ++++, Impact ++, Visibility ++
	// Targeting: Young ++, Adult +++, Senior +++
	public static PerkInformation Coms_Bots2 = new PerkInformation
	{
		PerkCost		= 3,
		DependsOnPerks = new[] { Coms_Bots1 },
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 8,  // 4 * 2
			Impact		= 4,  // 2 * 2
			Visibility = 4,  // 2 * 2
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 12, // 4 * 3
			Impact		= 6,  // 2 * 3
			Visibility = 6,  // 2 * 3
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 12, // 4 * 3
			Impact		= 6,  // 2 * 3
			Visibility = 6,  // 2 * 3
		},
	};

	// 1f-v. Own Social Network (depends on Internet TV, Fake News, Bots 2, Influencers 3, cost $$$$$)
	// Base stats: Virality ++++, Impact ++++, Visibility ++++
	// Targeting: Young +++, Adult +++, Senior ++
	public static PerkInformation Coms_OwnSocialNetwork = new PerkInformation
	{
		PerkCost		= 5,
		DependsOnPerks = new[] { Coms_InternetTV, Coms_FakeNews, Coms_Bots2, Coms_Influencers3 },
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 12, // 4 * 3
			Impact		= 12, // 4 * 3
			Visibility = 12, // 4 * 3
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 12, // 4 * 3
			Impact		= 12, // 4 * 3
			Visibility = 12, // 4 * 3
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 8,  // 4 * 2
			Impact		= 8,  // 4 * 2
			Visibility = 8,  // 4 * 2
		},
	};

	// 1g. Online Games (no dependency, cost $$$)
	// Base stats: Virality +++, Impact +, Visibility 0
	// Targeting: Young +++, Adult +, Senior 0
	public static PerkInformation Coms_OnlineGames = new PerkInformation
	{
		PerkCost		= 3,
		DependsOnPerks = null,
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 9,  // 3 * 3
			Impact		= 3,  // 1 * 3
			Visibility = 0,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 3,  // 3 * 1
			Impact		= 1,  // 1 * 1
			Visibility = 0,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
	};

	// =====================================================================
	// 2. MANIFESTATIONS
	// =====================================================================

	// --- 2a. Campaign Topics ---
	// Campaign topics primarily expand event options and affect the dynamic election website.
	// Each opened topic increases visibility. Stats are DIRECT values (not multiplied).
	// Virality and Impact per age group, Visibility = 1 globally per topic.

	// 2a-i. Anti-Immigrants (no dependency, cost 2)
	// Young: V-, I--  |  Adult: V+, I+  |  Senior: V++, I++
	public static PerkInformation Campaign_AntiImmigrants = new PerkInformation
	{
		PerkName = "Anti-Immigrantská kampaň",
		PerkDescription = "Otevře kampaň proti imigrantům.",
		PerkCost		= 2,
		DependsOnPerks = null,
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = -1,
			Impact		= -2,
			Visibility = 1,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 1,
			Impact		= 1,
			Visibility = 1,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 2,
			Impact		= 2,
			Visibility = 1,
		},
	};

	// 2a-ii. Anti-National Minorities (no dependency, cost 2)
	// Young: V-, I-  |  Adult: V-, I+  |  Senior: V+, I+
	public static PerkInformation Campaign_AntiNationalMinorities = new PerkInformation
	{
		PerkName = "Anti-Národnostní menšiny",
		PerkDescription = "Otevře kampaň proti národnostním menšinám.",
		PerkCost		= 2,
		DependsOnPerks = null,
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = -1,
			Impact		= -1,
			Visibility = 1,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = -1,
			Impact		= 1,
			Visibility = 1,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 1,
			Impact		= 1,
			Visibility = 1,
		},
	};

	// 2a-iii. Anti-Sexual Minorities (no dependency, cost 2)
	// Young: V---, I---  |  Adult: V+, I+  |  Senior: V++, I++
	public static PerkInformation Campaign_AntiSexualMinorities = new PerkInformation
	{
		PerkName = "Anti-Sexuální menšiny",
		PerkDescription = "Otevře kampaň proti sexuálním menšinám.",
		PerkCost		= 2,
		DependsOnPerks = null,
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = -3,
			Impact		= -3,
			Visibility = 1,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 1,
			Impact		= 1,
			Visibility = 1,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 2,
			Impact		= 2,
			Visibility = 1,
		},
	};

	// 2a-iv. Anti-Religious Minorities (no dependency, cost 2)
	// Young: V--, I-  |  Adult: V+, I+  |  Senior: V++, I++
	public static PerkInformation Campaign_AntiReligiousMinorities = new PerkInformation
	{
		PerkName = "Proti náboženským menšinám",
		PerkDescription = "Otevírá kampaň proti náboženský menšinám",
		PerkCost		= 2,
		DependsOnPerks = null,
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = -2,
			Impact		= -1,
			Visibility = 1,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 1,
			Impact		= 1,
			Visibility = 1,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 2,
			Impact		= 2,
			Visibility = 1,
		},
	};

	// 2a-v. Anti-Elites (no dependency, cost 2)
	// Young: V+++, I++  |  Adult: V+, I+  |  Senior: V--, I-
	public static PerkInformation Campaign_AntiElites = new PerkInformation
	{
		PerkName = "Anti-elity",
		PerkDescription = "otevírá kampaň proti elitám",
		PerkCost		= 2,
		DependsOnPerks = null,
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 3,
			Impact		= 2,
			Visibility = 1,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 1,
			Impact		= 1,
			Visibility = 1,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = -2,
			Impact		= -1,
			Visibility = 1,
		},
	};

	// --- 2b. Common Sense (no dependency, cost 2) ---
	// Base stats: Virality 1, Impact 1, Visibility 1
	// Targeting: Young +, Adult ++, Senior +++
	// Special: University-educated people -5%
	public static PerkInformation Manif_CommonSense = new PerkInformation
	{
		PerkName = "Selský rozum",
		PerkDescription = "Zvyšuje virálnost, dopad a viditelnost kampaní mezi všemi věkovými skupinami, ale snižuje je mezi vysokoškolsky vzdělanými lidmi.",
		PerkCost		= 2,
		DependsOnPerks = null,
		IsBought		= false,
		SpecialEffect  = new[]
		{
			new EnumStructs.S_PerkSpecialEffect
			{
				EffectsType = EnumStructs.E_PerkSpecialType.School,
				EffectsEducation = EnumStructs.E_Education.High,
				EffectsGroup = null,
				EffectAmmount = -5, 
			}
		},
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 1,  // 1 * 1
			Impact		= 1,  // 1 * 1
			Visibility = 1,  // 1 * 1
		},
		AdultStat = new EnumStructs.S_StatData 
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 2,  // 1 * 2
			Impact		= 2,  // 1 * 2
			Visibility = 2,  // 1 * 2
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 3,  // 1 * 3
			Impact		= 3,  // 1 * 3
			Visibility = 3,  // 1 * 3
		},
	};

	// 2b-i. Truth Relativism (depends on Common Sense, cost 2)
	// Base stats: Virality 1, Impact 1, Visibility 1
	// Targeting: Young +, Adult ++, Senior +++
	// Special: University-educated people -5%
	public static PerkInformation Manif_TruthRelativism = new PerkInformation
	{
		PerkName = "Relativizace pravdy",
		PerkDescription = "Zvyšuje virálnost, dopad a viditelnost kampaní mezi všemi věkovými skupinami, ale snižuje je mezi vysokoškolsky vzdělanými lidmi.",
		PerkCost		= 2,
		DependsOnPerks = new[] { Manif_CommonSense },
		IsBought		= false,
		SpecialEffect  = new[]
		{
			new EnumStructs.S_PerkSpecialEffect
			{
				EffectsType = EnumStructs.E_PerkSpecialType.School,
				EffectsEducation = EnumStructs.E_Education.High,
				EffectsGroup = null,
				EffectAmmount = -5, 
			}
		},
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 1,
			Impact		= 1,
			Visibility = 1,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 2,
			Impact		= 2,
			Visibility = 2,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 3,
			Impact		= 3,
			Visibility = 3,
		},
	};

	// 2c. Demonstrations (no dependency, cost 4)
	// Base stats: Virality 2, Impact 2, Visibility 2
	// Targeting: Young +, Adult ++, Senior +++
	public static PerkInformation Manif_Demonstrations1 = new PerkInformation
	{
		PerkName = "Demonstrace 1",
		PerkDescription = "Zvyšuje virálnost, dopad a viditelnost demonstrací mezi všemi věkovými skupinami.",
		PerkCost		= 4,
		DependsOnPerks = null,
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 2,  // 2 * 1
			Impact		= 2,  // 2 * 1
			Visibility = 2,  // 2 * 1
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 4,  // 2 * 2
			Impact		= 4,  // 2 * 2
			Visibility = 4,  // 2 * 2
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 6,  // 2 * 3
			Impact		= 6,  // 2 * 3
			Visibility = 6,  // 2 * 3
		},
	};

	// 2c-i. Demonstrations 2 (depends on Demonstrations, cost 4)
	// Base stats: Virality 2, Impact 2, Visibility 2
	// Targeting: Young +, Adult ++, Senior +++
	// Special: Generates clickable money bubbles on the map
	public static PerkInformation Manif_Demonstrations2 = new PerkInformation
	{
		PerkName = "Demonstrace 2",
		PerkDescription = "Vylepšuje demonstrace a občas generuje peněžní bubliny na mapě.",
		PerkCost		= 4,
		DependsOnPerks = new[] { Manif_Demonstrations1 },
		IsBought		= false,
		SpecialEffect  = new[] // Generates clickable money bubbles on the map
        {
            new EnumStructs.S_PerkSpecialEffect
            {
                EffectsType = EnumStructs.E_PerkSpecialType.FinancePopUp,
                EffectsGroup = null,
                EffectsEducation = null,
                EffectAmmount = null
            }
        },
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 2,
			Impact		= 2,
			Visibility = 2,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 4,
			Impact		= 4,
			Visibility = 4,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 6,
			Impact		= 6,
			Visibility = 6,
		},
	};

	// 2c-ii. Paramilitary Groups (depends on Demonstrations 2, cost 4)
	// Base stats: Virality 3, Impact 3, Visibility 5
	// Targeting: Young +, Adult ++, Senior +++
	// Special: Followers +10%, Immune people +5%
	public static PerkInformation Manif_ParamilitaryGroups = new PerkInformation
	{
		PerkName = "Polovojenské skupiny",
		PerkDescription = "Zvyšuje virálnost, dopad a viditelnost demonstrací mezi všemi věkovými skupinami a zvyšuje počet následovníků.",
		PerkCost		= 4,
		DependsOnPerks = new[] { Manif_Demonstrations2 },
		IsBought		= false,
		SpecialEffect  = new[] // Followers +10%, Immune people +5%
        {
            new EnumStructs.S_PerkSpecialEffect
            {
                EffectsType = EnumStructs.E_PerkSpecialType.SocialGroup,
                EffectsGroup = EnumStructs.E_Manipulatable.Collaborator,
                EffectsEducation = null,
                EffectAmmount = 10
            },
            new EnumStructs.S_PerkSpecialEffect
            {
                EffectsType = EnumStructs.E_PerkSpecialType.SocialGroup,
                EffectsGroup = EnumStructs.E_Manipulatable.Immune,
                EffectsEducation = null,
                EffectAmmount = 5
            }
        },
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 3,  // 3 * 1
			Impact		= 3,  // 3 * 1
			Visibility = 5,  // 5 * 1
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 6,  // 3 * 2
			Impact		= 6,  // 3 * 2
			Visibility = 10, // 5 * 2
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 9,  // 3 * 3
			Impact		= 9,  // 3 * 3
			Visibility = 15, // 5 * 3
		},
	};

	// 2d. Verbal Aggressivity (no dependency, cost 3)
	// Base stats: Virality 2, Impact 2, Visibility 3
	// Targeting: Young ++, Adult ++, Senior ++
	public static PerkInformation Manif_VerbalAggressivity = new PerkInformation
	{
		PerkName = "Verbální agresivita",
		PerkDescription = "Zvyšuje virálnost, dopad a viditelnost kampaní mezi všemi věkovými skupinami.",
		PerkCost		= 3,
		DependsOnPerks = null,
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 4,  // 2 * 2
			Impact		= 4,  // 2 * 2
			Visibility = 6,  // 3 * 2
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 4,  // 2 * 2
			Impact		= 4,  // 2 * 2
			Visibility = 6,  // 3 * 2
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 4,  // 2 * 2
			Impact		= 4,  // 2 * 2
			Visibility = 6,  // 3 * 2
		},
	};

	// 2e. Boycott and Economic Pressure (no dependency, cost 3)
	// Base stats: Virality 3, Impact 2, Visibility 2
	// Targeting: Young +, Adult ++, Senior ++
	public static PerkInformation Manif_BoycottEconomicPressure = new PerkInformation
	{
		PerkName = "Bojkot a ekonomický tlak",
		PerkDescription = "Zvyšuje virálnost, dopad a viditelnost kampaní mezi všemi věkovými skupinami.",
		PerkCost		= 3,
		DependsOnPerks = null,
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 3,  // 3 * 1
			Impact		= 2,  // 2 * 1
			Visibility = 2,  // 2 * 1
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 6,  // 3 * 2
			Impact		= 4,  // 2 * 2
			Visibility = 4,  // 2 * 2
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 6,  // 3 * 2
			Impact		= 4,  // 2 * 2
			Visibility = 4,  // 2 * 2
		},
	};

	// =====================================================================
	// 3. ACTIONS
	// =====================================================================

	// 3a. Encrypted Communications 1 (no dependency, cost $$$)
	// Special: Reduces democracy meter by 10%
	// No combat stats — purely strategic effect
	public static PerkInformation Action_EncryptedComms1 = new PerkInformation
	{
		PerkCost		= 3,
		DependsOnPerks = null,
		IsBought		= false,
		SpecialEffect  = "Reduces democracy meter by 10%",
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
	};

	// 3a-i. Encrypted Communications 2 (depends on Encrypted Communications 1, cost $$$$$)
	// Special: Reduces democracy meter by 15%
	public static PerkInformation Action_EncryptedComms2 = new PerkInformation
	{
		PerkCost		= 5,
		DependsOnPerks = new[] { Action_EncryptedComms1 },
		IsBought		= false,
		SpecialEffect  = "Reduces democracy meter by 15%",
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
	};

	// 3b. Fake Compromising Materials (no dependency, cost $$$)
	// Base stats: Virality +++, Impact ++, Visibility ++
	// No specific age targeting — uniform across all groups
	public static PerkInformation Action_FakeCompromising = new PerkInformation
	{
		PerkCost		= 3,
		DependsOnPerks = null,
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 3,
			Impact		= 2,
			Visibility = 2,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 3,
			Impact		= 2,
			Visibility = 2,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 3,
			Impact		= 2,
			Visibility = 2,
		},
	};

	// 3c. Found Political Party (special dependency: 25% followers in population, cost $$$$$)
	// Base stats: Virality +++++, Impact ++++, Visibility ++++
	// Targeting: Young +, Adult ++, Senior +++
	public static PerkInformation Action_PoliticalParty = new PerkInformation
	{
		PerkCost		= 5,
		DependsOnPerks = null, // Special condition: requires 25% followers — checked in game logic
		IsBought		= false,
		SpecialEffect  = "Requires at least 25% followers in the population to unlock",
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 5,  // 5 * 1
			Impact		= 4,  // 4 * 1
			Visibility = 4,  // 4 * 1
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 10, // 5 * 2
			Impact		= 8,  // 4 * 2
			Visibility = 8,  // 4 * 2
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 15, // 5 * 3
			Impact		= 12, // 4 * 3
			Visibility = 12, // 4 * 3
		},
	};

	// 3d. Political Campaign 1 (depends on Political Party, cost $$$$)
	// Strategic milestone perk
	public static PerkInformation Action_PoliticalCampaign1 = new PerkInformation
	{
		PerkCost		= 4,
		DependsOnPerks = new[] { Action_PoliticalParty },
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
	};

	// 3d-i. Political Campaign 2 (depends on Political Campaign 1, cost $$$$)
	public static PerkInformation Action_PoliticalCampaign2 = new PerkInformation
	{
		PerkCost		= 4,
		DependsOnPerks = new[] { Action_PoliticalCampaign1 },
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
	};

	// 3e. Elected to Parliament (depends on Political Campaign 2, cost 0)
	public static PerkInformation Action_ElectedToParliament = new PerkInformation
	{
		PerkCost		= 0,
		DependsOnPerks = new[] { Action_PoliticalCampaign2 },
		IsBought		= false,
		SpecialEffect  = "Elected to parliament — unlocks government actions",
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
	};

	// 3e-i. Government Participation (depends on Elected to Parliament, cost $$$$)
	public static PerkInformation Action_GovernmentParticipation = new PerkInformation
	{
		PerkCost		= 4,
		DependsOnPerks = new[] { Action_ElectedToParliament },
		IsBought		= false,
		SpecialEffect  = "Participation in government — unlocks authoritarian actions",
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
	};

	// 3e-ii. Intimidate Political Opponents (depends on Government Participation, cost $$)
	// Visibility ++, Virality +, Impact -
	public static PerkInformation Action_IntimidateOpponents = new PerkInformation
	{
		PerkCost		= 2,
		DependsOnPerks = new[] { Action_GovernmentParticipation },
		IsBought		= false,
		SpecialEffect  = null,
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 1,
			Impact		= -1,
			Visibility = 2,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 1,
			Impact		= -1,
			Visibility = 2,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 1,
			Impact		= -1,
			Visibility = 2,
		},
	};

	// 3e-ii-1. Persecute Undesirables 1 (depends on Intimidate Opponents, cost $$)
	// Special: Removes 20% immune people from the map
	public static PerkInformation Action_PersecuteUndesirables1 = new PerkInformation
	{
		PerkCost		= 2,
		DependsOnPerks = new[] { Action_IntimidateOpponents },
		IsBought		= false,
		SpecialEffect  = "Removes 20% of immune people from the map",
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
	};

	// 3e-ii-2. Persecute Undesirables 2 (depends on Persecute Undesirables 1, cost $$)
	// Special: Removes another 20% immune people from the map
	public static PerkInformation Action_PersecuteUndesirables2 = new PerkInformation
	{
		PerkCost		= 2,
		DependsOnPerks = new[] { Action_PersecuteUndesirables1 },
		IsBought		= false,
		SpecialEffect  = "Removes 20% of immune people from the map",
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
	};

	// 3e-iii. Media Control (depends on Government Participation, cost $$)
	// Special: Reduces visibility by 20%, increases immune people by 10%
	public static PerkInformation Action_MediaControl = new PerkInformation
	{
		PerkCost		= 2,
		DependsOnPerks = new[] { Action_GovernmentParticipation },
		IsBought		= false,
		SpecialEffect  = "Reduces visibility by 20%, increases immune people by 10%",
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
	};

	// 3e-iii-1. Intimidate Journalists (depends on Media Control, cost $$)
	// Special: Reduces visibility by 20%
	public static PerkInformation Action_IntimidateJournalists = new PerkInformation
	{
		PerkCost		= 2,
		DependsOnPerks = new[] { Action_MediaControl },
		IsBought		= false,
		SpecialEffect  = "Reduces visibility by 20%",
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
	};

	// 3e-iii-2. Introduce Censorship (depends on Intimidate Journalists, cost $$)
	// Special: Reduces visibility by 20%
	public static PerkInformation Action_IntroduceCensorship = new PerkInformation
	{
		PerkCost		= 2,
		DependsOnPerks = new[] { Action_IntimidateJournalists },
		IsBought		= false,
		SpecialEffect  = "Reduces visibility by 20%",
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
	};

	// 3e-iv. Change Constitution (depends on Introduce Censorship AND Persecute Undesirables 1, cost $$$)
	// Special: END OF GAME — totalitarian regime established
	public static PerkInformation Action_ChangeConstitution = new PerkInformation
	{
		PerkCost		= 3,
		DependsOnPerks = new[] { Action_IntroduceCensorship, Action_PersecuteUndesirables1 },
		IsBought		= false,
		SpecialEffect  = "END OF GAME — totalitarian regime fully established",
		YouthStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Young,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		AdultStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Adult,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
		SeniorStat = new EnumStructs.S_StatData
		{
			AgeGroup   = EnumStructs.E_Age.Senior,
			Virality   = 0,
			Impact		= 0,
			Visibility = 0,
		},
	};
}