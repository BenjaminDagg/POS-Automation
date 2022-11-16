using NUnit.Framework;
using System;
using System.Threading;
using POS_Automation.Model;
using System.Threading.Tasks;
using POS_Automation.Pages;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace POS_Automation
{
    public class DeviceManagementSocketTests : BaseTest
    {
        private LoginPage _loginPage;
        private DeviceManagementPage _devicePage;
        private SettingsPage _settingsPage;
        private TransactionPortalClient GameClient;
        private TransactionPortalClient PosClient;
        private PromoTicketRepository promoTicketRepository;

        [SetUp]
        public void Setup()
        {
            _loginPage = new LoginPage(driver);
            _devicePage = new DeviceManagementPage(driver);
            _settingsPage = new SettingsPage(driver);

            promoTicketRepository = new PromoTicketRepository();

            DatabaseManager.ResetTestMachine();

            GameClient = new TransactionPortalClient(TestData.TransactionPortalIpAddress, 4550);
            GameClient.Connect();

            PosClient = new TransactionPortalClient(TestData.TransactionPortalIpAddress, 4551);
            PosClient.Connect();

            //GameClient.Listen().Wait();
        }

        [TearDown]
        public void TearDown()
        {
            //DatabaseManager.ResetTestMachine();
            GameClient.CLose();
            PosClient.CLose();
        }


        [Test]
        public async Task GetAllMachines()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            string res = PosClient.Execute($"3,Z,{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},GetAllMachines");

            Assert.True(res.Contains("GetAllMachines"));
            Assert.True(res.Contains(TestData.DefaultMachineNumber));   //response returns connected machine mach no
            Assert.True(res.Contains(TestData.DefaultIPAddress));   //response returns connected machine IP address
            Assert.True(res.IndexOf(",Z,") != -1);  //response is a Z trans
            Assert.True(res.Substring(res.Length - 2, 2) == ",0");  //response code should be 0
        }
    }
}