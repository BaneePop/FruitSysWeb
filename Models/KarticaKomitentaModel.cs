using System.Globalization;

namespace FruitSysWeb.Models
{
    /// <summary>
    /// Jedna transakcija u kartici komitenta sa tekućim saldom
    /// </summary>
    public class KarticaStavkaModel
    {
        private static readonly CultureInfo SrFormat = new CultureInfo("sr-Latn-RS");

        // Ključni podaci iz vPrometFinansijev9
        public long ID { get; set; }
        public long KomitentID { get; set; }
        public string Komitent { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public string Dokument { get; set; } = string.Empty;
        public string? DokumentTip { get; set; }
        public int DokumentStatus { get; set; }
        public string? Artikal { get; set; }
        public decimal Kolicina { get; set; }
        public decimal Potrazuje { get; set; }   // Šta komitent duguje nama (FK-, IS-)
        public decimal Duguje { get; set; }      // Šta mi dugujemo komitentu (KL-, UP-)

        // Tekući saldo — računa se u servisu kumulativno
        public decimal TekuciSaldo { get; set; }

        // ─── Computed ───
        public string DatumFormatted => Datum.ToString("dd.MM.yyyy", SrFormat);
        public string KolicinaFormatted => Kolicina != 0 ? Kolicina.ToString("N2", SrFormat) + " kg" : "-";
        public string PotrazujeFormatted => Potrazuje != 0 ? Potrazuje.ToString("N2", SrFormat) : "-";
        public string DugujeFormatted => Duguje != 0 ? Duguje.ToString("N2", SrFormat) : "-";
        public string TekuciSaldoFormatted => TekuciSaldo.ToString("N2", SrFormat);

        public string TipDokumenta
        {
            get
            {
                if (Dokument.StartsWith("KL-")) return "Nabavka";
                if (Dokument.StartsWith("FK-")) return "Faktura";
                if (Dokument.StartsWith("IS-")) return "Isplata";
                if (Dokument.StartsWith("UP-")) return "Uplata";
                return DokumentTip ?? "Ostalo";
            }
        }

        public string TipIkonica
        {
            get
            {
                if (Dokument.StartsWith("KL-")) return "bi-box-arrow-in-down text-primary";
                if (Dokument.StartsWith("FK-")) return "bi-receipt text-success";
                if (Dokument.StartsWith("IS-")) return "bi-cash-stack text-danger";
                if (Dokument.StartsWith("UP-")) return "bi-arrow-up-circle text-info";
                return "bi-file-text text-secondary";
            }
        }

        public string SaldoBadgeClass => TekuciSaldo > 0 ? "bg-success" : TekuciSaldo < 0 ? "bg-danger" : "bg-secondary";
        public string SaldoTextClass => TekuciSaldo > 0 ? "text-success fw-bold" : TekuciSaldo < 0 ? "text-danger fw-bold" : "text-secondary";
    }

    /// <summary>
    /// Sumarne informacije o komitentovom prometu za header kartice
    /// </summary>
    public class KarticaKomitentaSumaModel
    {
        private static readonly CultureInfo SrFormat = new CultureInfo("sr-Latn-RS");

        public long KomitentID { get; set; }
        public string Komitent { get; set; } = string.Empty;
        public decimal UkupnoPotrazuje { get; set; }
        public decimal UkupnoDuguje { get; set; }
        public decimal ZavrsniSaldo { get; set; }
        public int BrojTransakcija { get; set; }

        public string UkupnoPotrazujeFormatted => UkupnoPotrazuje.ToString("N2", SrFormat);
        public string UkupnoDugujeFormatted => UkupnoDuguje.ToString("N2", SrFormat);
        public string ZavrsniSaldoFormatted => ZavrsniSaldo.ToString("N2", SrFormat);
        public string SaldoTextClass => ZavrsniSaldo > 0 ? "text-success" : ZavrsniSaldo < 0 ? "text-danger" : "text-secondary";
        public string SaldoLabel => ZavrsniSaldo > 0 ? "Duguju nam" : ZavrsniSaldo < 0 ? "Dugujemo im" : "Izmireno";
    }
}
