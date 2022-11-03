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
    public class DeviceManagementTests : BaseTest
    {
        private LoginPage _loginPage;
        private DeviceManagementPage _devicePage;
        private SettingsPage _settingsPage;
        private GameSimulator GameSimulator;
        private MachineRepository _machineRepository;
        private AppSettings _appSettings;

        [SetUp]
        public void Setup()
        {
            _appSettings = AppSettingsManager.Read();

            _loginPage = new LoginPage(driver);
            _devicePage = new DeviceManagementPage(driver);
            _settingsPage = new SettingsPage(driver);

            _machineRepository = new MachineRepository();

            DatabaseManager.ResetTestMachine();

            GameSimulator = new GameSimulator(_logService);
            GameSimulator.StartUp();
        }

        [TearDown]
        public void TearDown()
        {
            AppSettingsManager.Write(_appSettings);

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

        //set all offline/online buttons should be disabled if a connection cannot be made
        [Test]
        public async Task ServerDisconnectedButtonsReadonly()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickSetingsTab();

            _settingsPage.EnterIpAddress("1.1.1.1");
            _settingsPage.SaveDeviceSettings();

            NavigationTabs.ClickDeviceTab();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            Assert.True(_devicePage.IsReadOnly(_devicePage.SetAllOnlineButton));
            Assert.True(_devicePage.IsReadOnly(_devicePage.SetAllOfflineButton));

            NavigationTabs.ClickSetingsTab();
            _settingsPage.EnterIpAddress(TestData.TransactionPortalIpAddress);
            _settingsPage.SaveDeviceSettings();
        }

        [Test]
        public async Task SetAllOnlinePrompt()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            GameSimulator.SetOffline();
            Thread.Sleep(15000);

            driver.FindElement(_devicePage.SetAllOnlineButton).Click();
            Assert.True(_devicePage.ChangeStatusAlert.IsOpen);
        }

        [Test]
        public async Task MachineOnlineButtonsEnabled()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            Assert.True(_devicePage.IsReadOnly(_devicePage.SetAllOnlineButton));
            Assert.False(_devicePage.IsReadOnly(_devicePage.SetAllOfflineButton));
        }

        [Test]
        public async Task MachineOfflineButtonsEnabled()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            GameSimulator.SetOffline();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            Assert.False(_devicePage.IsReadOnly(_devicePage.SetAllOnlineButton));
            Assert.True(_devicePage.IsReadOnly(_devicePage.SetAllOfflineButton));
        }

        [Test]
        public void ReconnectButtonVisiblity()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            Assert.False(_devicePage.ReconnectBtnIsVisible());

            NavigationTabs.ClickSetingsTab();
            _settingsPage.EnterIpAddress("1.1.1.1");
            _settingsPage.SaveDeviceSettings();
            NavigationTabs.ClickDeviceTab();

            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);
            Assert.True(_devicePage.ReconnectBtnIsVisible());

            //reset ip to correct server
            NavigationTabs.ClickSetingsTab();
            _settingsPage.EnterIpAddress(TestData.TransactionPortalIpAddress);
            _settingsPage.SaveDeviceSettings();
        }

        [Test]
        public void SortDeviceListAscendingOrder()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();
            
            _devicePage.SortGridByHeaderAscending(1);
            Thread.Sleep(TestData.PollingIntervalSec * 1000);
            var machNos = _devicePage.GetValuesForColumn(1);
            var expectedMachNos = machNos.OrderBy(x => x);
            Assert.IsTrue(expectedMachNos.SequenceEqual(machNos));
            
            _devicePage.SortGridByHeaderAscending(2);
            Thread.Sleep(TestData.PollingIntervalSec * 1000);
            var ipAddresses = _devicePage.GetValuesForColumn(2);
            var expectedIPAddresses = ipAddresses.OrderBy(x => x);
            Assert.IsTrue(expectedIPAddresses.SequenceEqual(ipAddresses));
            
            _devicePage.SortGridByHeaderAscending(3);
            Thread.Sleep(TestData.PollingIntervalSec * 1000);
            var dates = _devicePage.GetValuesForColumn(3)
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => DateTime.ParseExact(x, "MM/dd/yyyy hh:mm tt", CultureInfo.InvariantCulture))
                .ToList();
            var expectedDates = dates.OrderBy(x => x);
            Assert.IsTrue(expectedDates.SequenceEqual(dates));
            
            _devicePage.SortGridByHeaderAscending(4);
            Thread.Sleep(TestData.PollingIntervalSec * 1000);
            var transTypes = _devicePage.GetValuesForColumn(4);
            var expectedTransTypes = transTypes.OrderBy(x => x);
            Assert.True(expectedTransTypes.SequenceEqual(transTypes));
            
            _devicePage.SortGridByHeaderAscending(5);
            Thread.Sleep(TestData.PollingIntervalSec * 1000);
            var deviceDescriptions = _devicePage.GetValuesForColumn(5);
            var expectedDescriptions = deviceDescriptions.OrderBy(x => x);
            Assert.True(expectedDescriptions.SequenceEqual(deviceDescriptions));
           
            _devicePage.SortGridByHeaderAscending(6);
            Thread.Sleep(TestData.PollingIntervalSec * 1000);
            var balances = _devicePage.GetValuesForColumn(6).Select(x => decimal.Parse(x, NumberStyles.Currency)).ToList();
            var expectedBalances = balances.OrderBy(x => x);
            Assert.True(expectedBalances.SequenceEqual(balances));
        }

        [Test]
        public void SortDeviceListDescendingOrder()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            _devicePage.SortGridByHeaderDescending(1);
            Thread.Sleep(TestData.PollingIntervalSec * 1000);
            var machNos = _devicePage.GetValuesForColumn(1);
            var expectedMachNos = machNos.OrderByDescending(x => x);
            Assert.IsTrue(expectedMachNos.SequenceEqual(machNos));
            
            _devicePage.SortGridByHeaderDescending(2);
            Thread.Sleep(TestData.PollingIntervalSec * 1000);
            var ipAddresses = _devicePage.GetValuesForColumn(2);
            var expectedIPAddresses = ipAddresses.OrderByDescending(x => x);
            Assert.IsTrue(expectedIPAddresses.SequenceEqual(ipAddresses));

            _devicePage.SortGridByHeaderDescending(3);
            Thread.Sleep(TestData.PollingIntervalSec * 1000);
            var dates = _devicePage.GetValuesForColumn(3)
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => DateTime.ParseExact(x, "MM/dd/yyyy hh:mm tt", CultureInfo.InvariantCulture))
                .ToList();
            var expectedDates = dates.OrderByDescending(x => x);
            Assert.IsTrue(expectedDates.SequenceEqual(dates));

            _devicePage.SortGridByHeaderDescending(4);
            Thread.Sleep(TestData.PollingIntervalSec * 1000);
            var transTypes = _devicePage.GetValuesForColumn(4);
            var expectedTransTypes = transTypes.OrderByDescending(x => x);
            Assert.True(expectedTransTypes.SequenceEqual(transTypes));

            _devicePage.SortGridByHeaderDescending(5);
            Thread.Sleep(TestData.PollingIntervalSec * 1000);
            var deviceDescriptions = _devicePage.GetValuesForColumn(5);
            var expectedDescriptions = deviceDescriptions.OrderByDescending(x => x);
            Assert.True(expectedDescriptions.SequenceEqual(deviceDescriptions));

            _devicePage.SortGridByHeaderDescending(6);
            Thread.Sleep(TestData.PollingIntervalSec * 1000);
            var balances = _devicePage.GetValuesForColumn(6).Select(x => decimal.Parse(x, NumberStyles.Currency)).ToList();
            var expectedBalances = balances.OrderByDescending(x => x);
            Assert.True(expectedBalances.SequenceEqual(balances));
            
        }

        [Test]
        public void VerifyDeviceListDataColumns()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            Assert.AreEqual("Mach ID", _devicePage.GetHeader(0));
            Assert.AreEqual("Mach NO", _devicePage.GetHeader(1));
            Assert.AreEqual("IP", _devicePage.GetHeader(2));
            Assert.AreEqual("Last Played", _devicePage.GetHeader(3));
            Assert.AreEqual("Trans Type", _devicePage.GetHeader(4));
            Assert.AreEqual("Description", _devicePage.GetHeader(5));
            Assert.AreEqual("Balance", _devicePage.GetHeader(6));
        }

        [Test]
        public void LoadMachines()
        {
            var machines = _machineRepository.GetAllMachines();

            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            int machCount = _devicePage.MachineCount;

            Assert.AreEqual(machines.Count,machCount);
        }

        [Test]
        public void StatusIconMachineOnline()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var status = _devicePage.MachineStatus(TestData.DefaultMachineNumber);
            string statusBtnText = _devicePage.MachineStatusButtonText(TestData.DefaultMachineNumber);

            Assert.True(status);
            Assert.AreEqual("Set Offline",statusBtnText);
        }

        [Test]
        public void StatusIconMachineOffline()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            GameSimulator.SetOffline();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var status = _devicePage.MachineStatus(TestData.DefaultMachineNumber);
            string statusBtnText = _devicePage.MachineStatusButtonText(TestData.DefaultMachineNumber);

            Assert.False(status);
            Assert.AreEqual("Set Online", statusBtnText);
        }

        [Test]
        public void StatusIconMachineDisconnected()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            GameSimulator.ShutDown();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var status = _devicePage.MachineStatus(3);
            string statusBtnText = _devicePage.MachineStatusButtonText(3);

            Assert.False(status);
            Assert.AreEqual(string.Empty, statusBtnText);
        }

        [Test]
        public void MachineLastPlayUpdate()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            var machBefore = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            
            GameSimulator.Play();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var machAfter = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            
            Assert.AreNotEqual(machAfter.LastPlayed,machBefore.LastPlayed);
            Assert.Greater(machAfter.LastPlayed,machBefore.LastPlayed);
        }

        [Test]
        public void MachineLastPlayDateFormat()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            GameSimulator.Play();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var lastPlayedDates = _devicePage.GetValuesForColumn(3);
            var targetDate = lastPlayedDates[0];
            DateTime outDate;
            string[] dateFormats = { "MM/dd/yyyy hh:mm:ss tt" };
            string[] dateStrings = { targetDate };

            Assert.True(DateTime.TryParseExact(targetDate, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out outDate));
        }

        [Test]
        public void LastTransactionTest()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            int amount = 0;
            GameSimulator.Win(out amount);
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var machine = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            Assert.AreEqual('W',machine.TransType);
        }

        [Test]
        public void MachineBalanceAddTest()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            var machineBefore = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            Assert.AreEqual(5,machineBefore.Balance);

            GameSimulator.BillIn(1);
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var machineAfter = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            Assert.AreEqual(6, machineAfter.Balance);
        }

        [Test]
        public void ActionButtonMachineOnline()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            string buttonText = _devicePage.MachineStatusButtonText(TestData.DefaultMachineNumber);
            Assert.AreEqual("Set Offline",buttonText);

            _devicePage.ClickMachineActionButton(TestData.DefaultMachineNumber);
            Assert.True(_devicePage.ChangeStatusAlert.IsOpen);
            
            string alertText = _devicePage.ChangeStatusAlert.AlertText;
            Assert.AreEqual($"Are you sure you would like to set Machine ID {TestData.DefaultMachineNumber} Offline?",alertText);
        }

        [Test]
        public void ActionButtonMachineOffline()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            GameSimulator.SetOffline();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            string buttonText = _devicePage.MachineStatusButtonText(TestData.DefaultMachineNumber);
            Assert.AreEqual("Set Online", buttonText);

            _devicePage.ClickMachineActionButton(TestData.DefaultMachineNumber);
            Assert.True(_devicePage.ChangeStatusAlert.IsOpen);

            string alertText = _devicePage.ChangeStatusAlert.AlertText;
            Assert.AreEqual($"Are you sure you would like to set Machine ID {TestData.DefaultMachineNumber} Online?", alertText);
        }

        [Test]
        public void ActionButtonMachineDisconnected()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            GameSimulator.ShutDown();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            string buttonText = _devicePage.MachineStatusButtonText(3);
            Assert.AreEqual(string.Empty, buttonText);
        }

        [Test]
        public void VerifyServerLabel()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickSetingsTab();

            string expectedIpAddress = _settingsPage.GetIpAddress();
            int expectedPort = _settingsPage.GetPortNumber();

            NavigationTabs.ClickDeviceTab();

            string actualServerText = _devicePage.Server;
            string expectedServer = expectedIpAddress + ":" + expectedPort.ToString();

            Assert.AreEqual(expectedServer, actualServerText);
        }

        [Test]
        public void VerifyPollingIntervalLabel()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickSetingsTab();

            int expectedPollInterval = _settingsPage.GetPollingInterval();

            NavigationTabs.ClickDeviceTab();

            int actualPollInterval = _devicePage.PollingInterval;

            Assert.AreEqual(expectedPollInterval, actualPollInterval);
        }

        [Test]
        public void ServerStatusLabelConnected()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            string actualServerStatusText = _devicePage.ConnectionStatus;
            Assert.AreEqual("Connected",actualServerStatusText);
        }

        [Test]
        public void ServerStatusLabelDisconnected()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickSetingsTab();

            _settingsPage.EnterIpAddress("1.1.1.1");
            _settingsPage.SaveDeviceSettings();

            NavigationTabs.ClickDeviceTab();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            string actualServerStatusText = _devicePage.ConnectionStatus;
            Assert.AreEqual("Disconnected", actualServerStatusText);

            //change server ip back
            NavigationTabs.ClickSetingsTab();
            _settingsPage.EnterIpAddress(TestData.TransactionPortalIpAddress);
            _settingsPage.SaveDeviceSettings();
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