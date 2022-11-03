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
    public class MachineIntegrationTests : BaseTest
    {
        private LoginPage _loginPage;
        private DeviceManagementPage _devicePage;
        private SettingsPage _settingsPage;
        private GameSimulator GameSimulator;

        [SetUp]
        public void Setup()
        {

            _loginPage = new LoginPage(driver);
            _devicePage = new DeviceManagementPage(driver);
            _settingsPage = new SettingsPage(driver);

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
        public async Task LastPlayed_Loss()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            var machBefore = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);

            GameSimulator.Loss();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var machAfter = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);

            Assert.GreaterOrEqual(machAfter.LastPlayed, machBefore.LastPlayed);
        }

        [Test]
        public async Task LastPlayed_Win()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            var machBefore = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);

            int balance = 0;
            GameSimulator.Win(out balance);
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var machAfter = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);

            Assert.GreaterOrEqual(machAfter.LastPlayed, machBefore.LastPlayed);
        }

        [Test]
        public async Task WinningPlay()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            var machBefore = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);

            int betAmount = GameSimulator.gameplayParams.BetAmount;
            int balanceStart = GameSimulator.gameplayParams.BalanceCredits;
            int winAmount = 0;
            GameSimulator.Win(out winAmount);
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var machAfter = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            var balanceAfter = machAfter.Balance * 100;
            int expectedBalance = (balanceStart - betAmount) + winAmount;

            Assert.GreaterOrEqual(machAfter.LastPlayed, machBefore.LastPlayed);
            Assert.AreEqual('W', machAfter.TransType);
            Assert.AreEqual(expectedBalance,balanceAfter);

        }

        [Test]
        public async Task LosingPlay()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            var machBefore = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);

            int betAmount = GameSimulator.gameplayParams.BetAmount;
            int balanceStart = GameSimulator.gameplayParams.BalanceCredits;

            GameSimulator.Loss();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var machAfter = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            var balanceAfter = machAfter.Balance * 100;
            int expectedBalance = (balanceStart - betAmount);

            Assert.GreaterOrEqual(machAfter.LastPlayed, machBefore.LastPlayed);
            Assert.AreEqual('L', machAfter.TransType);
            Assert.AreEqual(expectedBalance, balanceAfter);
        }

        [Test]
        public async Task BillIn()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            int betAmount = GameSimulator.gameplayParams.BetAmount;

            var machBefore = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            var balanceStart = machBefore.Balance * 100;

            GameSimulator.BillIn(1);
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var machAfter = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            var balanceAfter = machAfter.Balance * 100;

            var expectedBalance = (balanceStart + 100);

            Assert.AreEqual('M', machAfter.TransType);
            Assert.AreEqual(expectedBalance, balanceAfter);
        }

        [Test]
        public async Task Cashout()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            var machBefore = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            var balanceStart = machBefore.Balance * 100;

            GameSimulator.CashOut();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var machAfter = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            var balanceAfter = machAfter.Balance * 100;

            var expectedBalance = 0;

            Assert.AreEqual('V', machAfter.TransType);
            Assert.AreEqual(expectedBalance, balanceAfter);
        }

        [Test]
        public async Task SetOffline()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            var machBefore = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            Assert.True(machBefore.Status);

            GameSimulator.SetOffline();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var machAfter = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            Assert.False(machAfter.Status);
        }

        [Test]
        public async Task SetOnline()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            var machBefore = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            Assert.True(machBefore.Status);

            GameSimulator.SetOffline();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var machAfter = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            Assert.False(machAfter.Status);

            GameSimulator.SetOnline();
            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            var machFinal = _devicePage.GetMachineByMachNo(TestData.DefaultMachineNumber);
            Assert.True(machFinal.Status);
        }
    }
}