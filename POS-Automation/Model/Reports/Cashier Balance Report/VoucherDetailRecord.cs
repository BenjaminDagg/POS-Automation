using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model.Reports.Cashier_Balance_Report
{
    public class VoucherDetailRecord
    {
        public string VoucherNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
