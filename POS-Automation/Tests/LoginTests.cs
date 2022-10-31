using NUnit.Framework;
using System;
using System.Threading;
using POS_Automation.Model;
using System.Threading.Tasks;

namespace POS_Automation
{
    public class LoginTests : BaseTest
    {
        private LoginPage _loginPage;
        private DeviceManagementPage _deviceManagementPage;
        private GameSimulator GameSimulator;

        [SetUp]
        public async Task Setup()
        {
            _loginPage = new LoginPage(driver);
            _deviceManagementPage = new DeviceManagementPage(driver);
            Console.WriteLine("in setup of login");
            DatabaseManager.ResetTestMachine();
            GameSimulator = new GameSimulator(_logService);
            await GameSimulator.StartUp();
        }

        [TearDown]
        public async Task TearDown()
        {
            await GameSimulator.ShutDown();
            DatabaseManager.ResetTestMachine();
        }

        [Test]
        public void Test1()
        {
            Thread.Sleep(5000);
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            //_loginPage.NavigationTabs.ClickDeviceTab();
            Thread.Sleep(3000);
        }

        [Test]
        public void Test2()
        {
            
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();
            Thread.Sleep(10000);
            
        }
    }
}