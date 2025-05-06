using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mokkivaraus.Models
{
    public class Asiakas
    {
        public int asiakasID { get; set; }
        public string? etunimi { get; set; }
        public string? sukunimi { get; set; }
        public string? postiNo { get; set; }
        public string? lahiOsoite { get; set; }
        public string? email {  get; set; }
        public string? puhelin {  get; set; }
        public int varausID {  get; set; }

        public string? kokonaisNimi=>$"{etunimi} {sukunimi}";
    }
}
