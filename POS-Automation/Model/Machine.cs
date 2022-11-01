using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model
{
    public class Machine
    {
        public string Id { get; set; }
        public string MachNo { get; set; }
        public string IpAddress { get; set; }
        public DateTime LastPlayed { get; set; }
        public char? TransType { get; set; }
        public string Description { get; set; }
        public Decimal Balance { get; set; }
        public bool Status { get; set; }
    }
}
