using System.Globalization;

namespace FruitSysWeb.Models
{
    /// <summary>
    /// Cena koštanja po radnom nalogu razložena po elementima troška:
    /// Direktan rad + Režija + Direktan materijal (sirovina) + Ambalaža primarna + Ambalaža sekundarna
    /// </summary>
    public class CenaKostanjaModel
    {
        private static readonly CultureInfo SrFormat = new CultureInfo("sr-Latn-RS");

        // ─── Identifikacija ───
        public long RadniNalogID { get; set; }
        public string RadniNalog { get; set; } = string.Empty;
        public string Artikal { get; set; } = string.Empty;
        public string Pakovanje { get; set; } = string.Empty;
        public string Komitent { get; set; } = string.Empty;
        public string? LotNaloga { get; set; }
        public DateTime? DatumPocetka { get; set; }
        public DateTime? DatumZavrsetka { get; set; }

        // ─── Količina gotovog proizvoda ───
        /// <summary>Ukupna kg gotovog proizvoda (RpArtikalTip=2) iz vPreradaPregled_v2</summary>
        public decimal KolicinaGotovog { get; set; }

        /// <summary>Broj pakovanja (komada)</summary>
        public decimal BrojPakovanja { get; set; }

        // ─── Troškovi ───
        /// <summary>Direktan rad = SUM(CenaKostanjaDirektanRad) gde DirektanRadObracunat = 1</summary>
        public decimal TrosakDirektanRad { get; set; }

        /// <summary>Režija = SUM(CenaKostanjaDirektanRad) gde DirektanRadObracunat = 0</summary>
        public decimal TrosakRezija { get; set; }

        /// <summary>
        /// Direktan materijal = SUM(Kolicina × NajnovijaCena) iz vPreradaPregled_v2 (RpArtikalTip=1, MagacinID=3).
        /// Za sirovine bez cene koristi prosek cena sirovine za koje cena postoji.
        /// </summary>
        public decimal TrosakMaterijal { get; set; }

        /// <summary>
        /// Ambalaža primarna = kese, džakovi, gajbe (AmbalazaTip != 3, MagacinID=4).
        /// Vrednost = Kolicina × NajnovijaCena iz KalkulacijaArtikalCena.
        /// </summary>
        public decimal TrosakAmbalazaPrimarna { get; set; }

        /// <summary>
        /// Ambalaža sekundarna = kutije (AmbalazaTip = 3, MagacinID=4).
        /// Vrednost = Kolicina × NajnovijaCena iz KalkulacijaArtikalCena.
        /// </summary>
        public decimal TrosakAmbalazaSekundarna { get; set; }

        // ─── Agregatni ───
        public decimal TrosakAmbalazaUkupno => TrosakAmbalazaPrimarna + TrosakAmbalazaSekundarna;
        public decimal TrosakBezSirovine => TrosakDirektanRad + TrosakRezija + TrosakAmbalazaUkupno;
        public decimal UkupanTrosak => TrosakDirektanRad + TrosakRezija + TrosakMaterijal + TrosakAmbalazaUkupno;

        // ─── Cena po kg ───
        public decimal CenaDirektanRadPoKg => KolicinaGotovog > 0 ? TrosakDirektanRad / KolicinaGotovog : 0;
        public decimal CenaRezijaPoKg => KolicinaGotovog > 0 ? TrosakRezija / KolicinaGotovog : 0;
        public decimal CenaMaterijalPoKg => KolicinaGotovog > 0 ? TrosakMaterijal / KolicinaGotovog : 0;
        public decimal CenaAmbalazaPrimarnaPoKg => KolicinaGotovog > 0 ? TrosakAmbalazaPrimarna / KolicinaGotovog : 0;
        public decimal CenaAmbalazaSekundarnaPoKg => KolicinaGotovog > 0 ? TrosakAmbalazaSekundarna / KolicinaGotovog : 0;
        /// <summary>Cena po kg bez sirovine: rad + režija + ambalaza</summary>
        public decimal CenaBezSirovinePoKg => KolicinaGotovog > 0 ? TrosakBezSirovine / KolicinaGotovog : 0;
        /// <summary>Ukupna cena po kg: svi elementi uključujući sirovina</summary>
        public decimal UkupnaCenaPoKg => KolicinaGotovog > 0 ? UkupanTrosak / KolicinaGotovog : 0;

