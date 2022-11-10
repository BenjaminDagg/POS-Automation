using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model.Reports
{
    public class CashBankActivityReport<T> : Report<T> where T : CashBankActivityReportRecord
    {
        public decimal TotalPayout { get; set; }
        public decimal TotalMoney { get; set; }

        public CashBankActivitySession GetSession(string session)
        {
            var result = new CashBankActivitySession();

            foreach(var record in Data)
            {
                foreach(var sesh in record.Sessions)
                {
                    if(sesh.SessionId == session)
                    {
                        return sesh;
                    }
                }
            }

            return result;
        }
    }
}
