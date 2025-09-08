using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Services.Models.Requests
{
    public class FilterRequest
    {
        [Display(Name = "Od datuma")]
        public DateTime? OdDatum { get; set; }
        
        [Display(Name = "Do datuma")]
        public DateTime? DoDatum { get; set; }
        
        [Display(Name = "Komitent ID")]
        public long? KomitentId { get; set; }
        
        [Display(Name = "Komitent")]
        public string? Komitent { get; set; }
        
        [Display(Name = "Tip komitenta")]
        public string? KomitentTip { get; set; }
        
        [Display(Name = "Artikal ID")]
        public long? ArtikalId { get; set; }
        
        [Display(Name = "Artikal")]
        public string? Artikal { get; set; }
        
        [Display(Name = "Tip artikla")]
        public int? TipArtikla { get; set; }
        
        [Display(Name = "Tip")] // String verzija za padajuće menije
        public string? Tip { get; set; }
        
        [Display(Name = "Radni nalog")]
        public string? RadniNalog { get; set; }
        
        [Display(Name = "Dokument tip")]
        public string? DokumentTip { get; set; }
        
        [Display(Name = "Min količina")]
        public decimal? MinKolicina { get; set; }
        
        [Display(Name = "Max količina")]
        public decimal? MaxKolicina { get; set; }
        
        [Display(Name = "Min saldo")]
        public decimal? MinSaldo { get; set; }
        
        [Display(Name = "Max saldo")]
        public decimal? MaxSaldo { get; set; }
        
        // DODATO: Specifični filteri za Lager
        [Display(Name = "Minimalna količina lager")]
        public decimal? MinimalnaKolicinaLager { get; set; }
        
        [Display(Name = "Pakovanje")]
        public string? Pakovanje { get; set; }
        
        [Display(Name = "Lot")]
        public string? Lot { get; set; }
        
        [Display(Name = "Rok važenja od")]
        public DateTime? RokVazenjaOd { get; set; }
        
        [Display(Name = "Rok važenja do")]
        public DateTime? RokVazenjaDo { get; set; }
        
        // DODATO: Boolean filteri za brzu selekciju
        [Display(Name = "Samo gotove robe")]
        public bool? SamoGotoveRobe { get; set; }
        
        [Display(Name = "Samo sirovine")]
        public bool? SamoSirovine { get; set; }
        
        [Display(Name = "Samo ambalaze")]
        public bool? SamoAmbalaže { get; set; }
        
        // DODATO: Kolekcije za multiple selekciju
        [Display(Name = "Artikal IDs")]
        public List<long>? ArtikalIds { get; set; }
        
        [Display(Name = "Komitent IDs")]
        public List<long>? KomitentIds { get; set; }
        
        // DODATO: Status filteri
        [Display(Name = "Status")]
        public string? Status { get; set; }
        
        [Display(Name = "Aktivno")]
        public bool? Aktivno { get; set; }
        
        [Display(Name = "Dokument Status")]
        public int? DokumentStatus { get; set; }
        
        // NOVO: Filter za ArtikalKlasifikacija (vrsta artikla)
        [Display(Name = "Artikal Klasifikacija ID")]
        public long? ArtikalKlasifikacijaId { get; set; }
        
        [Display(Name = "Vrsta Artikla")]
        public string? VrstaArtikla { get; set; }

        // METODA: Provera da li ima aktivne filtere
        public bool ImaAktivneFiltre()
        {
            return OdDatum.HasValue ||
                   DoDatum.HasValue ||
                   KomitentId.HasValue ||
                   !string.IsNullOrEmpty(Komitent) ||
                   !string.IsNullOrEmpty(KomitentTip) ||
                   ArtikalId.HasValue ||
                   !string.IsNullOrEmpty(Artikal) ||
                   TipArtikla.HasValue ||
                   !string.IsNullOrEmpty(Tip) ||
                   !string.IsNullOrEmpty(RadniNalog) ||
                   !string.IsNullOrEmpty(DokumentTip) ||
                   MinKolicina.HasValue ||
                   MaxKolicina.HasValue ||
                   MinSaldo.HasValue ||
                   MaxSaldo.HasValue ||
                   MinimalnaKolicinaLager.HasValue ||
                   !string.IsNullOrEmpty(Pakovanje) ||
                   !string.IsNullOrEmpty(Lot) ||
                   RokVazenjaOd.HasValue ||
                   RokVazenjaDo.HasValue ||
                   SamoGotoveRobe == true ||
                   SamoSirovine == true ||
                   SamoAmbalaže == true ||
                   ArtikalIds?.Any() == true ||
                   KomitentIds?.Any() == true ||
                   !string.IsNullOrEmpty(Status) ||
                   Aktivno.HasValue ||
                   DokumentStatus.HasValue ||
                   ArtikalKlasifikacijaId.HasValue ||
                   !string.IsNullOrEmpty(VrstaArtikla);
        }

        // METODA: Opis aktivnih filtera
        public string GetAktivneFiltereOpis()
        {
            var filteri = new List<string>();

            if (OdDatum.HasValue && DoDatum.HasValue)
                filteri.Add($"Period: {OdDatum:dd.MM.yyyy} - {DoDatum:dd.MM.yyyy}");
            else if (OdDatum.HasValue)
                filteri.Add($"Od: {OdDatum:dd.MM.yyyy}");
            else if (DoDatum.HasValue)
                filteri.Add($"Do: {DoDatum:dd.MM.yyyy}");

            if (!string.IsNullOrEmpty(Komitent))
                filteri.Add($"Komitent: {Komitent}");

            if (!string.IsNullOrEmpty(KomitentTip))
                filteri.Add($"Tip komitenta: {GetKomitentTipNaziv(KomitentTip)}");

            if (!string.IsNullOrEmpty(Artikal))
                filteri.Add($"Artikal: {Artikal}");

            if (!string.IsNullOrEmpty(Tip))
                filteri.Add($"Tip artikla: {GetArtikalTipNaziv(Tip)}");

            if (!string.IsNullOrEmpty(RadniNalog))
                filteri.Add($"Radni nalog: {RadniNalog}");

            if (MinKolicina.HasValue)
                filteri.Add($"Min količina: {MinKolicina:N2}");

            if (MaxKolicina.HasValue)
                filteri.Add($"Max količina: {MaxKolicina:N2}");

            if (!string.IsNullOrEmpty(Pakovanje))
                filteri.Add($"Pakovanje: {Pakovanje}");

            if (!string.IsNullOrEmpty(Lot))
                filteri.Add($"Lot: {Lot}");

            if (SamoGotoveRobe == true)
                filteri.Add("Samo gotove robe");

            if (SamoSirovine == true)
                filteri.Add("Samo sirovine");

            if (SamoAmbalaže == true)
                filteri.Add("Samo ambalaze");

            if (RokVazenjaOd.HasValue && RokVazenjaDo.HasValue)
                filteri.Add($"Rok važenja: {RokVazenjaOd:dd.MM.yyyy} - {RokVazenjaDo:dd.MM.yyyy}");

            if (MinimalnaKolicinaLager.HasValue)
                filteri.Add($"Min količina lager: {MinimalnaKolicinaLager:N2}");

            if (!string.IsNullOrEmpty(VrstaArtikla))
                filteri.Add($"Vrsta artikla: {VrstaArtikla}");

            return filteri.Any() ? string.Join(", ", filteri) : "Nema aktivnih filtera";
        }

        // HELPER METODE za nazive
        private static string GetKomitentTipNaziv(string? tip)
        {
            return tip?.ToLower() switch
            {
                "kupac" => "Kupac",
                "dobavljac" => "Dobavljač",
                "proizvodjac" => "Proizvođač",
                "otkupljivac" => "Otkupljivač",
                _ => tip ?? "Nepoznato"
            };
        }

        private static string GetArtikalTipNaziv(string? tip)
        {
            if (int.TryParse(tip, out int tipInt))
            {
                return tipInt switch
                {
                    1 => "Sirovina",
                    2 => "Ambalaza",
                    3 => "Potrosni materijal",
                    4 => "Gotova roba",
                    5 => "Oprema",
                    _ => $"Tip {tipInt}"
                };
            }
            return tip ?? "Nepoznato";
        }

        /* // METODA: Reset filtera
        public void ResetFilters()
        {
            OdDatum = null;
            DoDatum = null;
            KomitentId = null;
            Komitent = null;
            KomitentTip = null;
            ArtikalId = null;
            Artikal = null;
            TipArtikla = null;
            Tip = null;
            RadniNalog = null;
            DokumentTip = null;
            MinKolicina = null;
            MaxKolicina = null;
            MinSaldo = null;
            MaxSaldo = null;
            MinimalnaKolicinaLager = null;
            Pakovanje = null;
            Lot = null;
            RokVazenjaOd = null;
            RokVazenjaDo = null;
            SamoGotoveRobe = null;
            SamoSirovine = null;
            SamoAmbalaže = null;
            ArtikalIds = null;
            KomitentIds = null;
            Status = null;
            Aktivno = null;
            DokumentStatus = null;
            ArtikalKlasifikacijaId = null;
            VrstaArtikla = null;
        }
 */
        // METODA: Kopiraj filtere
        public FilterRequest Clone()
        {
            return new FilterRequest
            {
                OdDatum = this.OdDatum,
                DoDatum = this.DoDatum,
                KomitentId = this.KomitentId,
                Komitent = this.Komitent,
                KomitentTip = this.KomitentTip,
                ArtikalId = this.ArtikalId,
                Artikal = this.Artikal,
                TipArtikla = this.TipArtikla,
                Tip = this.Tip,
                RadniNalog = this.RadniNalog,
                DokumentTip = this.DokumentTip,
                MinKolicina = this.MinKolicina,
                MaxKolicina = this.MaxKolicina,
                MinSaldo = this.MinSaldo,
                MaxSaldo = this.MaxSaldo,
                MinimalnaKolicinaLager = this.MinimalnaKolicinaLager,
                Pakovanje = this.Pakovanje,
                Lot = this.Lot,
                RokVazenjaOd = this.RokVazenjaOd,
                RokVazenjaDo = this.RokVazenjaDo,
                SamoGotoveRobe = this.SamoGotoveRobe,
                SamoSirovine = this.SamoSirovine,
                SamoAmbalaže = this.SamoAmbalaže,
                ArtikalIds = this.ArtikalIds?.ToList(),
                KomitentIds = this.KomitentIds?.ToList(),
                Status = this.Status,
                Aktivno = this.Aktivno,
                DokumentStatus = this.DokumentStatus,
                ArtikalKlasifikacijaId = this.ArtikalKlasifikacijaId,
                VrstaArtikla = this.VrstaArtikla
            };
        }

        // METODA: Validacija filtera
        public List<string> Validate()
        {
            var errors = new List<string>();

            if (OdDatum.HasValue && DoDatum.HasValue && OdDatum > DoDatum)
            {
                errors.Add("Datum 'Od' ne može biti veći od datuma 'Do'");
            }

            if (MinKolicina.HasValue && MaxKolicina.HasValue && MinKolicina > MaxKolicina)
            {
                errors.Add("Minimalna količina ne može biti veća od maksimalne");
            }

            if (MinSaldo.HasValue && MaxSaldo.HasValue && MinSaldo > MaxSaldo)
            {
                errors.Add("Minimalni saldo ne može biti veći od maksimalnog");
            }

            if (RokVazenjaOd.HasValue && RokVazenjaDo.HasValue && RokVazenjaOd > RokVazenjaDo)
            {
                errors.Add("Rok važenja 'Od' ne može biti veći od 'Do'");
            }

            if (MinimalnaKolicinaLager.HasValue && MinimalnaKolicinaLager < 0)
            {
                errors.Add("Minimalna količina lager ne može biti negativna");
            }

            return errors;
        }

        // METODA: Da li je filter prazan
        public bool IsEmpty()
        {
            return !ImaAktivneFiltre();
        }

        // METODA: Postavke default vrednosti za specific case
        public static FilterRequest CreateDefault()
        {
            var pocetakMeseca = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            return new FilterRequest
            {
                OdDatum = pocetakMeseca,
                DoDatum = DateTime.Now,
                MinimalnaKolicinaLager = 10
            };
        }

        // METODA: Za rad sa datumima
        public void SetDateRange(DateTime odDatum, DateTime doDatum)
        {
            OdDatum = odDatum;
            DoDatum = doDatum;
        }

        public void SetCurrentMonth()
        {
            var pocetakMeseca = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            SetDateRange(pocetakMeseca, DateTime.Now);
        }

        public void SetLastMonth()
        {
            var prosliMesec = DateTime.Now.AddMonths(-1);
            var pocetakProšlogMeseca = new DateTime(prosliMesec.Year, prosliMesec.Month, 1);
            var krajProšlogMeseca = pocetakProšlogMeseca.AddMonths(1).AddDays(-1);
            SetDateRange(pocetakProšlogMeseca, krajProšlogMeseca);
        }

        public void SetLast30Days()
        {
            SetDateRange(DateTime.Now.AddDays(-30), DateTime.Now);
        }
    }
}