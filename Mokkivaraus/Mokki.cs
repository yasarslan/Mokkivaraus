using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;


namespace Mokkivaraus
{
    public class Mokki
    {
        public int Mokki_id { get; set; }              
        public string? MokkiNimi { get; set; }
        public string? Postinumero { get; set; }
        public string? Katuosoite { get; set; }
        public decimal Hinta { get; set; }
        public string? Kuvaus { get; set; }
        public int Henkilomaara { get; set; }
        public string? Alue { get; set; }
        public int AlueID { get; set; }
        public string? Varustelu { get; set; } = "Vapaa"; // default "Vapaa"
    }

}
