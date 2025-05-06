using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mokkivaraus
{
    public class Palvelu
    {
        public int PalveluID { get; set; }
        public string? AlueNimi { get; set; }
        public int AlueID { get; set; }
        public string? PalveluNimi {  get; set; }
        public string? Kuvaus {  get; set; }
        public decimal Hinta { get; set; }
        public decimal Alv {  get; set; }


    }
}
