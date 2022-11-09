using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace POS_Automation.Model.Reports
{
    public class Report<T>
    {
       
        public List<T> Data{ get; set; }

        public string Title { get; set; }
        public DateTime RunDate { get; set; }
        public string ReportPeriod { get; set; }

        public static Cell TitleCell => new Cell(4, 1);

        public static Cell PeriodCell => new Cell(6, 1);

        public static Cell RuntimeCell => new Cell(2, 6);

    }
}
