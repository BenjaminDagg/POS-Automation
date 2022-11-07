using NUnit.Framework;
using System;
using System.Threading;
using POS_Automation.Model;
using System.Threading.Tasks;
using POS_Automation.Pages.Payout;
using System.Collections.Generic;
using OpenQA.Selenium.Appium.Windows;
using System.Linq;
using POS_Automation.Model.Payout;

namespace POS_Automation
{
    public class UserPermissionTest : BaseTest
    {
        public UserPermissionTest() : base()
        {

        }

        private LoginPage _loginPage;

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
        public void AdminUserMenu()
        {
            _loginPage.Login(TestData.AdminUsername,TestData.AdminPassword);

            Assert.True(NavigationTabs.SettingsIsVisible());
            Assert.True(NavigationTabs.DeviceManagementIsVisible());
            Assert.True(NavigationTabs.ReportIsVisible());
            Assert.False(NavigationTabs.PayoutIsVisible());
        }

        [Test]
        public void CashierUserMenu()
        {
            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);

            Assert.False(NavigationTabs.SettingsIsVisible());
            Assert.True(NavigationTabs.DeviceManagementIsVisible());
            Assert.False(NavigationTabs.ReportIsVisible());
            Assert.True(NavigationTabs.PayoutIsVisible());
        }

        [Test]
        public void SupervisorUserMenu()
        {
            _loginPage.Login(TestData.SupervisorUsername, TestData.SupervisorPassword);

            Assert.False(NavigationTabs.SettingsIsVisible());
            Assert.True(NavigationTabs.DeviceManagementIsVisible());
            Assert.True(NavigationTabs.ReportIsVisible());
            Assert.False(NavigationTabs.PayoutIsVisible());
        }
    }
}
