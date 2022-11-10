using POS_Automation.Model.Reports.Cashier_Balance_Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model.Reports
{
    public class CashierBalanceReport<T> : Report<T> where T : CashierSessionSummaryRecord
    {
        
        public static Cell TitleCell = new Cell(3, 5);

        public static Cell PeriodCell = new Cell(6,3);

        public static Cell RuntimeCell = new Cell(2, 13);
        public List<VoucherDetailRecord> UnpaidVouchers { get; set; }
        public decimal TotalStartingBalance { get; set; }
        public decimal TotalPayoutAmount { get; set; }
        public decimal TotalAmountAdded { get; set; }
        public decimal TotalAmountRemoved { get; set; }
        public decimal TotalEndBalance { get; set; }
        public decimal TotalUnpaidVoucherAmount { get; set; }

        public CashierBalanceReport()
        {
            UnpaidVouchers = new List<VoucherDetailRecord>();
        }
    }
}
