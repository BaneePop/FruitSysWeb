namespace FruitSysWeb.Models
{
    // ─── 1. Dobijena gotova roba ───────────────────────────────────────────────
    public class GotovaRobaRedModel
    {
        public DateTime Datum { get; set; }
        public string Artikal { get; set; } = string.Empty;
        public decimal Kolicina { get; set; }
    }

    // ─── 2. Utrošene sirovine ─────────────────────────────────────────────────
    public class UtrosenaSirovinaRedModel
    {
        public DateTime Datum { get; set; }
        public string Artikal { get; set; } = string.Empty;
        public decimal Kolicina { get; set; }
    }

    // ─── 3. Utrošena ambalaza ─────────────────────────────────────────────────
    public class UtrosenaAmbalazeRedModel
    {
        public DateTime Datum { get; set; }
        public string Artikal { get; set; } = string.Empty;
        public string VrstaAmbalazeNaziv { get; set; } = string.Empty;
        public decimal Kolicina { get; set; }
    }

    // ─── 4. Trošak radne snage — direktan ─────────────────────────────────────
    public class TrosakRadneSnageDirektanRedModel
    {
        public DateTime Datum { get; set; }
        public decimal BrojRadnihSati { get; set; }
        public decimal Iznos { get; set; }
        public decimal TrosakPoKg { get; set; }
    }

    // ─── 5. Trošak radne snage — indirektan ───────────────────────────────────
    public class TrosakRadneSnageIndirektanRedModel
    {
        public DateTime Datum { get; set; }
        public decimal BrojRadnihSati { get; set; }
        public decimal Iznos { get; set; }
        public decimal TrosakPoKg { get; set; }
    }

    // ─── Kompletan izveštaj ───────────────────────────────────────────────────
    public class IzvestajProizvodnjeModel
    {
        public List<GotovaRobaRedModel> GotovaRoba { get; set; } = new();
        public List<UtrosenaSirovinaRedModel> UtroseneSirovine { get; set; } = new();
        public List<UtrosenaAmbalazeRedModel> UtrosenaAmbalaza { get; set; } = new();
        public List<TrosakRadneSnageDirektanRedModel> TrosakDirektan { get; set; } = new();
        public List<TrosakRadneSnageIndirektanRedModel> TrosakIndirektan { get; set; } = new();

        // ── Sumarni podaci ─────────────────────────────────────────────────────

        /// <summary>Suma kolicine gotove robe grupisana po nazivu artikla</summary>
        public Dictionary<string, decimal> SumaGotovaRobaPoArtiklu =>
            GotovaRoba.GroupBy(r => r.Artikal)
                      .ToDictionary(g => g.Key, g => g.Sum(r => r.Kolicina));

        /// <summary>Suma sirovine grupisana po nazivu artikla</summary>
        public Dictionary<string, decimal> SumaSirovinePоArtiklu =>
            UtroseneSirovine.GroupBy(r => r.Artikal)
                            .ToDictionary(g => g.Key, g => g.Sum(r => r.Kolicina));

        /// <summary>Suma ambalaze grupisana po vrsti ambalaže</summary>
        public Dictionary<string, decimal> SumaAmbalazePoVrsti =>
            UtrosenaAmbalaza.GroupBy(r => r.VrstaAmbalazeNaziv)
                            .ToDictionary(g => g.Key, g => g.Sum(r => r.Kolicina));

        // ── Direktan rad ───────────────────────────────────────────────────────
        public decimal UkupnoSatiDirektan => TrosakDirektan.Sum(r => r.BrojRadnihSati);
        public decimal UkupnoIznosDirektan => TrosakDirektan.Sum(r => r.Iznos);
        public decimal UkupnaKolicinaGotoveRobe => GotovaRoba.Sum(r => r.Kolicina);
        public decimal TrosakPoKgDirektan =>
            UkupnaKolicinaGotoveRobe > 0 ? UkupnoIznosDirektan / UkupnaKolicinaGotoveRobe : 0;

        // ── Indirektan rad ─────────────────────────────────────────────────────
        public decimal UkupnoSatiIndirektan => TrosakIndirektan.Sum(r => r.BrojRadnihSati);
        public decimal UkupnoIznosIndirektan => TrosakIndirektan.Sum(r => r.Iznos);
        public decimal TrosakPoKgIndirektan =>
            UkupnaKolicinaGotoveRobe > 0 ? UkupnoIznosIndirektan / UkupnaKolicinaGotoveRobe : 0;

        // ── Ukupno (deo 6) ─────────────────────────────────────────────────────
        public decimal UkupnoSati => UkupnoSatiDirektan + UkupnoSatiIndirektan;
        public decimal UkupnoIznos => UkupnoIznosDirektan + UkupnoIznosIndirektan;
        public decimal TrosakPoKgUkupno =>
            UkupnaKolicinaGotoveRobe > 0 ? UkupnoIznos / UkupnaKolicinaGotoveRobe : 0;
    }
}
