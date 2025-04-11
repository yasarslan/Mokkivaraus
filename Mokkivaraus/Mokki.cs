using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mokkivaraus
{
    public class Mokki
    {
        public string Nimi { get; set; }
        public string Sijainti { get; set; }
        public decimal Hinta { get; set; }
        public int Kapasiteetti { get; set; }
        public string Alue { get; set; }
        public string Varaustilanne { get; set; } = "Vapaa"; // default "Vapaa"
    }

}
