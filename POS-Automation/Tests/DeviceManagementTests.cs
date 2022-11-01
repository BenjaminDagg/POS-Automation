using NUnit.Framework;
using System;
using System.Threading;
using POS_Automation.Model;
using System.Threading.Tasks;

namespace POS_Automation
{
    public class DeviceManagementTests : BaseTest
    {
        private LoginPage _loginPage;
        private DeviceManagementPage _devicePage;
        private GameSimulator GameSimulator;

        [SetUp]
        public void Setup()
        {
            _loginPage = new LoginPage(driver);
            _devicePage = new DeviceManagementPage(driver);

            DatabaseManager.ResetTestMachine();

            GameSimulator = new GameSimulator(_logService);
            GameSimulator.StartUp();
        }

        [TearDown]
        public void TearDown()
        {
            GameSimulator.ShutDown();
            DatabaseManager.ResetTestMachine();
        }

        [Test]
        public async Task NoMachinesButtonsReadonly()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            GameSimulator.ShutDown();
            Thread.Sleep(10000);

            Assert.True(_devicePage.IsReadOnly(_devicePage.SetAllOnlineButton));
            Assert.True(_devicePage.IsReadOnly(_devicePage.SetAllOfflineButton));
        }

        [Test]
        public async Task SetAllOnlinePrompt()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            GameSimulator.SetOffline();
            Thread.Sleep(10000);

            driver.FindElement(_devicePage.SetAllOnlineButton).Click();
            Assert.True(_devicePage.ChangeStatusAlert.IsOpen);
        }

        [Test]
        public async Task SetAllOfflinePrompt()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            driver.FindElement(_devicePage.SetAllOfflineButton).Click();
            Assert.True(_devicePage.ChangeStatusAlert.IsOpen);
        }

        [Test]
        public void SuccesfullLogin()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            //GameSimulator.SetOffline();
            
            Thread.Sleep(20000);
            Byte[] data = System.Text.Encoding.ASCII.GetBytes("test");
            data = new Byte[256];

            // String to store the response ASCII representation.
            String responseData = String.Empty;

            // Read the first batch of the TcpServer response bytes.
            Int32 bytes = GameSimulator.transactionPortalService.tpClient.tcpClient.GetStream().Read(data, 0, data.Length); //(**This receives the data using the byte method**)
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes); //(**This converts it to string**)
            Console.WriteLine(responseData);

            GameSimulator.transactionPortalService.tpClient.Execute("3,Z,2021-07-15 10:39:22,0,,0,ShutdownMachine");

            var mach = _devicePage.GetMachineByMachNo("00001");

            Assert.False(mach.Status);
        }


    }
}