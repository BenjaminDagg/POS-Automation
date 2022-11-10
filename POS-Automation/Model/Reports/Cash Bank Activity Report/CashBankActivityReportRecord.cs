using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model.Reports
{
    public class CashBankActivityReportRecord
    {
        public List<CashBankActivitySession> Sessions;
        public decimal TotalPayout { get; set; }
        public decimal TotalMoney { get; set; }
        public string CreatedBy { get; set; }

        public CashBankActivityReportRecord()
        {
            Sessions = new List<CashBankActivitySession>();
        }
    }
}
