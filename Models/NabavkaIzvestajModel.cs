using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class NabavkaIzvestajModel
    {
        [Display(Name = "ID")]
        public long ID { get; set; }

        [Display(Name = "Broj Dokumenta")]
        public string BrojDokumenta { get; set; } = string.Empty;

        [Display(Name = "Datum")]
        public DateTime Datum { get; set; }

        [Display(Name = "Dobavljač ID")]
        public long? DobavljacID { get; set; }

        [Display(Name = "Dobavljač")]
        public string Dobavljac { get; set; } = string.Empty;

        [Display(Name = "Artikal ID")]
        public long? ArtikalID { get; set; }

        [Display(Name = "Artikal")]
        public string Artikal { get; set; } = string.Empty;

        [Display(Name = "Vrsta Artikla")]
        public string VrstaArtikla { get; set; } = string.Empty;

        [Display(Name = "Vrsta Gotovog Proizvoda")]
        public string VrstaGotovogProizvoda { get; set; } = string.Empty;

        [Display(Name = "Količina")]
        public decimal Kolicina { get; set; }

        [Display(Name = "JM")]
        public string JedinicaMere { get; set; } = string.Empty;

        [Display(Name = "Bruto Cena")]
        public decimal BrutoCena { get; set; }

        [Display(Name = "Bruto Iznos")]
        public decimal BrutoIznos { get; set; }

        [Display(Name = "Zaduženje Ukupno")]
        public decimal ZaduzenjeUkupno { get; set; }

        [Display(Name = "Dokument Status")]
        public int DokumentStatus { get; set; }

        [Display(Name = "Status")]
        public string Status => DokumentStatus switch
        {
            2 => "Otvoren",
            3 => "Zaključen",
            4 => "Storno", 
            _ => "Nepoznato"
        };

        [Display(Name = "Status Badge")]
        public string StatusBadge => DokumentStatus switch
        {
            2 => "bg-warning text-dark",
            3 => "bg-success text-white", 
            4 => "bg-danger text-white",
            _ => "bg-secondary text-white"
        };

        [Display(Name = "Broj Radnog Naloga")]
        public string? BrojRadnogNaloga { get; set; }

        [Display(Name = "Broj Ugovora")]
        public string? BrojUgovora { get; set; }

        [Display(Name = "Tip Dokumenta")]
        public string TipDokumenta { get; set; } = "Nabavka";

        // Computed properties
        [Display(Name = "Jedinična Cena")]
        public decimal JedinicnaCena => Kolicina != 0 ? BrutoIznos / Kolicina : 0;

        [Display(Name = "Datum Formatiran")]
        public string DatumFormatiran => Datum.ToString("dd.MM.yyyy");

        [Display(Name = "Bruto Iznos Formatiran")]
        public string BrutoIznosFormatiran => BrutoIznos.ToString("N2") + " RSD";

        [Display(Name = "Količina Formatirana")]
        public string KolicinaFormatirana => Kolicina.ToString("N2") + " " + JedinicaMere;

        [Display(Name = "Magacin")]
        public string? Magacin { get; set; }

        [Display(Name = "Otkupno Mesto")]
        public string? OtkupnoMesto { get; set; }

        [Display(Name = "PDV")]
        public decimal PDV { get; set; }

        [Display(Name = "Neto Iznos")]
        public decimal NetoIznos { get; set; }
    }
}
