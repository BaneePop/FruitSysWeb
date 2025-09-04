using System.ComponentModel.DataAnnotations;
using System.Text;
using FruitSysWeb.Models;

namespace FruitSysWeb.Models
{
    public class PreradaPregledModel
    {
        public DateTime? Datum { get; set; }
        public string RadniNalog { get; set; } = string.Empty;
        public string Artikal { get; set; } = string.Empty;
        public decimal? KolicinaGotovProizvod { get; set; }
        public decimal? KolicinaSirovina { get; set; }
        public string Komitent { get; set; } = string.Empty;
    }
}