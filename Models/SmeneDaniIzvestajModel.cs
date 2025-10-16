using System;

namespace FruitSysWeb.Models
{
    /// <summary>
    /// Model za izveštaj smenskog rada po danima i nedeljama
    /// Kombinuje podatke iz SmenskiIzvestaj, EvidencijaRada, ProizvodniProces tabela
    /// </summary>
    public class SmeneDaniIzvestajModel
    {
        // Osnovni identifikatori - neće se prikazivati u UI
        public long ID { get; set; }
        public long SmenskiIzvestajID { get; set; }
        public long? PoslovodjaID { get; set; }
        public long? ProizvodniProcesID { get; set; }

        // Prikaz kolone - glavne kolone za prikaz
        public string BrojIzvestaja { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public int Smena { get; set; }
        public int DokumentStatus { get; set; }

        // Poslovne informacije
        public string? Smenovoda { get; set; }
        public string? ProizvodniProces { get; set; }

        // Podaci o radu - agregirani iz EvidencijaRada
        public decimal UkupnoSati { get; set; }
        public decimal UkupanTrosak { get; set; }
        public int UkupnoBrojRadnika { get; set; }

        // Produktivnost i efikasnost - kalkulisano
        public decimal Produktivnost { get; set; }  // kg/h ili komada/h
        public decimal Efikasnost { get; set; }     // procenat iskorišćenja
        public decimal UkupnaKolicina { get; set; } // ukupna proizvedena količina

        // Vremenski podaci
        public int DanUNedelji { get; set; }         // 1=Ponedeljak, 7=Nedelja
        public int NedeljaUGodini { get; set; }
        public int MesecUGodini { get; set; }

        // Metadata - sistemske kolone
        public DateTime Kreirano { get; set; }
        public DateTime Azurirano { get; set; }
        public int Version { get; set; }

        // Kalkulisana svojstva za UI
        public string StatusText
        {
            get
            {
                return DokumentStatus switch
                {
                    2 => "Otvoren",
                    3 => "Zaključen",
                    4 => "Storno",
                    _ => "Nepoznato"
                };
            }
        }

        public string StatusBadgeClass
        {
            get
            {
                return DokumentStatus switch
                {
                    2 => "bg-primary",     // Otvoren - plavo
                    3 => "bg-success",     // Zaključen - zeleno  
                    4 => "bg-danger",      // Storno - crveno
                    _ => "bg-secondary"
                };
            }
        }
    }

    /// <summary>
    /// Helper model za RadniProces dropdown
    /// </summary>
    /* public class RadniProcesModel
    {
        public long ID { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public int? RadniProcesTip { get; set; }
        public long? RezijaID { get; set; }
        public DateTime Kreirano { get; set; }
        public DateTime Azurirano { get; set; }
        public int Version { get; set; }
    } */

    /// <summary>
    /// Helper model za ProizvodniProces dropdown
    /// </summary>
    /* public class ProizvodniProcesModel
    {
        public long ID { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public DateTime Kreirano { get; set; }
        public DateTime Azurirano { get; set; }
        public int Version { get; set; }
    } */
}
