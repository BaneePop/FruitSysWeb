using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class Artikal
    {
        [Display(Name = "ID")]
        public long Id { get; set; }

        [Display(Name = "Naziv")]
        public string Naziv { get; set; } = string.Empty;

        [Display(Name = "Tip")]
        public int MagacinID { get; set; }

        [Display(Name = "Tip naziv")]
        public string TipNaziv => GetTipNaziv(MagacinID);

        [Display(Name = "Jedinica mere ID")]
        public int? JedinicaMereID { get; set; }

        [Display(Name = "Datum kreiranja")]
        public DateTime? Kreirano { get; set; }

        [Display(Name = "Tip naziv")]
        public string AmbalazaTip => GetAmbalazaTip(MagacinID);

        // ✅ ISPRAVKA: Oba polja za kompatibilnost
        [Display(Name = "Aktivan")]
        public int? Aktivan { get; set; }

        [Display(Name = "Aktivno")]
        public bool Aktivno => Aktivan == 1;

        private static string GetTipNaziv(int MagacinID)
        {
            return MagacinID switch
            {
                1 => "Ne postojeci",
                2 => "Sveza roba",
                3 => "Sirovina",
                4 => "Ambalaza",
                5 => "Poluproizvod",
                6 => "Gotova roba",
                7 => "Kalo i Rastur",
                8 => "Usluzni Lager",
                9 => "Repromaterijal",
                10 => "Djubriva",
                11 => "Usluzni Lager Voce i Povrce",
                12 => "Usluzni Lager Razno",

                _ => $"MagacinID {MagacinID}"
            };
        }

        private static string GetAmbalazaTip(int tip)
        {
            return tip switch
            {
                1 => "Gajba",
                2 => "Kesa",
                3 => "Kutija",
                4 => "Paleta",
                _ => $"Tip {tip}"
            };
        }
    }
}
