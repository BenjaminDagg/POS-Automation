using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using POS_Automation.Model.Reports;

namespace POS_Automation.Model.Reports
{
    public class DailyCashierActivityReport<T> : Report<T> where T : DailyCashierActivityReportRecord
    {
        public int TotalVouchers { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalTransactions { get; set; }

        public List<CashierActivityRecord> GetRecordsBySessionsId(string session)
        {
            var records = new List<CashierActivityRecord>();

            foreach(DailyCashierActivityReportRecord result in Data)
            {
                foreach(CashierActivityRecord activity in result.Activities)
                {
                    if(activity.SessionId == session)
                    {
                        
                    }
                }
            }

            return records;
        }

        public string GetSessionByVoucher(string voucher)
        {
            string result = string.Empty;

            foreach (DailyCashierActivityReportRecord record in Data)
            {
                foreach (CashierActivityRecord activity in record.Activities)
                {
                    if(activity.VoucherNumber == voucher)
                    {
                        result =  activity.SessionId;
                    }
                }
            }

            return result;
        }
    }
}
