using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model.Reports
{
    public class CashBankActivitySession
    {
        public string SessionId { get; set; }
        public List<CashBankActivityTransaction> Transactions { get; set; }
        public decimal TotalMoney { get; set; }
        public decimal TotalPayout { get; set; }

        public CashBankActivitySession()
        {
            Transactions = new List<CashBankActivityTransaction>();
        }
    }
}