        // ─── Udeli (%) ───
        public decimal UdelDirektanRad => UkupanTrosak > 0 ? TrosakDirektanRad / UkupanTrosak * 100 : 0;
        public decimal UdelRezija => UkupanTrosak > 0 ? TrosakRezija / UkupanTrosak * 100 : 0;
        public decimal UdelMaterijal => UkupanTrosak > 0 ? TrosakMaterijal / UkupanTrosak * 100 : 0;
        public decimal UdelAmbalazaPrimarna => UkupanTrosak > 0 ? TrosakAmbalazaPrimarna / UkupanTrosak * 100 : 0;
        public decimal UdelAmbalazaSekundarna => UkupanTrosak > 0 ? TrosakAmbalazaSekundarna / UkupanTrosak * 100 : 0;

        // ─── Rad ───
        public decimal BrojRadnihSati { get; set; }
        public int BrojRadnika { get; set; }

        // ─── Formatiranje ───
        public string KolicinaFormatted => KolicinaGotovog.ToString("N2", SrFormat) + " kg";
        public string BrojPakovanjaFormatted => BrojPakovanja > 0 ? BrojPakovanja.ToString("N0", SrFormat) + " kom" : "-";
        public string DatumPocetkaFormatted => DatumPocetka?.ToString("dd.MM.yyyy", SrFormat) ?? "-";
        public string DatumZavrsetkaFormatted => DatumZavrsetka?.ToString("dd.MM.yyyy", SrFormat) ?? "-";

        public string TrosakDirektanRadFormatted => TrosakDirektanRad.ToString("N2", SrFormat);
        public string TrosakRezijaFormatted => TrosakRezija.ToString("N2", SrFormat);
        public string TrosakMaterijalFormatted => TrosakMaterijal.ToString("N2", SrFormat);
        public string TrosakAmbalazaPrimarnaFormatted => TrosakAmbalazaPrimarna.ToString("N2", SrFormat);
        public string TrosakAmbalazaSekundarnaFormatted => TrosakAmbalazaSekundarna.ToString("N2", SrFormat);
        public string UkupanTrosakFormatted => UkupanTrosak.ToString("N2", SrFormat);

        public string CenaBezSirovinePoKgFormatted => CenaBezSirovinePoKg.ToString("N2", SrFormat);
        public string UkupnaCenaPoKgFormatted => UkupnaCenaPoKg.ToString("N2", SrFormat);
        public string CenaDirektanRadPoKgFormatted => CenaDirektanRadPoKg.ToString("N2", SrFormat);
        public string CenaRezijaPoKgFormatted => CenaRezijaPoKg.ToString("N2", SrFormat);
        public string CenaMaterijalPoKgFormatted => CenaMaterijalPoKg.ToString("N2", SrFormat);
        public string CenaAmbalazaPrimarnaPoKgFormatted => CenaAmbalazaPrimarnaPoKg.ToString("N2", SrFormat);
        public string CenaAmbalazaSekundarnaPoKgFormatted => CenaAmbalazaSekundarnaPoKg.ToString("N2", SrFormat);

        // ─── Badge klasa za cenu po kg (prag za zamrznutu robu ~30-100 RSD/kg) ───
        public string CenaPoKgBadgeClass => UkupnaCenaPoKg switch
        {
            <= 50 => "bg-success",
            <= 100 => "bg-warning text-dark",
            _ => "bg-danger"
        };
    }
}
