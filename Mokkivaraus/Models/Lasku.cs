using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mokkivaraus.Models
{
    public class Lasku
    {
        public string LaskuNumero { get; set; }
        public string Asiakas { get; set; }
        public string Summa { get; set; }
        public string Tuote { get; set; }
        public string Tila { get; set; }
        public string Paivamaara { get; set; }
    }
}
