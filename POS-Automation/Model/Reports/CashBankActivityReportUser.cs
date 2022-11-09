using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model.Reports
{
    public class CashBankActivityReportUser
    {
        public List<CashBankActivitySession> Sessions;
        public decimal TotalPayout { get; set; }
        public decimal TotalMoney { get; set; }
        public string CreatedBy { get; set; }

        public CashBankActivityReportUser()
        {
            Sessions = new List<CashBankActivitySession>();
        }
    }
}
