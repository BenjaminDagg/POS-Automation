using NUnit.Framework;
using System;
using System.Threading;

namespace POS_Automation
{
    public class LoginTests : BaseTest
    {
        private LoginPage _loginPage;
        private DeviceManagementPage _deviceManagementPage;

        [SetUp]
        public void Setup()
        {
            _loginPage = new LoginPage(driver);
            _deviceManagementPage = new DeviceManagementPage(driver);
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
            NavigationTabs.ClickSetingsTab();
            Thread.Sleep(1000);
            
        }
    }
}