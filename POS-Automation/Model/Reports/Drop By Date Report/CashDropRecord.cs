using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model.Reports.Drop_By_Date_Report
{
    public class CashDropRecord
    {
        public string TerminalId { get; set; }
        public int Amount5Dollar { get; set; }
        public int Amount10Dollar { get; set; }
        public int Amount1Dollar { get; set; }
        public int Amount20Dollar { get; set; }
        public int Amount50Dollar { get; set; }
        public int Amount100Dollar { get; set; }
        public decimal TotalTicketAmount { get; set; }
        public int TotalTickets { get; set; }
        public int TotalBills { get; set; }
        public decimal TotalDropAmount { get; set; }
        public DateTime DropTime { get; set; }
        public string Account { get; set; }
    }
}
