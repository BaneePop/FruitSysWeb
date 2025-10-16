using System.ComponentModel.DataAnnotations;

using System.Text;

namespace FruitSysWeb.Models
{
    /// <summary>
    /// Model za PaletniList tabelu
    /// </summary>
    public class PaletniListModel
    {
        public long ID { get; set; }

        [Display(Name = "Sifra")]
        public string Sifra { get; set; } = string.Empty;
        
        public decimal Tezina { get; set; }
        public bool Aktivan { get; set; }
        public DateTime DatumKreiranja { get; set; }
        public DateTime? DatumZatvaranja { get; set; }
        public decimal BrojAmbalaze { get; set; }
        public int DokumentStatus { get; set; }
        public decimal BrutoTezina { get; set; }
        public DateTime Kreirano { get; set; }
        public DateTime Azurirano { get; set; }
        public int Version { get; set; }

        // Foreign Keys
        public long ArtikalID { get; set; }
        public long? KomitentID { get; set; }
        public long? AmbalazaID { get; set; }
        public long? LokacijaID { get; set; }
        public long? EvidencijaRadaID { get; set; }
        public long? OtpremnicaStavkaID { get; set; }
        public long? PrijemnicaStavkaID { get; set; }
        public long? PrenosMagacinStavkaID { get; set; }
        public long? RadniNalogID { get; set; }
        public string? Opis { get; set; }
        public long? PakovanjeID { get; set; }
        public int PaletniListTip { get; set; } // 1=Prijem, 2=Proizvodnja, 3=Otprema
        public string? OtpremnicaDobavljaca { get; set; }
        public string? LotDobavljaca { get; set; }

        // Joined properties
        public string Artikal { get; set; } = string.Empty;
        public string Komitent { get; set; } = string.Empty;
    }

    /// <summary>
    /// Model za grupisane podatke za chart - prijem po dobaavljačima i vrstama voća
    /// </summary>
    public class PrijemPoVocuModel
    {
        public string Voce { get; set; } = string.Empty; // Naziv artikla (voća)
        public string Dobavljac { get; set; } = string.Empty; // Naziv komitenta
        public decimal UkupnaKolicina { get; set; } // Suma težina
        public DateTime Datum { get; set; }
    }

    /// <summary>
    /// Model za ukupne statistike prijema
    /// </summary>
    public class PrijemStatistikaModel
    {
        public string Voce { get; set; } = string.Empty;
        public decimal UkupnaKolicina { get; set; }
        public int BrojPrijema { get; set; }
        public List<PrijemPoDobavljacuModel> Dobavljaci { get; set; } = new();
        public DateTime Datum { get; set; }

    }

    public class PrijemPoDobavljacuModel
    {
        public string Dobavljac { get; set; } = string.Empty;
        public decimal Kolicina { get; set; }
    }
}