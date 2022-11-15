using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model.Reports.Cashier_Balance_Report
{
    public class CashRemovedRecord
    {
        public string MachineNumber { get; set; }
        public string MachineDescription { get; set; }
        public decimal CashRemoved { get; set; }
        public DateTime TImestamp { get; set; }
    }
}
