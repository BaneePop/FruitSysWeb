using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class PrijemnicaModel
    {
        [Display(Name = "ID")]
        public long ID { get; set; }

        [Display(Name = "Šifra")]
        public string Sifra { get; set; } = string.Empty;

        [Display(Name = "Datum")]
        public DateTime Datum { get; set; }

        [Display(Name = "Vozač")]
        public string? Vozac { get; set; }

        [Display(Name = "Vozilo")]
        public string? Vozilo { get; set; }

        [Display(Name = "Napomena")]
        public string? Napomena { get; set; }

        [Display(Name = "Otpremnica")]
        public string? Otpremnica { get; set; }

        [Display(Name = "Tip Prijema")]
        public int? TipPrijema { get; set; }

        [Display(Name = "Dokument Status")]
        public int DokumentStatus { get; set; }

        [Display(Name = "Kreirano")]
        public DateTime Kreirano { get; set; }

        [Display(Name = "Ažurirano")]
        public DateTime Azurirano { get; set; }

        [Display(Name = "Version")]
        public int Version { get; set; }

        [Display(Name = "Ugovor ID")]
        public long? UgovorID { get; set; }

        [Display(Name = "Komitent ID")]
        public long? KomitentID { get; set; }

        [Display(Name = "Komitent")]
        public string? Komitent { get; set; }

        [Display(Name = "Magacin ID")]
        public long? MagacinID { get; set; }

        [Display(Name = "Magacin")]
        public string? Magacin { get; set; }

        [Display(Name = "Lokacija ID")]
        public long? LokacijaID { get; set; }

        [Display(Name = "Lokacija")]
        public string? Lokacija { get; set; }

        [Display(Name = "Otkupno Mesto ID")]
        public long? OtkupnoMestoID { get; set; }

        [Display(Name = "Otkupno Mesto")]
        public string? OtkupnoMesto { get; set; }

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

        [Display(Name = "Primiti")]
        public bool? Primiti { get; set; }

        [Display(Name = "Primiti Uz Selekciju")]
        public bool? PrimitiUzSelekciju { get; set; }

        [Display(Name = "Reklamirati")]
        public bool? Reklamirati { get; set; }

        [Display(Name = "Uslovno Primiti Za")]
        public string? UslovnoPrimitiZa { get; set; }

        [Display(Name = "Tehnolog ID")]
        public long? TehnologID { get; set; }

        [Display(Name = "Tehnolog")]
        public string? Tehnolog { get; set; }

        [Display(Name = "Vratiti")]
        public bool? Vratiti { get; set; }

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

        [Display(Name = "Tip Prijema Tekst")]
        public string TipPrijemaTekst => TipPrijema switch
        {
            1 => "Standardni prijem",
            2 => "Hitni prijem",
            3 => "Kontrolni prijem",
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
                if (Reklamirati == true) return "Reklamirati";
                if (Vratiti == true) return "Vratiti";
                if (PrimitiUzSelekciju == true) return "Primiti uz selekciju";
                if (Primiti == true) return "Primiti";
                return "Na kontroli";
            }
        }

        [Display(Name = "Kontrola Boja")]
        public string KontrolaBoja
        {
            get
            {
                if (Reklamirati == true) return "danger";
                if (Vratiti == true) return "warning";
                if (PrimitiUzSelekciju == true) return "info";
                if (Primiti == true) return "success";
                return "secondary";
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
    }
}
