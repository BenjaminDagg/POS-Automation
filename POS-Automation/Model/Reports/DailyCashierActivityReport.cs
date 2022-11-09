using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace POS_Automation.Model.Reports
{
    public class DailyCashierActivityReport<T> : Report<T>
    {
        public int TotalVouchers { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalTransactions { get; set; }
    }
}
