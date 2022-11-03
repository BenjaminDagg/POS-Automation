using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model.Settings
{
    public class TestDbRetryConfiguration
    {
        public string NumberTimesToRetry { get; set; }
        public string SecondsToWait { get; set; }
    }
}
