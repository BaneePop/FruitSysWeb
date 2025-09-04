namespace FruitSysWeb.Services.Models.Requests
{
    public class FilterRequest
    {
        public DateTime? OdDatum { get; set; }
        public DateTime? DoDatum { get; set; }
        public long? KomitentId { get; set; }
        public string? KomitentTip { get; set; }
        public long? ArtikalId { get; set; }
        public string? ArtikalTip { get; set; }
        public string? SearchTerm { get; set; }
        public string? RadniNalog { get; set; }
        
        public bool HasFilters => 
            OdDatum.HasValue || DoDatum.HasValue || 
            KomitentId.HasValue || !string.IsNullOrEmpty(KomitentTip) ||
            ArtikalId.HasValue || !string.IsNullOrEmpty(ArtikalTip) ||
            !string.IsNullOrEmpty(SearchTerm) || !string.IsNullOrEmpty(RadniNalog);
    }
}