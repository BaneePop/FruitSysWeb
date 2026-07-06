using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class OtkupniListModel
    {
        [Display(Name = "ID")]
        public long ID { get; set; }

        [Display(Name = "Version")]
        public int Version { get; set; }

        [Display(Name = "Šifra")]
        public string Sifra { get; set; } = string.Empty;

        [Display(Name = "Datum")]
        public DateTime Datum { get; set; }

        [Display(Name = "Datum Prometa")]
        public DateTime? DatumPrometa { get; set; }

        [Display(Name = "Obrisan")]
        public bool Obrisan { get; set; }

        [Display(Name = "Otkupljivač ID")]
        public long? OtkupljivacID { get; set; }

        [Display(Name = "Otkupljivač")]
        public string? Otkupljivac { get; set; }

        [Display(Name = "Komitent ID")]
        public long? KomitentID { get; set; }

        [Display(Name = "Komitent")]
        public string? Komitent { get; set; }

        [Display(Name = "Otkupno Mesto ID")]
        public long? OtkupnoMestoID { get; set; }

        [Display(Name = "Otkupno Mesto")]
        public string? OtkupnoMesto { get; set; }

        [Display(Name = "Iznos Osnovice")]
        public decimal IznosOsnovice { get; set; }

        [Display(Name = "Stopa PDV")]
        public decimal StopaPDV { get; set; }

        [Display(Name = "Iznos PDV")]
        public decimal IznosPDV { get; set; }

        [Display(Name = "Iznos Ukupno")]
        public decimal IznosUkupno { get; set; }

        [Display(Name = "Datum Isplate PDV Nadoknade")]
        public DateTime? DatumIsplatePDVNadoknade { get; set; }

        [Display(Name = "Datum Isplate Osnovice")]
        public DateTime? DatumIsplateOsnovice { get; set; }

        [Display(Name = "Rok Isplate")]
        public string? RokIsplate { get; set; }

        [Display(Name = "Način Isplate")]
        public int? NacinIsplate { get; set; }

        [Display(Name = "Kreirano")]
        public DateTime Kreirano { get; set; }

        [Display(Name = "Ažurirano")]
        public DateTime Azurirano { get; set; }

        [Display(Name = "Dokument Status")]
        public int DokumentStatus { get; set; }

        [Display(Name = "Ugovor Otkup ID")]
        public long? UgovorOtkupID { get; set; }

        [Display(Name = "Prijemnica ID")]
        public long? PrijemnicaID { get; set; }

        // Computed properties
        [Display(Name = "Status")]
        public string Status => DokumentStatus switch
        {
            2 => "Otvoren",
            3 => "Zaključen",
            4 => "Storno",
            _ => "Nepoznato"
        };

        [Display(Name = "Stopa Poreza %")]
        public decimal StopaPorezaProcenat => StopaPDV;

        [Display(Name = "Dani od Datuma")]
        public int DaniOdDatuma => (int)(DateTime.Now - Datum).TotalDays;

        [Display(Name = "Je Zaključen")]
        public bool JeZakljucen => DokumentStatus == 3;

        [Display(Name = "Je Storno")]
        public bool JeStorno => DokumentStatus == 4;

        [Display(Name = "Je Obrisan")]
        public bool JeObrisan => Obrisan;

        [Display(Name = "Status Boja")]
        public string StatusBoja => DokumentStatus switch
        {
            2 => "warning",
            3 => "success",
            4 => "danger",
            _ => "secondary"
        };

        [Display(Name = "Način Isplate Tekst")]
        public string NacinIsplateTekst => NacinIsplate switch
        {
            1 => "Gotovina",
            2 => "Ček",
            3 => "Kartica",
            4 => "Bankovni transfer",
            _ => "Nepoznato"
        };

        [Display(Name = "Je Isplaćen")]
        public bool JeIsplacen => DatumIsplateOsnovice.HasValue;

        [Display(Name = "Dani do Isplate")]
        public int? DaniDoIsplate => DatumIsplateOsnovice.HasValue
            ? (int)(DatumIsplateOsnovice.Value - DateTime.Now).TotalDays
            : null;
    }
}
