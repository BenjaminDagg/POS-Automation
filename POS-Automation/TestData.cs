using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation
{
    public static class TestData
    {
        //User login
        public static string AdminUsername = ConfigurationManager.AppSettings["AdminUsername"];
        public static string AdminPassword = ConfigurationManager.AppSettings["AdminPassword"];
    }
}
