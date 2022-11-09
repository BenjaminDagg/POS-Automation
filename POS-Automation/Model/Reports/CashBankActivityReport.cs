using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model.Reports
{
    public class CashBankActivityReport<CashBankActivityReportRecord> : Report<CashBankActivityReportRecord>
    {
        public decimal TotalPayout { get; set; }
        public decimal TotalMoney { get; set; }
    }
}
