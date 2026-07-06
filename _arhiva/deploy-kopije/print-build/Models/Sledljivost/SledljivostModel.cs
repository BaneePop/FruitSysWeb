namespace FruitSysWeb.Models.Sledljivost
{
    /// <summary>
    /// Glavni model za sledljivost - kontejner za sve povezane dokumente
    /// </summary>
    public class SledljivostModel
    {
        // Glavni dokument (Radni Nalog ili Paletni List)
        public string? TipDokumenta { get; set; } // "RadniNalog" ili "PaletniList"
        public string? Sifra { get; set; }
        public long? ID { get; set; }
        
        // Detalji Radnog Naloga (ako je RN)
        public RadniNalogDetalji? RadniNalog { get; set; }
        
        // Upstream (nabavka)
        public List<PrijemnicaDetalji> Prijemnice { get; set; } = new();
        public List<PaletniListDetalji> PaletniListoviUlaz { get; set; } = new();
        public List<EvidencijaRadaDetalji> EvidencijeRada { get; set; } = new();
        
        // Downstream (prodaja)
        public List<PaletniListDetalji> PaletniListoviIzlaz { get; set; } = new();
        public List<OtpremnicaDetalji> Otpremnice { get; set; } = new();
    }

    /// <summary>
    /// Detalji Radnog Naloga
    /// </summary>
    public class RadniNalogDetalji
    {
        public long ID { get; set; }
        public string? Sifra { get; set; }
        public long? KomitentID { get; set; }
        public string? KomitentNaziv { get; set; }
        public string? LotNaloga { get; set; }
        public int? BrojPakovanja { get; set; }
        public decimal? Kolicina { get; set; }
        public DateTime? Datum { get; set; }
        public int? DokumentStatus { get; set; }
        public string? StatusNaziv { get; set; }
    }

    /// <summary>
    /// Detalji Evidencije Rada
    /// </summary>
    public class EvidencijaRadaDetalji
    {
        public long ID { get; set; }
        public string? Sifra { get; set; }
        public long? RadniNalogID { get; set; }
        public long? SmenskiIzvestajID { get; set; }
        public string? SmenskiIzvestajSifra { get; set; }
        public DateTime? Datum { get; set; }
        public int? Smena { get; set; }
        public decimal? BrojRadnihSati { get; set; }
        
        // Povezani paletni listovi (utrošeni)
        public List<long> UtroseniPaletniListoviIDs { get; set; } = new();
    }

    /// <summary>
    /// Detalji Paletnog Lista
    /// </summary>
    public class PaletniListDetalji
    {
        public long ID { get; set; }
        public string? Sifra { get; set; }
        public decimal? Tezina { get; set; }
        public bool Aktivan { get; set; }
        public DateTime? DatumKreiranja { get; set; }
        public int? DokumentStatus { get; set; }
        public string? StatusNaziv { get; set; }
        
        // Artikal
        public long? ArtikalID { get; set; }
        public string? ArtikalNaziv { get; set; }
        public int? ArtikalMagacinID { get; set; } // Tip artikla (6 = Gotova roba)
        
        // Komitent
        public long? KomitentID { get; set; }
        public string? KomitentNaziv { get; set; }
        
        // Ambalaza
        public long? AmbalazaID { get; set; }
        public string? AmbalazaNaziv { get; set; }
        
        // Pakovanje
        public long? PakovanjeID { get; set; }
        public string? PakovanjeNaziv { get; set; }
        
        // Povezani dokumenti (ID-jevi)
        public long? EvidencijaRadaID { get; set; }
        public long? OtpremnicaStavkaID { get; set; }
        public long? PrijemnicaStavkaID { get; set; }
        public long? RadniNalogID { get; set; }
        
        // NOVO: Povezani dokumenti (Šifre)
        public string? EvidencijaRadaSifra { get; set; }
        public string? PrijemnicaSifra { get; set; }
        public string? OtpremnicaSifra { get; set; }
        public string? RadniNalogSifra { get; set; }
        public string? SmenskiIzvestajSifra { get; set; }
        
        // Dobavljač info
        public string? OtpremnicaDobavljaca { get; set; }
        public string? LotDobavljaca { get; set; }
        
        // NOVO: Praćenje povezanih paletnih listova
        public List<long> PovezaniPaletniListoviIDs { get; set; } = new List<long>();
        public List<string> PovezaniPaletniListoviSifre { get; set; } = new List<string>();
        public List<PaletniListDetalji> PovezaniPaletniListovi { get; set; } = new List<PaletniListDetalji>();

        // NOVO: Praćenje SVIH evidencija rada i radnih naloga gde je paletni list korišćen
        public List<string> KoriscenUEvidencijama { get; set; } = new List<string>();
        public List<string> KoriscenUSmenama { get; set; } = new List<string>();
        public List<string> KoriscenURadnimNalozima { get; set; } = new List<string>();
        
        // Tip paletnog lista
        public string TipPaletnogLista
        {
            get
            {
                if (PrijemnicaStavkaID.HasValue) return "Nabavka";
                if (EvidencijaRadaID.HasValue && !RadniNalogID.HasValue) return "Polu Proizvod";
                if (RadniNalogID.HasValue) return "Gotov Proizvod";
                if (OtpremnicaStavkaID.HasValue) return "Prodaja";
                return "Ostalo";
            }
        }
    }

    /// <summary>
    /// Detalji Prijemnice
    /// </summary>
    public class PrijemnicaDetalji
    {
        public long ID { get; set; }
        public string? Sifra { get; set; }
        public DateTime? Datum { get; set; }
        public string? Otpremnica { get; set; }
        public long? KomitentID { get; set; }
        public string? KomitentNaziv { get; set; }
        public long? MagacinID { get; set; }
        public string? Vozilo { get; set; }
        public int? DokumentStatus { get; set; }
        public string? StatusNaziv { get; set; }
        
        // Stavke
        public List<PrijemnicaStavkaDetalji> Stavke { get; set; } = new();
    }

    /// <summary>
    /// Detalji Prijemnica Stavka
    /// </summary>
    public class PrijemnicaStavkaDetalji
    {
        public long ID { get; set; }
        public long? PrijemnicaID { get; set; }
        public decimal? Kolicina { get; set; }
        public long? ArtikalID { get; set; }
        public string? ArtikalNaziv { get; set; }
        
        // Povezani paletni listovi
        public List<long> PaletniListoviIDs { get; set; } = new();
    }

    /// <summary>
    /// Detalji Otpremnice
    /// </summary>
    public class OtpremnicaDetalji
    {
        public long ID { get; set; }
        public string? Sifra { get; set; }
        public DateTime? Datum { get; set; }
        public string? Vozilo { get; set; }
        public long? KomitentID { get; set; }
        public string? KomitentNaziv { get; set; }
        public long? RadniNalogID { get; set; }
        public int? DokumentStatus { get; set; }
        public string? StatusNaziv { get; set; }
        
        // Stavke
        public List<OtpremnicaStavkaDetalji> Stavke { get; set; } = new();
    }

    /// <summary>
    /// Detalji Otpremnica Stavka
    /// </summary>
    public class OtpremnicaStavkaDetalji
    {
        public long ID { get; set; }
        public long? OtpremnicaID { get; set; }
        public decimal? Kolicina { get; set; }
        public long? ArtikalID { get; set; }
        public string? ArtikalNaziv { get; set; }
        
        // Povezani paletni listovi
        public List<long> PaletniListoviIDs { get; set; } = new();
    }

    /// <summary>
    /// Model za autocomplete search
    /// </summary>
    public class DokumentSearchResultModel
    {
        public string? Sifra { get; set; }
        public string? Tip { get; set; } // "RadniNalog" ili "PaletniList"
        public long ID { get; set; }
        public string? Opis { get; set; } // Npr. "Kupac: Maxi d.o.o., 500 kg"
    }

    /// <summary>
    /// Model za Prijem Sledljivost — kontejner za prijemnicu i sve njene paletne listove
    /// </summary>
    public class PrijemSledljivostModel
    {
        public long PrijemnicaID { get; set; }
        public string? PrijemnicaSifra { get; set; }
        public DateTime? Datum { get; set; }
        public string? KomitentNaziv { get; set; }
        public string? OtpremnicaDobavljaca { get; set; }
        public int? DokumentStatus { get; set; }
        public string? StatusNaziv { get; set; }

        public List<PrijemPaletniListRow> PaletniListovi { get; set; } = new();
    }

    /// <summary>
    /// Jedan paletni list iz prijemnice sa svim vezama u proizvodnji
    /// </summary>
    public class PrijemPaletniListRow
    {
        public long PaletniListID { get; set; }
        public string? Sifra { get; set; }
        public string? ArtikalNaziv { get; set; }
        public decimal? Tezina { get; set; }
        public string? LotDobavljaca { get; set; }

        // Veze u proizvodnji
        public List<RadniNalogInfo> RadniNalozi { get; set; } = new();
        public List<string> SmenskiIzvestaji { get; set; } = new();
        public List<string> EvidencijeRada { get; set; } = new();

        // Gotovi proizvodi nastali u radnim nalozima gde je ovaj PL korišćen
        public List<GotoviPLInfo> GotoviPaletniListovi { get; set; } = new();

        public bool NaLageru => !RadniNalozi.Any() && !EvidencijeRada.Any();
    }

    public class RadniNalogInfo
    {
        public string? Sifra { get; set; }
        public string? KomitentNaziv { get; set; }
    }

    public class GotoviPLInfo
    {
        public string? Sifra { get; set; }
        public string? ArtikalNaziv { get; set; }
        public decimal? Tezina { get; set; }
        public string? OtpremnicaSifra { get; set; }
        public string? KomitentNaziv { get; set; }
    }
}
