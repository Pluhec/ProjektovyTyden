using System.Collections.Generic;

public static class NewsTestData
{
    public static List<NewsItem> GetAll()
    {
        return new List<NewsItem>
        {
            // DEMOCRATIC NEWS (0-33%): Neutral, balanced news
            new NewsItem {
                id = "dem1",
                title = "Parlament diskutuje o rozpočtu",
                text = "Poslanci dnes jednali o návrhu státního rozpočtu. Debata pokračuje zítra.",
                date = "2026-02-27",
                label = "AKTUÁLNĚ",
                minStance = 0f
            },
            new NewsItem {
                id = "dem2",
                title = "Občanská diskuze o klimatu",
                text = "V sobotu se uskuteční veřejná debata o klimatických opatřeních s účastí expertů.",
                date = "2026-02-26",
                label = "NOVINKA",
                minStance = 0f
            },
            new NewsItem {
                id = "dem3",
                title = "Setkání s občany v regionech",
                text = "Zástupci města vyrazili na turné po regionech, kde naslouchají místním potřebám.",
                date = "2026-02-25",
                label = "",
                minStance = 0f
            },
            new NewsItem {
                id = "dem4",
                title = "Nová studie o vzdělávání",
                text = "Univerzita zveřejnila komplexní analýzu současného stavu školství.",
                date = "2026-02-24",
                label = "",
                minStance = 0f
            },
            new NewsItem {
                id = "dem5",
                title = "Konference o místní správě",
                text = "Starostové se sejdou k výměně zkušeností o fungování samospráv.",
                date = "2026-02-23",
                label = "",
                minStance = 0f
            },

            // MODERATE NEWS (33-66%): Slight propaganda slant
            new NewsItem {
                id = "mod1",
                title = "Vláda schválila důležitý zákon",
                text = "Nové opatření přinese pokrok pro všechny občany. Opoziční hlasy byly vyslyšeny.",
                date = "2026-02-27",
                label = "AKTUÁLNĚ",
                minStance = 33f
            },
            new NewsItem {
                id = "mod2",
                title = "Prezident ocenil úspěšné projekty",
                text = "Vedení země vyzdvihlo iniciativy, které zlepšují kvalitu života obyvatel.",
                date = "2026-02-26",
                label = "NOVINKA",
                minStance = 33f
            },
            new NewsItem {
                id = "mod3",
                title = "Reforma přináší pozitivní změny",
                text = "Nová opatření ukazují první výsledky. Experti oceňují progresivní přístup.",
                date = "2026-02-25",
                label = "",
                minStance = 33f
            },
            new NewsItem {
                id = "mod4",
                title = "Investice do infrastruktury",
                text = "Vláda oznámila masivní program na modernizaci dopravní sítě.",
                date = "2026-02-24",
                label = "",
                minStance = 33f
            },
            new NewsItem {
                id = "mod5",
                title = "Spolupráce s partnery",
                text = "Uzavřeli jsme strategickou dohodu, která posílí naši pozici.",
                date = "2026-02-23",
                label = "",
                minStance = 33f
            },

            // PROPAGANDA NEWS (33-66%): Strong bias
            new NewsItem {
                id = "prop1",
                title = "Vedení poráží nepřátele pokroku",
                text = "Naše strana odmítla škodlivé návrhy destruktivní opozice. Lid stojí za námi!",
                date = "2026-02-27",
                label = "VÍTĚZSTVÍ",
                minStance = 33f
            },
            new NewsItem {
                id = "prop2",
                title = "Historický úspěch strany",
                text = "Pod naším vedením země dosáhla nevídaných výsledků. Alternativy vedou jen k chaosu.",
                date = "2026-02-26",
                label = "TRIUMF",
                minStance = 33f
            },
            new NewsItem {
                id = "prop3",
                title = "Ochrana národa před hrozbami",
                text = "Zavedli jsme nová opatření k ochraně před vnějšími a vnitřními nepřáteli.",
                date = "2026-02-25",
                label = "BEZPEČNOST",
                minStance = 33f
            },
            new NewsItem {
                id = "prop4",
                title = "Opozice šíří dezinformace",
                text = "Odhalili jsme koordinovanou kampaň lží ze strany destabilizačních sil.",
                date = "2026-02-24",
                label = "VAROVÁNÍ",
                minStance = 33f
            },
            new NewsItem {
                id = "prop5",
                title = "Masová podpora vedení",
                text = "Statisíce občanů vyjádřily podporu našemu kurzu. Lid ví, co je správné.",
                date = "2026-02-23",
                label = "JEDNOTA",
                minStance = 33f
            },

            // TOTALITARIAN NEWS (66-100%): Extreme propaganda
            new NewsItem {
                id = "tot1",
                title = "Nepřátelé národa zatčeni",
                text = "Bezpečnostní složky zadržely zrádce, kteří ohrožovali stabilitu státu. Jsou to nepřátelé lidu!",
                date = "2026-02-27",
                label = "POVINNÉ ČTENÍ",
                minStance = 66f
            },
            new NewsItem {
                id = "tot2",
                title = "Veliký vůdce zachránil národ",
                text = "Bez moudrého vedení by země padla do anarchie. Musíme být vděční za jeho oběti!",
                date = "2026-02-26",
                label = "SLAVNÝ DEN",
                minStance = 66f
            },
            new NewsItem {
                id = "tot3",
                title = "Nové zákony zaručují pořádek",
                text = "Centrální výbor rozhodl o posílení kontroly nad médii a veřejným prostorem. Je to nutné!",
                date = "2026-02-25",
                label = "DEKRET",
                minStance = 66f
            },
            new NewsItem {
                id = "tot4",
                title = "Disidenti jsou nebezpečím",
                text = "Občané jsou vyzváni k bdělosti. Jakékoli pochybnosti o vedení jsou zradou!",
                date = "2026-02-24",
                label = "VÝZVA",
                minStance = 66f
            },
            new NewsItem {
                id = "tot5",
                title = "Absolutní moc, absolutní pravda",
                text = "Vedení má vždy pravdu. Kdo pochybuje, je nepřítel. Není třeba diskutovat.",
                date = "2026-02-23",
                label = "OFICIÁLNÍ",
                minStance = 66f
            },
            new NewsItem {
                id = "tot6",
                title = "Totální kontrola médií zavedena",
                text = "Všechny zpravodajství musí být schváleno Ministerstvem pravdy. Svoboda je otroctví.",
                date = "2026-02-22",
                label = "NAŘÍZENÍ",
                minStance = 66f
            },
            new NewsItem {
                id = "tot7",
                title = "Surveillace pro vaši bezpečnost",
                text = "Nové kamery a odposlech chrání občany. Kdo se bojí, má něco na svědomí!",
                date = "2026-02-21",
                label = "OCHRANA",
                minStance = 66f
            }
        };
    }

    // Legacy method for backwards compatibility
    public static List<NewsItem> Get10()
    {
        return GetAll();
    }
}