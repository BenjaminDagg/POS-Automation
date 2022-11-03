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
    public class SetMachineOnlineTest : BaseTest
    {
        private LoginPage _loginPage;
        private DeviceManagementPage _devicePage;
        private SettingsPage _settingsPage;
        private GameSimulator GameSimulator;
        private MachineRepository _machineRepository;
        private TransactionPortalClient PosClient;


        [SetUp]
        public void Setup()
        {
            _loginPage = new LoginPage(driver);
            _devicePage = new DeviceManagementPage(driver);
            _settingsPage = new SettingsPage(driver);

            _machineRepository = new MachineRepository();

            DatabaseManager.ResetTestMachine();

            GameSimulator = new GameSimulator(_logService);
            GameSimulator.StartUp();

            PosClient = new TransactionPortalClient(TestData.TransactionPortalIpAddress, 4551);
            PosClient.Connect();

            //GameSimulator.transactionPortalService.tpClient.Listen().Wait();
        }

        [TearDown]
        public void TearDown()
        {
            GameSimulator.ShutDown();
            DatabaseManager.ResetTestMachine();

            PosClient.CLose();
        }

        [Test]
        public void SetAllOnlinePrompt()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            GameSimulator.SetOffline();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            _devicePage.ClickSetAllOnlineButton();
            Assert.True(_devicePage.ChangeStatusAlert.IsOpen);

            string alertText = _devicePage.ChangeStatusAlert.AlertText;
            Assert.AreEqual("Are you sure you would like to set all machines online?", alertText);
        }

        [Test]
        public void SetAllOnlineCancel()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            GameSimulator.SetOffline();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            _devicePage.ClickSetAllOnlineButton();
            Assert.True(_devicePage.ChangeStatusAlert.IsOpen);
            _devicePage.ChangeStatusAlert.Cancel();

            var machine = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            Assert.False(machine.Status);
        }

        [Test]
        public void SetMachineOnlinePrompt()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            GameSimulator.SetOffline();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            _devicePage.ClickMachineActionButton(TestData.DefaultMachineNumber);
            Assert.True(_devicePage.ChangeStatusAlert.IsOpen);

            string alertText = _devicePage.ChangeStatusAlert.AlertText;
            Assert.AreEqual($"Are you sure you would like to set Machine ID {TestData.DefaultMachineNumber} Online?", alertText);
        }

        [Test]
        public void SetMachineOnlineCancel()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            GameSimulator.SetOffline();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            _devicePage.ClickMachineActionButton(TestData.DefaultMachineNumber);
            Assert.True(_devicePage.ChangeStatusAlert.IsOpen);
            _devicePage.ChangeStatusAlert.Cancel();

            var machine = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            Assert.False(machine.Status);
        }

        [Test]
        public void SetMachineOnline()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            GameSimulator.SetOffline();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var machineBefore = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            Assert.False(machineBefore.Status);

            _devicePage.SetMachineOnline(TestData.DefaultMachineNumber);
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var machineAfter = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            Assert.True(machineAfter.Status);
        }

        [Test]
        public void SetAllOnline()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            GameSimulator.SetOffline();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var machineBefore = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            Assert.False(machineBefore.Status);

            _devicePage.SetAllOnline();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var machineAfter = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            Assert.True(machineAfter.Status);
        }

        [Test]
        public void TpMessageRequest()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            GameSimulator.SetOffline();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            string dateString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var request = $"4,Z,{dateString},StartupMachine,{TestData.DefaultIPAddress}";
            var response = PosClient.Execute(request);

            Assert.True(response.Contains(",0,,0,StartupMachine"));
        }

        [Test]
        public void TpMessageResponseGame()
        {
            

            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            GameSimulator.SetOffline();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            _devicePage.SetMachineOnline(TestData.DefaultMachineNumber);

            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var response = GameSimulator.transactionPortalService.tpClient.Read();
            Console.WriteLine(response);

            Assert.True(response.Contains("Startup"));
        }
    }
}