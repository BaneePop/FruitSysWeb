
using System.ComponentModel.DataAnnotations;

namespace FruitSysWeb.Models
{
    public class Komitent
    {
        public int Id { get; set; }
        public string? Naziv { get; set; }
        public string? Adresa { get; set; }
        public long ID { get; set; }
        public bool FizickoLice { get; set; }
        public bool JeDobavljac { get; set; }
        public bool JeKupac { get; set; }
        public bool JeOtkupljivac { get; set; }
        public bool Aktivan { get; set; } = true;
        public int Tip { get; set; } // DODAJTE OVO

    }
}