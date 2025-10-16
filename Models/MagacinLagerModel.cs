using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class MagacinLagerModel
    {
        [Display(Name = "Artikal ID")]
        public long ArtikalID { get; set; }

        // ✅ NOVO: Tip kao int (MagacinID iz baze)
        [Display(Name = "Tip")]
        public int? Tip { get; set; }

        // ✅ NOVO: TipArtikla kao string (CASE statement rezultat)
        [Display(Name = "Tip artikla")]
        public string? TipArtikla { get; set; }

        // ZADRŽANO: Stari naziv za backward compatibility
        [Display(Name = "Tip artikla (staro)")]
        public string? ArtikalTip { get; set; }

        [Display(Name = "MagacinID")]
        public int MagacinID { get; set; }

        [Display(Name = "Artikal")]
        public string Artikal { get; set; } = string.Empty;

        [Display(Name = "Količina")]
        public decimal Kolicina { get; set; }

        [Display(Name = "Pakovanje")]
        public string? Pakovanje { get; set; }

        [Display(Name = "Jedinica mere")]
        public string? JM { get; set; }

        [Display(Name = "Lot")]
        public string? Lot { get; set; }

        [Display(Name = "Rok važenja")]
        public DateTime? RokVazenja { get; set; }

        [Display(Name = "Cena")]
        public decimal? Cena { get; set; }

        [Display(Name = "Vrednost")]
        public decimal Vrednost => Kolicina * (Cena ?? 0);

        // Za kolonu "Za najavljene Utovare"
        [Display(Name = "Za najavljene Utovare")]
        public string? ZaNajavljeneUtovare { get; set; }

        [Display(Name = "Status")]
        public string Status => GetStatusOpis();

        // ✅ NOVO: Helper property za badge klasu prema MagacinID
        public string TipArtiklaBadgeClass => GetTipBadgeClass();

        [Display(Name = "Tip naziv")]
        public string TipNaziv => GetTipNaziv(Tip ?? MagacinID);

        private string GetStatusOpis()
        {
            if (Kolicina <= 0) return "Nema na stanju";
            if (Kolicina < 10) return "Ispod minimuma";
            if (Kolicina < 20) return "Ograničeno";
            return "Dostupno";
        }

        private static string GetTipNaziv(int magacinId)
        {
            return magacinId switch
            {
                2 => "Sveza Roba",
                3 => "Sirovine",
                4 => "Ambalaza",
                5 => "Polu Proizvod",
                6 => "Gotov Proizvod",
                7 => "Kalo i Rastur",
                8 => "Usl.Mleko",
                9 => "Repromaterijal",
                10 => "Đubriva",
                11 => "Usl. Voće",
                12 => "Usl. Meso",
                _ => $"MagacinID {magacinId}"
            };
        }

        // ✅ NOVO: Badge klasa prema MagacinID
        private string GetTipBadgeClass()
        {
            int magacinId = Tip ?? MagacinID;

            return magacinId switch
            {
                2 => "bg-danger text-white",      // Sveza Roba - Crvena
                3 => "bg-secondary text-white",   // Sirovine - Siva
                4 => "bg-success text-white",     // Ambalaza - Zelena
                5 => "bg-secondary text-white",   // Polu Proizvod - Siva
                6 => "bg-success text-white",     // Gotov Proizvod - Zelena
                7 => "bg-dark text-white",        // Kalo i Rastur - Crna
                8 => "bg-info text-white",        // Usl.Mleko - Plava
                9 => "bg-warning text-dark",      // Repromaterijal - Žuta/Braon
                10 => "bg-success text-white",    // Đubriva - Zelena
                11 => "bg-info text-white",       // Usl. Voće - Plava
                12 => "bg-info text-white",       // Usl. Meso - Plava
                _ => "bg-secondary text-white"    // Default - Siva
            };
        }
    }
}
