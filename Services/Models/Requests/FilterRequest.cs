namespace FruitSysWeb.Services.Models.Requests
{
    public class FilterRequest
    {
        // Osnovni datumski filteri
        public DateTime? OdDatum { get; set; }
        public DateTime? DoDatum { get; set; }
        
        // Radni nalog filteri
        public string? RadniNalog { get; set; }
        public long? RadniNalogId { get; set; }
        
        // Partner filteri
        public long? KomitentId { get; set; }
        public string? KomitentTip { get; set; }
        
        // Artikal filteri
        public long? ArtikalId { get; set; }
        public string? ArtikalTip { get; set; }
        public int? TipArtikla { get; set; }
        public string? Klasifikacija { get; set; }
        
        // Količina filteri
        public decimal? MinKolicina { get; set; }
        public decimal? MaxKolicina { get; set; }
        public List<long>? ArtikalIds { get; set; }
        public List<long>? KomitentIds { get; set; }
        
        // Finansijski filteri
        public decimal? MinSaldo { get; set; }
        public decimal? MaxSaldo { get; set; }
        
        // DODANO na osnovu stvarne strukture vwMagacinLager
        public string? Pakovanje { get; set; }
        public int? PakovanjeTip { get; set; }
        public int? AmbalazaTip { get; set; }
        public string? Lot { get; set; }
        public DateTime? RokVazenjaOd { get; set; }
        public DateTime? RokVazenjaDo { get; set; }
        
        // Dokument filteri
        public string? DokumentTip { get; set; }
        public int? DokumentStatus { get; set; }
        
        // Boolean filteri za brže filtriranje
        public bool? SamoGotoveRobe { get; set; }
        public bool? SamoSirovine { get; set; }
        public bool? SamoAmbalaže { get; set; }
        public bool? SamoOprema { get; set; }
        
        // Partner boolean filteri
        public bool? SamoKupci { get; set; }
        public bool? SamoDobavljaci { get; set; }
        public bool? SamoProizvodjaci { get; set; }
        public bool? SamoOtkupljivaci { get; set; }
        
        // Lager specifični filteri
        public bool? SamoSaLagerom { get; set; }
        public decimal? MinimalnaKolicinaLager { get; set; } = 10;
        public bool? SamoIsticiRokVazenja { get; set; }
        public int? DaniDoIstekaRoka { get; set; } = 30; // default 30 dana
        
        // Reset metoda
        public void Reset()
        {
            OdDatum = null;
            DoDatum = null;
            RadniNalog = null;
            RadniNalogId = null;
            KomitentId = null;
            KomitentTip = null;
            ArtikalId = null;
            ArtikalTip = null;
            TipArtikla = null;
            Klasifikacija = null;
            MinKolicina = null;
            MaxKolicina = null;
            ArtikalIds = null;
            KomitentIds = null;
            MinSaldo = null;
            MaxSaldo = null;
            Pakovanje = null;
            PakovanjeTip = null;
            AmbalazaTip = null;
            Lot = null;
            RokVazenjaOd = null;
            RokVazenjaDo = null;
            DokumentTip = null;
            DokumentStatus = null;
            SamoGotoveRobe = null;
            SamoSirovine = null;
            SamoAmbalaže = null;
            SamoOprema = null;
            SamoKupci = null;
            SamoDobavljaci = null;
            SamoProizvodjaci = null;
            SamoOtkupljivaci = null;
            SamoSaLagerom = null;
            MinimalnaKolicinaLager = 10;
            SamoIsticiRokVazenja = null;
            DaniDoIstekaRoka = 30;
        }

        // Helper metode
        public bool ImaAktivneFiltre()
        {
            return OdDatum.HasValue || DoDatum.HasValue || 
                   !string.IsNullOrEmpty(RadniNalog) ||
                   KomitentId.HasValue || ArtikalId.HasValue ||
                   !string.IsNullOrEmpty(KomitentTip) || !string.IsNullOrEmpty(ArtikalTip) ||
                   MinSaldo.HasValue || MaxSaldo.HasValue ||
                   !string.IsNullOrEmpty(Pakovanje) || !string.IsNullOrEmpty(Lot) ||
                   SamoGotoveRobe.HasValue || SamoKupci.HasValue ||
                   RokVazenjaOd.HasValue || RokVazenjaDo.HasValue ||
                   MinimalnaKolicinaLager != 10;
        }

        public string GetAktivneFiltereOpis()
        {
            var opisi = new List<string>();
            
            if (OdDatum.HasValue) opisi.Add($"Od: {OdDatum.Value:dd.MM.yyyy}");
            if (DoDatum.HasValue) opisi.Add($"Do: {DoDatum.Value:dd.MM.yyyy}");
            if (!string.IsNullOrEmpty(KomitentTip)) opisi.Add($"Partner: {KomitentTip}");
            if (!string.IsNullOrEmpty(ArtikalTip)) 
            {
                if (int.TryParse(ArtikalTip, out int tipId))
                {
                    var tipNaziv = FruitSysWeb.Models.ArtikalTipHelper.GetDisplayName(tipId);
                    opisi.Add($"Artikal: {tipNaziv}");
                }
            }
            if (!string.IsNullOrEmpty(Pakovanje)) opisi.Add($"Pakovanje: {Pakovanje}");
            if (!string.IsNullOrEmpty(Lot)) opisi.Add($"Lot: {Lot}");
            if (SamoGotoveRobe == true) opisi.Add("Samo gotove robe");
            if (SamoKupci == true) opisi.Add("Samo kupci");
            if (SamoDobavljaci == true) opisi.Add("Samo dobavljači");
            if (MinimalnaKolicinaLager != 10) opisi.Add($"Min količina: {MinimalnaKolicinaLager}");
            if (RokVazenjaOd.HasValue) opisi.Add($"Rok od: {RokVazenjaOd.Value:dd.MM.yyyy}");
            if (RokVazenjaDo.HasValue) opisi.Add($"Rok do: {RokVazenjaDo.Value:dd.MM.yyyy}");
            
            return opisi.Count > 0 ? string.Join(", ", opisi) : "Nema aktivnih filtera";
        }

        // DODANO: Helper metode za validaciju
        public bool IsValidDateRange()
        {
            if (OdDatum.HasValue && DoDatum.HasValue)
            {
                return OdDatum.Value <= DoDatum.Value;
            }
            return true;
        }

        public bool IsValidQuantityRange()
        {
            if (MinKolicina.HasValue && MaxKolicina.HasValue)
            {
                return MinKolicina.Value <= MaxKolicina.Value;
            }
            return true;
        }

        public bool IsValidSaldoRange()
        {
            if (MinSaldo.HasValue && MaxSaldo.HasValue)
            {
                return MinSaldo.Value <= MaxSaldo.Value;
            }
            return true;
        }

        // DODANO: Metoda za konverziju tip artikla string -> int
        public int? GetArtikalTipAsInt()
        {
            if (!string.IsNullOrEmpty(ArtikalTip) && int.TryParse(ArtikalTip, out int tip))
            {
                return tip;
            }
            return TipArtikla;
        }
    }
}