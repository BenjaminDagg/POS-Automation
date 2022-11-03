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
    public class SetMachineOfflineTest : BaseTest
    {
        private LoginPage _loginPage;
        private DeviceManagementPage _devicePage;
        private SettingsPage _settingsPage;
        private GameSimulator GameSimulator;
        private MachineRepository _machineRepository;
        private TransactionPortalClient PosClient;
        private TransactionPortalClient GameClient;

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

            GameClient = new TransactionPortalClient(TestData.TransactionPortalIpAddress, 4550);
            GameClient.Connect();

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
            GameClient.CLose();
        }

        [Test]
        public void SetAllOfflinePrompt()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            _devicePage.ClickSetAllOfflineButton();
            Assert.True(_devicePage.ChangeStatusAlert.IsOpen);

            string alertText = _devicePage.ChangeStatusAlert.AlertText;
            Assert.AreEqual("Are you sure you would like to set all machines offline?", alertText);
        }

        [Test]
        public void SetAllOfflineCancel()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            _devicePage.ClickSetAllOfflineButton();
            Assert.True(_devicePage.ChangeStatusAlert.IsOpen);
            _devicePage.ChangeStatusAlert.Cancel();

            var machine = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            Assert.True(machine.Status);
        }

        [Test]
        public void SetMachineOfflinePrompt()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            _devicePage.ClickMachineActionButton(TestData.DefaultMachineNumber);
            Assert.True(_devicePage.ChangeStatusAlert.IsOpen);

            string alertText = _devicePage.ChangeStatusAlert.AlertText;
            Assert.AreEqual($"Are you sure you would like to set Machine ID {TestData.DefaultMachineNumber} Offline?", alertText);
        }

        [Test]
        public void SetMachineOfflineCancel()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            _devicePage.ClickMachineActionButton(TestData.DefaultMachineNumber);
            Assert.True(_devicePage.ChangeStatusAlert.IsOpen);
            _devicePage.ChangeStatusAlert.Cancel();

            var machine = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            Assert.True(machine.Status);
        }

        [Test]
        public void SetMachineOffline()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var machineBefore = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            Assert.True(machineBefore.Status);

            _devicePage.SetMachineOffline(TestData.DefaultMachineNumber);
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var machineAfter = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            Assert.False(machineAfter.Status);
        }

        [Test]
        public void SetAllOffline()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var machineBefore = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            Assert.True(machineBefore.Status);

            _devicePage.SetAllOffline();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var machineAfter = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            Assert.False(machineAfter.Status);
        }

        [Test]
        public void TpMessageResponse()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);
            _devicePage.SetMachineOffline(TestData.DefaultMachineNumber);
            string dateString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var request = $"4,Z,{dateString},ShutdownMachine,{TestData.DefaultIPAddress}";
            var response = PosClient.Execute(request);
            
            Console.WriteLine(response);
            Assert.True(response.Contains(",0,,0,ShutdownMachine"));
        }



        [Test]
        public void TpMessageResponseGame()
        {


            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);
            _devicePage.SetMachineOffline(TestData.DefaultMachineNumber);
            /*
            string dateString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var request = $"4,Z,{dateString},StartupMachine,{TestData.DefaultIPAddress}";
            PosClient.Execute(request);
            */

            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var response = GameSimulator.transactionPortalService.tpClient.Read();
            Console.WriteLine(response);

            Assert.True(response.Contains("Shutdown,300,Transaction Portal Control initiated."));
        }
    }
}