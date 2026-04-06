using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class OtpremnicaModel
    {
        [Display(Name = "ID")]
        public long ID { get; set; }

        [Display(Name = "Šifra")]
        public string Sifra { get; set; } = string.Empty;

        [Display(Name = "Datum")]
        public DateTime Datum { get; set; }

        [Display(Name = "Vozilo")]
        public string? Vozilo { get; set; }

        [Display(Name = "Vozač")]
        public string? Vozac { get; set; }

        [Display(Name = "Vozař")]
        public string? Vozar { get; set; }

        [Display(Name = "Vozař PIB")]
        public string? VozarPib { get; set; }

        [Display(Name = "Vozař Adresa")]
        public string? VozarAdresa { get; set; }

        [Display(Name = "Broj Plombi")]
        public string? BrojPlombi { get; set; }

        [Display(Name = "Granični Prelaz ID")]
        public long? GranicniPrelazID { get; set; }

        [Display(Name = "Granični Prelaz")]
        public string? GranicniPrelaz { get; set; }

        [Display(Name = "Napomena")]
        public string? Napomena { get; set; }

        [Display(Name = "Vozač Broj Pasoša")]
        public string? VozacBrojPasosa { get; set; }

        [Display(Name = "Otprema Tip")]
        public int? OtpremaTip { get; set; }

        [Display(Name = "Kreirano")]
        public DateTime Kreirano { get; set; }

        [Display(Name = "Ažurirano")]
        public DateTime Azurirano { get; set; }

        [Display(Name = "Version")]
        public int Version { get; set; }

        [Display(Name = "Komitent ID")]
        public long? KomitentID { get; set; }

        [Display(Name = "Komitent")]
        public string? Komitent { get; set; }

        [Display(Name = "Magacin ID")]
        public long? MagacinID { get; set; }

        [Display(Name = "Magacin")]
        public string? Magacin { get; set; }

        [Display(Name = "Radni Nalog ID")]
        public long? RadniNalogID { get; set; }

        [Display(Name = "Radni Nalog")]
        public string? RadniNalog { get; set; }

        [Display(Name = "Ugovor ID")]
        public long? UgovorID { get; set; }

        [Display(Name = "Vozař ID")]
        public long? VozarID { get; set; }

        [Display(Name = "Uzorkovano")]
        public bool? Uzorkovano { get; set; }

        [Display(Name = "Količina")]
        public decimal? Kolicina { get; set; }

        [Display(Name = "Oznaka")]
        public string? Oznaka { get; set; }

        [Display(Name = "Vizuelna Kontrola")]
        public bool? VizuelnaKontrola { get; set; }

        [Display(Name = "Vizuelna Kontrola Primedba")]
        public string? VizuelnaKontrolaPrimedba { get; set; }

        [Display(Name = "Stanje Robe Pakovanja")]
        public bool? StanjeRobePakovanja { get; set; }

        [Display(Name = "Stanje Robe Pakovanja Primedba")]
        public string? StanjeRobePakovanjaPrimedba { get; set; }

        [Display(Name = "Temperatura")]
        public decimal? Temperatura { get; set; }

        [Display(Name = "Temperatura Primedba")]
        public string? TemperaturaPrimedba { get; set; }

        [Display(Name = "Stanje Vozila")]
        public bool? StanjeVozila { get; set; }

        [Display(Name = "Stanje Vozila Primedba")]
        public string? StanjeVozilaPrimedba { get; set; }

        [Display(Name = "Tehnolog ID")]
        public long? TehnologID { get; set; }

        [Display(Name = "Tehnolog")]
        public string? Tehnolog { get; set; }

        [Display(Name = "Kontrola Deklaracija")]
        public bool? KontrolaDeklaracija { get; set; }

        [Display(Name = "Kontrola Deklaracija Primedba")]
        public string? KontrolaDeklaracijaPrimedba { get; set; }

        [Display(Name = "Dokument Status")]
        public int DokumentStatus { get; set; }

        [Display(Name = "Aktivno")]
        public bool? Aktivno { get; set; }

        // Computed properties
        [Display(Name = "Status")]
        public string Status => DokumentStatus switch
        {
            2 => "Otvoren",
            3 => "Zaključen",
            4 => "Storno",
            _ => "Nepoznato"
        };

        [Display(Name = "Otprema Tip Tekst")]
        public string OtpremaTipTekst => OtpremaTip switch
        {
            1 => "Domaca otprema",
            2 => "Izvoz",
            3 => "Tranzit",
            _ => "Nepoznato"
        };

        [Display(Name = "Dani od Datuma")]
        public int DaniOdDatuma => (int)(DateTime.Now - Datum).TotalDays;

        [Display(Name = "Je Zaključen")]
        public bool JeZakljucen => DokumentStatus == 3;

        [Display(Name = "Je Storno")]
        public bool JeStorno => DokumentStatus == 4;

        [Display(Name = "Status Boja")]
        public string StatusBoja => DokumentStatus switch
        {
            2 => "warning",
            3 => "success",
            4 => "danger",
            _ => "secondary"
        };

        [Display(Name = "Kontrola Status")]
        public string KontrolaStatus
        {
            get
            {
                if (VizuelnaKontrola == false) return "Neispravno";
                if (StanjeRobePakovanja == false) return "Neispravno";
                if (StanjeVozila == false) return "Neispravno";
                if (KontrolaDeklaracija == false) return "Neispravno";
                return "Ispravno";
            }
        }

        [Display(Name = "Kontrola Boja")]
        public string KontrolaBoja
        {
            get
            {
                if (VizuelnaKontrola == false) return "danger";
                if (StanjeRobePakovanja == false) return "danger";
                if (StanjeVozila == false) return "danger";
                if (KontrolaDeklaracija == false) return "danger";
                return "success";
            }
        }

        [Display(Name = "Temperatura Status")]
        public string TemperaturaStatus
        {
            get
            {
                if (!Temperatura.HasValue) return "Nije merena";
                if (Temperatura <= 4) return "Odlična";
                if (Temperatura <= 8) return "Dobra";
                if (Temperatura <= 12) return "Prihvatljiva";
                return "Loša";
            }
        }

        [Display(Name = "Temperatura Boja")]
        public string TemperaturaBoja
        {
            get
            {
                if (!Temperatura.HasValue) return "secondary";
                if (Temperatura <= 4) return "success";
                if (Temperatura <= 8) return "info";
                if (Temperatura <= 12) return "warning";
                return "danger";
            }
        }

        [Display(Name = "Je Izvoz")]
        public bool JeIzvoz => OtpremaTip == 2;

        [Display(Name = "Je Tranzit")]
        public bool JeTranzit => OtpremaTip == 3;

        [Display(Name = "Ima Plombe")]
        public bool ImaPlombe => !string.IsNullOrEmpty(BrojPlombi);
    }
}
