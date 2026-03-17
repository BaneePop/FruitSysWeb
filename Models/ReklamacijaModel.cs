using System.Globalization;

namespace FruitSysWeb.Models
{
    /// <summary>
    /// Tipovi reklamacija - hijerarhijska struktura
    /// </summary>
    public static class ReklamacijaTipovi
    {
        public static readonly Dictionary<string, List<string>> Kategorije = new()
        {
            ["Kvalitet"] = new List<string>
            {
                "Procenat", "Boja", "Oštećeni proizvod", "Buđ ili plesan", "Deformacija", "Ostalo"
            },
            ["Strane Primese"] = new List<string>
            {
                "Biljne", "Plastika", "Metal", "Ostale"
            },
            ["Hemija"] = new List<string>
            {
                "Pesticidi", "Bakterije", "Razno"
            },
            ["Pakovanje"] = new List<string>
            {
                "Kutija i kesa", "Paletiziranje", "Temperatura", "Težina"
            }
        };

        public static readonly List<string> Rezultati = new()
        {
            "U toku",
            "Prihvaćeno",
            "Delimično prihvaćeno",
            "Vraćeno vozilo",
            "Odbijeno"
        };

        /// <summary>Vraca sve podtipove za dati tip kategorije</summary>
        public static List<string> GetPodtipovi(string? kategorija)
        {
            if (string.IsNullOrEmpty(kategorija)) return new List<string>();
            return Kategorije.TryGetValue(kategorija, out var lista) ? lista : new List<string>();
        }
    }

    /// <summary>
    /// Dokument vezan za reklamaciju (slike, PDF-ovi, DOCX...)
    /// </summary>
    public class ReklamacijaDokumentModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Naziv { get; set; } = string.Empty;
        public string TipDokumenta { get; set; } = string.Empty; // "KupacDokument" | "NasOdgovor"
        public string PutanjaFajla { get; set; } = string.Empty; // relativna putanja u Data/Reklamacije/{reklamacijaId}/
        public string Ekstenzija { get; set; } = string.Empty;
        public long VelicinaBajta { get; set; }
        public string DatumDodavanja { get; set; } = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

        private static readonly CultureInfo SrFormat = new CultureInfo("sr-Latn-RS");

        public string VelicinaFormatted => VelicinaBajta switch
        {
            < 1024 => $"{VelicinaBajta} B",
            < 1024 * 1024 => $"{VelicinaBajta / 1024.0:F1} KB",
            _ => $"{VelicinaBajta / (1024.0 * 1024):F1} MB"
        };

        public string IkonaKlasa => Ekstenzija.ToLower() switch
        {
            ".pdf" => "bi bi-file-earmark-pdf text-danger",
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "bi bi-file-earmark-image text-success",
            ".docx" or ".doc" => "bi bi-file-earmark-word text-primary",
            ".xlsx" or ".xls" => "bi bi-file-earmark-excel text-success",
            _ => "bi bi-file-earmark text-secondary"
        };

        public bool JeSlika => new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" }
            .Contains(Ekstenzija.ToLower());
    }

    /// <summary>
    /// Jedna reklamacija vezana za Radni Nalog
    /// </summary>
    public class ReklamacijaModel
    {
        private static readonly CultureInfo SrFormat = new CultureInfo("sr-Latn-RS");

        public string Id { get; set; } = Guid.NewGuid().ToString();

        // Podaci o radnom nalogu (iz MySQL baze)
        public long RadniNalogID { get; set; }
        public string RadniNalogSifra { get; set; } = string.Empty;
        public string KupacNaziv { get; set; } = string.Empty;
        public long KupacID { get; set; }
        public string VrstaVoca { get; set; } = string.Empty;
        public string Artikal { get; set; } = string.Empty;
        public string? LotNaloga { get; set; }
        public DateTime? DatumRadnogNaloga { get; set; }

        // Podaci o reklamaciji
        public DateTime DatumReklamacije { get; set; } = DateTime.Today;
        public string TipReklamacije { get; set; } = string.Empty;   // Kategorija (npr. "Kvalitet")
        public string PodtipReklamacije { get; set; } = string.Empty; // Podkategorija (npr. "Boja")
        public string OpisReklamacije { get; set; } = string.Empty;
        public string OpisPreventivnihMera { get; set; } = string.Empty;
        public string Rezultat { get; set; } = "U toku";
        public string? NapomenaRezultata { get; set; }

        // Dokumenti
        public List<ReklamacijaDokumentModel> DokumentiKupca { get; set; } = new();
        public List<ReklamacijaDokumentModel> NasiOdgovori { get; set; } = new();

        // Audit
        public string DatumKreiranja { get; set; } = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        public string DatumIzmene { get; set; } = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        public string KreiraoKorisnik { get; set; } = string.Empty;

        // Computed
        public string DatumReklamacijeFormatted =>
            DatumReklamacije.ToString("dd.MM.yyyy", SrFormat);

        public string DatumRadnogNalogaFormatted =>
            DatumRadnogNaloga?.ToString("dd.MM.yyyy", SrFormat) ?? "-";

        public string RezultatBadgeKlasa => Rezultat switch
        {
            "Prihvaćeno" => "bg-danger",
            "Delimično prihvaćeno" => "bg-warning text-dark",
            "Vraćeno vozilo" => "bg-danger",
            "Odbijeno" => "bg-secondary",
            _ => "bg-info text-dark" // U toku
        };

        public int UkupnoDokumenata => DokumentiKupca.Count + NasiOdgovori.Count;
    }

    /// <summary>
    /// Filter za pretragu reklamacija
    /// </summary>
    public class ReklamacijaFilterModel
    {
        public string? KupacNaziv { get; set; }
        public string? VrstaVoca { get; set; }
        public string? TipReklamacije { get; set; }
        public string? PodtipReklamacije { get; set; }
        public string? Rezultat { get; set; }
        public DateTime? OdDatum { get; set; }
        public DateTime? DoDatum { get; set; }
        public string? TraziTekst { get; set; }
    }
}
