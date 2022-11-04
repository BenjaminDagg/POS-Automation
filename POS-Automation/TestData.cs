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
        public static string SuperUserUsername = ConfigurationManager.AppSettings["SuperUserUsername"];
        public static string SuperUserPassword = ConfigurationManager.AppSettings["SuperUserPassword"];
        public static string SupervisorUsername = ConfigurationManager.AppSettings["SupervisorUsername"];
        public static string SupervisorPassword = ConfigurationManager.AppSettings["SupervisorPassword"];
        public static string CashierUsername = ConfigurationManager.AppSettings["CashierUsername"];
        public static string CashierPassword = ConfigurationManager.AppSettings["CashierPassword"];

        //Payout Settings 
        public static int CashDrawerMaxBalance = int.Parse(ConfigurationManager.AppSettings["CashdrawerMaxBalance"]);

        //Transaction Portal
        public static string TransactionPortalIpAddress = ConfigurationManager.AppSettings["TransactionPortalIpAddress"];
        public static int TpPort = int.Parse(ConfigurationManager.AppSettings["TransactionPortalPortNumber"]);

        /* Database Settings */
        public static string DbServer = ConfigurationManager.AppSettings["DbServer"];
        public static string DbName = ConfigurationManager.AppSettings["DbName"];
        public static string DbUserName = ConfigurationManager.AppSettings["DbUsername"];
        public static string DbPassword = ConfigurationManager.AppSettings["DbPassword"];

        /* Deal Setup */
        public static int GameplayDealNumber = int.Parse(ConfigurationManager.AppSettings["GameplayDealNumber"]);   //Works: 120381. Can be any existing deal

        /* Machine Setup */
        public static string DefaultMachineNumber = ConfigurationManager.AppSettings["DefaultMachineNumber"];    //default machine in the list
        public static string DefaultLocationMachineNumber = ConfigurationManager.AppSettings["DefaultLocationMachineNumber"];
        public static string DefaultIPAddress = ConfigurationManager.AppSettings["DefaultMachineIpAddress"];
        public static string DefaultSerialNumber = ConfigurationManager.AppSettings["DefaultMachineSerialNumber"];
        public static string TestMachineNumber = ConfigurationManager.AppSettings["TestMachineNumber"];
        public static string TestLocationMachineNumber = ConfigurationManager.AppSettings["TestLocationMachineNumber"];
        public static string TestMachineSerialNumber = ConfigurationManager.AppSettings["TestMachineSerialNumber"];
        public static string TestMachineIpAddress = ConfigurationManager.AppSettings["TestMachineIpAddress"];

        /* Bank Settings */
        //can be any bank that exists
        public static int TestBankNumber = int.Parse(ConfigurationManager.AppSettings["DefaultBankNumber"]);
        public static string TestGameCode = ConfigurationManager.AppSettings["DefaultGameCode"];
        public static string TestGameTypeCode = ConfigurationManager.AppSettings["DefaultGameTypeCode"];

        /* Location Setup */
        public static int LocationId = int.Parse(ConfigurationManager.AppSettings["DefaultLocationId"]);

        public static int PollingIntervalSec = 5;

        public static string ConfigFilePath = ConfigurationManager.AppSettings["ConfigFileJsonPath"];
    }
}
