using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Model.Settings
{
    public class DeviceManagerSettings
    {
        public int PollingInterval { get; set; }
        public int ConnectionTimeOut { get; set; }
        public string ServiceEndPoint { get; set; }
        public int ServicePort { get; set; }
    }
}
