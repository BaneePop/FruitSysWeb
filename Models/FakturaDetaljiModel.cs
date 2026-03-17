using System.Globalization;

namespace FruitSysWeb.Models
{
    /// <summary>
    /// Podaci o kupcu za prikaz na fakturi
    /// </summary>
    public class KupacDetaljiModel
    {
        public long ID { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string Adresa { get; set; } = string.Empty;
        public string PostanskiBroj { get; set; } = string.Empty;
        public string Mesto { get; set; } = string.Empty;
        public string Drzava { get; set; } = string.Empty;
        public string? PoreskiBroj { get; set; }
        public string? MaticniBroj { get; set; }
        public string? BrojRacuna { get; set; }
        public string? Telefon { get; set; }
        public bool Ino { get; set; }

        public string PunaAdresa => $"{Adresa}, {PostanskiBroj} {Mesto}, {Drzava}";
    }

    /// <summary>
    /// Podaci o otpremnici vezanoj za fakturu
    /// </summary>
    public class OtpremnicaInfoModel
    {
        public long ID { get; set; }
        public string? Sifra { get; set; }
        public DateTime Datum { get; set; }
        public string? Vozilo { get; set; }
        public string? Vozac { get; set; }
        public string? RadniNalogSifra { get; set; }
        public string? LotNaloga { get; set; }
        public int? RadniNalogBrojPakovanja { get; set; }
        public decimal? BrutoTezina { get; set; }
    }

    /// <summary>
    /// Podaci o ugovoru vezanom za fakturu
    /// </summary>
    public class UgovorInfoModel
    {
        public long ID { get; set; }
        public string BrojUgovora { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public string? Paritet { get; set; }
        public string? Placanje { get; set; }
        public DateTime? RokIsporuke { get; set; }
    }

    /// <summary>
    /// Kompletan model sa svim podacima fakture za generisanje PDF/Excel dokumenta
    /// </summary>
    public class FakturaDetaljiModel
    {
        public FakturaModel Faktura { get; set; } = new();
        public KupacDetaljiModel? Kupac { get; set; }
        public OtpremnicaInfoModel? Otpremnica { get; set; }
        public UgovorInfoModel? Ugovor { get; set; }
        public List<FakturaStavkaModel> Stavke { get; set; } = new();

        private static readonly CultureInfo SrFormat = new CultureInfo("sr-Latn-RS");

        // Agregati stavki
        public decimal UkupnoNeto => Stavke.Sum(s => s.NetoIznos);
        public decimal UkupnoPorez => Stavke.Sum(s => s.PorezIznos);
        public decimal UkupnoBruto => Stavke.Sum(s => s.BrutoIznos);
        public decimal UkupnoNetoEur => Stavke.Sum(s => s.NetoIznosEur);
        public decimal UkupnoBrutoEur => Stavke.Sum(s => s.BrutoIznosEur);
        public decimal UkupnoKolicina => Stavke.Sum(s => s.Kolicina);
        public int UkupnoBrojPakovanja => Stavke.Sum(s => s.BrojPakovanja);

        // Formatovani agregati
        public string UkupnoNetoFormatted => UkupnoNeto.ToString("N2", SrFormat);
        public string UkupnoPorezFormatted => UkupnoPorez.ToString("N2", SrFormat);
        public string UkupnoBrutoFormatted => UkupnoBruto.ToString("N2", SrFormat);
        public string UkupnoNetoEurFormatted => UkupnoNetoEur.ToString("N2", SrFormat);
        public string UkupnoBrutoEurFormatted => UkupnoBrutoEur.ToString("N2", SrFormat);

        // Da li faktura ima EUR iznose
        public bool ImaEurIznose => Faktura.KursEur.HasValue && Faktura.KursEur > 0;

        // Svi LOT-ovi iz stavki (distinct, comma-separated)
        public string SveLotovi => string.Join(", ",
            Stavke.Select(s => s.Lot).Where(l => !string.IsNullOrWhiteSpace(l)).Distinct());

        // Da li je ino kupac
        public bool JeInoKupac => Kupac?.Ino ?? false;

        // PDV stopa (uzima iz prve stavke ili iz fakture)
        public decimal PdvStopa => Stavke.FirstOrDefault()?.PorezStopa ?? 0;
    }
}
