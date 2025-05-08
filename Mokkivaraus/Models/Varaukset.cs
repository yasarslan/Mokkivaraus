using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mokkivaraus.Models
{
    public class Varaukset
    {
        public int? varausID{get;set;}
        public Asiakas? asiakasVarattu{get;set;}
        public Mokki? mokkiVarattu{get;set;}
        public string? Alue{get;set;}
        public Palvelu? Palvelu{get;set;}
        public string? palvelutVarattu{get;set;}
        public DateTime? varausPaiva{get;set;}
        public DateTime? varausAlku{get;set;}
        public DateTime? varausLoppu{get;set;}
        public double? kokonaisKustannus{get;set;}
    }
}
