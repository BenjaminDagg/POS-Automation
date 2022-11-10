using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model.Reports
{
    public class DailyCashierActivityReportRecord
    {
        
        public List<CashierActivityRecord> Activities;
        public string CreatedBy { get; set; }
        public int TotalVouchers { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalTransactions { get; set; }
    }
}
