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
        private GameSimulator GameSimulator;

        [SetUp]
        public void Setup()
        {
            _loginPage = new LoginPage(driver);
   
        }

        [TearDown]
        public void TearDown()
        {
            
        }

        [Test]
        public void SuccesfullLogin()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();
            var devicePage = new DeviceManagementPage(driver);
            devicePage.DisplayDeviceList();
            //driver.FindElement(devicePage.LogoutButton).Click();
        }

        
    }
}