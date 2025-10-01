namespace FruitSysWeb.Models
{
    public class SmenskiIzvestajModel
    {
        public string BrojIzvestaja { get; set; } = "";
        public DateTime? Datum { get; set; }
        public string ProizvodniProces { get; set; } = "";
        public string RadniProces { get; set; } = "";
        public int BrojRadnika { get; set; }
        public decimal BrojRadnihSati { get; set; }
        public decimal TrosakPoRadnomNalogu { get; set; }
        public decimal ProcenatIskoriscenja { get; set; }
        
        // Helper properties
        public string ProductivnostBadgeClass => ProcenatIskoriscenja switch
        {
            >= 90 => "bg-success",
            >= 70 => "bg-info",
            >= 50 => "bg-warning",
            _ => "bg-danger"
        };
    }
    
   /*  public class ProizvodniProcesModel
    {
        public int Id { get; set; }
        public string Naziv { get; set; } = "";
        public string? Opis { get; set; }
        public bool Aktivan { get; set; } = true;
    }
    
    public class RadniProcesModel
    {
        public int Id { get; set; }
        public string Naziv { get; set; } = "";
        public string? Opis { get; set; }
        public bool Aktivan { get; set; } = true;
        public int? ProizvodniProcesId { get; set; }
    } */
}