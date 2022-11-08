using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model
{
    public class ReportListItem
    {
        public string ReportName { get; set; }
        public DateTime? LastRun { get; set; }
    }
}
