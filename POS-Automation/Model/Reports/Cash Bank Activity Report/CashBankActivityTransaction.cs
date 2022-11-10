using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model.Reports
{
    public class CashBankActivityTransaction
    {
        public string SessionId { get; set; }
        public string Station { get; set; }
        public string VoucherNumber { get; set; }
        public int ReferenceNumber { get; set; }
        public string TransType { get; set; }
        public decimal Money { get; set; }
        public decimal Payout { get; set; }
        public DateTime Date { get; set; }
    }
}
