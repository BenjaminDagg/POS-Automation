using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model.Reports.Drop_By_Date_Report
{
    public class DropByDateReport<T> : Report<T> where T : CashDropRecord
    {
        public static Cell LocationCell = new Cell(2, 2);
        public string Location { get; set; }
        public int Total1Dollar { get; set; }
        public int Total5Dollar { get; set; }
        public int Total10Dollar { get; set; }
        public int Total20Dollar { get; set; }
        public int Total50Dollar { get; set; }
        public int Total100Dollar { get; set; }
        public decimal TotalTicketAmount { get; set; }
        public int TotalTicketCount { get; set; }
        public int TotalBills { get; set; }
        public decimal TotalDropAmount { get; set; }

        public CashDropRecord GetLatestDrop(string terminal)
        {
            var sortedRecords = Data.OrderByDescending(r => r.DropTime);

            return sortedRecords.FirstOrDefault(r => r.TerminalId == terminal);
        }
    }
}
