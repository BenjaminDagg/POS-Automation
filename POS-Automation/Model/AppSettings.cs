using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POS_Automation.Model.Settings;

namespace POS_Automation.Model
{
    public class AppSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; }
        public Logging Logging { get; set; }
        public DefaultAccount DefaultAccount { get; set; }
        public UserPrinterSettings UserPrinterSettings { get; set; }
        public UserVoucherSettings UserVoucherSettings { get; set; }
        public DeviceManagerSettings DeviceManagerSettings { get; set; }
        public StartUpMode StartUpMode { get; set; }
        public TestDbRetryConfiguration TestDbRetryConfiguration { get; set; }
    }
}
