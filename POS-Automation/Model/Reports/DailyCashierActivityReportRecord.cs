using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model.Reports
{
    public class DailyCashierActivityReportRecord
    {
        public string CreatedBy { get; set; }
        public string SessionId { get; set; }
        public string Station { get; set; }
        public string VoucherNumber { get; set; }
        public decimal PayoutAmount { get; set; }
        public int ReceiptNumber { get; set; }
        public DateTime Date { get; set; }
    }
}
