using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model.Reports.Cashier_Balance_Report
{
    public class CashierSessionSummaryRecord
    {
        public string SessionId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal StartBalance { get; set; }
        public decimal TotalPayoutAmount { get; set; }
        public decimal TotalAmountAdded { get; set; }
        public decimal TotalAmountRemoved { get; set; }
        public decimal EndBalance { get; set; }
    }
}
