using System.Collections.Generic;

public static class NewsTestData
{
    public static List<NewsItem> Get10()
    {
        return new List<NewsItem>
        {
            new NewsItem {
                id = "1",
                title = "Spouštíme novou kampaň",
                text = "Dnes oficiálně startuje naše nová iniciativa zaměřená na budoucnost planety.",
                date = "2026-02-26",
                label = "NOVINKA"
            },
            new NewsItem {
                id = "2",
                title = "Změny v organizaci",
                text = "Dochází k úpravě interních procesů a přerozdělení kompetencí.",
                date = "2026-02-20",
                label = "NOVINKA"
            },
            new NewsItem {
                id = "3",
                title = "Nový mediální výstup",
                text = "Publikovali jsme rozhovor s předsedou hnutí o aktuálních tématech.",
                date = "2026-02-15",
                label = "NOVINKA"
            },
            new NewsItem {
                id = "4",
                title = "Setkání s občany",
                text = "Proběhlo veřejné setkání s občany v Brně.",
                date = "2026-02-10",
                label = ""
            },
            new NewsItem {
                id = "5",
                title = "Nový programový bod",
                text = "Představili jsme návrh nového programového opatření.",
                date = "2026-02-05",
                label = ""
            },
            new NewsItem {
                id = "6",
                title = "Tisková konference",
                text = "Konference proběhla za účasti médií i veřejnosti.",
                date = "2026-01-28",
                label = ""
            },
            new NewsItem {
                id = "7",
                title = "Podpora regionů",
                text = "Zahájili jsme projekt podpory lokálních iniciativ.",
                date = "2026-01-20",
                label = ""
            },
            new NewsItem {
                id = "8",
                title = "Analýza trhu",
                text = "Publikována interní analýza aktuální situace.",
                date = "2026-01-15",
                label = ""
            },
            new NewsItem {
                id = "9",
                title = "Strategické partnerství",
                text = "Navázali jsme spolupráci s novým partnerem.",
                date = "2026-01-05",
                label = ""
            },
            new NewsItem {
                id = "10",
                title = "Shrnutí roku",
                text = "Ohlédnutí za uplynulým obdobím a plán do budoucna.",
                date = "2025-12-20",
                label = ""
            }
        };
    }
}