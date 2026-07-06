namespace FruitSysWeb.Models
{
    // ─────────────────────────────────────────────────────────────
    // Tabela 1 — Stanje Robe
    // ─────────────────────────────────────────────────────────────

    public class KalkulacijaStanjeRobeRed
    {
        public string VrstaVoca { get; set; } = string.Empty;

        // kg nabavljeno od početka sezone (KL-: sveža, sirovine, poluproizvodi, gotova roba; bez ALTIVA/FRIKOS)
        public decimal Kupljeno { get; set; }

        // stanje lagera na dan pre početka sezone (iz prometa do tog datuma)
        public decimal PrenosaZaliha { get; set; }

        // kg prodato u sezoni (FK- dokumenti, bez ALTIVA/FRIKOS)
        public decimal Prodato { get; set; }

        // kalo u procesu prerade: (Kupljeno_sirovine + PrenosaZaliha_sirovine) × %
        public decimal Kalo { get; set; }

        // direktno iz MagacinLager (sve magacine, bez ALTIVA/FRIKOS)
        public decimal NaZalihama { get; set; }

        public string NaZalihama_CssClass => NaZalihama < 0 ? "text-danger fw-bold" : string.Empty;
    }

    public class KalkulacijaStanjeRobeResult
    {
        public List<KalkulacijaStanjeRobeRed> Redovi { get; set; } = new();

        public decimal UkupnoKupljeno      => Redovi.Sum(r => r.Kupljeno);
        public decimal UkupnoPrenosaZaliha => Redovi.Sum(r => r.PrenosaZaliha);
        public decimal UkupnoProdato       => Redovi.Sum(r => r.Prodato);
        public decimal UkupnoKalo          => Redovi.Sum(r => r.Kalo);
        public decimal UkupnoNaZalihama    => Redovi.Sum(r => r.NaZalihama);
    }

    // ─────────────────────────────────────────────────────────────
    // Tabela Usluga — uslužna prerada (ALTIVA + FRIKOS)
    // ─────────────────────────────────────────────────────────────

    public class KalkulacijaUslugaRed
    {
        public string VrstaVoca { get; set; } = string.Empty;
        public decimal Kupljeno { get; set; }
        public decimal PrenosaZaliha { get; set; }
        public decimal Prodato { get; set; }
        public decimal NaZalihama { get; set; }
    }

    public class KalkulacijaUslugaResult
    {
        public List<KalkulacijaUslugaRed> Redovi { get; set; } = new();

        public decimal UkupnoKupljeno      => Redovi.Sum(r => r.Kupljeno);
        public decimal UkupnoPrenosaZaliha => Redovi.Sum(r => r.PrenosaZaliha);
        public decimal UkupnoProdato       => Redovi.Sum(r => r.Prodato);
        public decimal UkupnoNaZalihama    => Redovi.Sum(r => r.NaZalihama);
    }

    // ─────────────────────────────────────────────────────────────
    // Tabela 2 — Obračun Otkupa
    // ─────────────────────────────────────────────────────────────

    public class KalkulacijaObracunOtkupaRed
    {
        public int KomitentID { get; set; }
        public string Dobavljac { get; set; } = string.Empty;

        // SUM Potrazuje na KL- dokumentima
        public decimal VrednostSaMarzom { get; set; }

        // ručno uneto početno stanje pre sezone (negativno = mi dugujemo, pozitivno = oni duguju)
        public decimal PocetnoStanje { get; set; }

        // SUM Duguje na IS- dokumentima
        public decimal Isplata { get; set; }

        // (Isplata − VrednostSaMarzom) + PocetnoStanje
        public decimal StanjeFinal => Isplata - VrednostSaMarzom + PocetnoStanje;

        // ostavljeno za kompatibilnost
        public decimal UkupnaMarza { get; set; }
        public decimal StanjeUkupno => Isplata - VrednostSaMarzom;
    }

    public class KalkulacijaObracunOtkupaResult
    {
        public List<KalkulacijaObracunOtkupaRed> Redovi { get; set; } = new();

        public decimal UkupnoVrednostSaMarzom  => Redovi.Sum(r => r.VrednostSaMarzom);
        public decimal UkupnoPocetnoStanje     => Redovi.Sum(r => r.PocetnoStanje);
        public decimal UkupnoIsplata           => Redovi.Sum(r => r.Isplata);
        public decimal UkupnoStanjeFinal       => Redovi.Sum(r => r.StanjeFinal);
    }

    // ─────────────────────────────────────────────────────────────
    // Tabela 3 — Obračun Poslovanja
    // ─────────────────────────────────────────────────────────────

    public class KalkulacijaObracunPosloStavka
    {
        public string Naziv { get; set; } = string.Empty;
        public decimal Vrednost { get; set; }
        public string? Napomena { get; set; }
    }

    public class KalkulacijaObracunPosloResult
    {
        // Red 1: SUM(Potrazuje) na KL- za period
        public decimal NabavnaVrednostSezona { get; set; }

        // Red 2: PrenosaZaliha × prosečna nabavna cena
        public decimal NabavnaVrednostLager { get; set; }

        // Red 3: SUM(kg sirovine kupljene) × cena prerade/kg sirovine
        public decimal Prerada { get; set; }

        // Red 4: SUM(Duguje) na FK- za period
        public decimal Prodaja { get; set; }

        // Detalji po vrsti za prikaz tooltipa/breakdown-a
        public List<KalkulacijaNabavnaVrednostLagerRed> LagerDetalji { get; set; } = new();
        public List<KalkulacijaPreradeRed> PreradeDetalji { get; set; } = new();
    }

    public class KalkulacijaNabavnaVrednostLagerRed
    {
        public string VrstaVoca { get; set; } = string.Empty;
        public decimal Kg { get; set; }
        public decimal CenaPoKg { get; set; }
        public bool CenaJeRucna { get; set; }   // true = iz JSON, false = iz baze
        public decimal Vrednost => Kg * CenaPoKg;
    }

    public class KalkulacijaPreradeRed
    {
        public string VrstaVoca { get; set; } = string.Empty;
        public decimal KgSirovine { get; set; }
        public decimal CenaPrerade { get; set; }
        public decimal Vrednost => KgSirovine * CenaPrerade;
    }

    // ─────────────────────────────────────────────────────────────
    // Konfiguracija — artikli za ručno unošenje nabavnih cena
    // ─────────────────────────────────────────────────────────────

    public class KalkulacijaArtikalNaCeniRed
    {
        public int ArtikalID { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string VrstaVoca { get; set; } = string.Empty;
        public string NazivMagacina { get; set; } = string.Empty;
        public decimal KgNaLageru { get; set; }
        // prosečna cena iz baze (prethodnih 12 meseci pre sezone)
        public decimal CenaIzBaze { get; set; }
        // ručno uneta cena (iz JSON konfiguracije) — 0 ako nije uneta
        public decimal CenaRucna { get; set; }
        // koju cenu sistem koristi
        public decimal AktivnaCena => CenaRucna > 0 ? CenaRucna : CenaIzBaze;
        public bool CenaJeRucna => CenaRucna > 0;
    }

    // ─────────────────────────────────────────────────────────────
    // Tabela 3 — Red 5: Vrednost Robe na Zalihama
    // ─────────────────────────────────────────────────────────────

    public class KalkulacijaVrednostZalihaRed
    {
        public int ArtikalID { get; set; }
        public string NazivGotovog { get; set; } = string.Empty;
        public string VrstaVoca { get; set; } = string.Empty;

        // kg dobijeni primenom formule iskorišćenja na sirovine na lageru
        public decimal KgIzSirovine { get; set; }

        // kg gotovog_roba koji su već u magacinu (MagacinID=6)
        public decimal KgDirektno { get; set; }

        public decimal UkupnoKg => KgIzSirovine + KgDirektno;
        public decimal CenaPoKg { get; set; }
        public decimal Vrednost => UkupnoKg * CenaPoKg;
    }

    public class KalkulacijaVrednostZalihaResult
    {
        public List<KalkulacijaVrednostZalihaRed> Redovi { get; set; } = new();
        public decimal UkupnoVrednost => Redovi.Sum(r => r.Vrednost);
        public bool NedostajuCene => Redovi.Any(r => r.CenaPoKg == 0 && r.UkupnoKg > 0);
    }

    // ─────────────────────────────────────────────────────────────
    // Konfiguracija — artikli za isključivanje iz Prenos Zaliha
    // ─────────────────────────────────────────────────────────────

    public class KalkulacijaLagerPrenosStavka
    {
        public int ArtikalID { get; set; }
        public int KlasId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string VrstaVoca { get; set; } = string.Empty;
        public string NazivMagacina { get; set; } = string.Empty;
        public bool JeLagerProizvodnje { get; set; }
        public int? KomitentID { get; set; }
        public string? NazivKomitenta { get; set; }
        public decimal Kolicina { get; set; }
        public bool Iskljuci { get; set; }
        /// <summary>Datum na koji se odnosi kolona Kolicina (dan pre sezone).</summary>
        public DateTime? DatumPrenosa { get; set; }
    }

    // ─────────────────────────────────────────────────────────────
    // Filter / period
    // ─────────────────────────────────────────────────────────────

    public class KalkulacijaPeriod
    {
        public DateTime OdDatum { get; set; }
        public DateTime DoDatum { get; set; }

        // dan pre početka sezone — za Prenos Zaliha
        public DateTime DanPreSezone => OdDatum.AddDays(-1);
    }

}
