using NUnit.Framework;
using System;
using System.Threading;
using POS_Automation.Model;
using System.Threading.Tasks;
using POS_Automation.Pages;

namespace POS_Automation
{
    public class SettingsTests : BaseTest
    {
        private LoginPage _loginPage;
        private SettingsPage _settingsPage;

        [SetUp]
        public void Setup()
        {
            _loginPage = new LoginPage(driver);
            _settingsPage = new SettingsPage(driver);
        }

        [TearDown]
        public void TearDown()
        {

        }

        [Test]
        public void PrinterOptions()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickSetingsTab();

            var items = _settingsPage.PrinterDropdown.Options;
            Assert.Greater(items.Count, 0);
        }

        [Test]
        public void PollingIntervalEmpty()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickSetingsTab();

            Assert.False(_settingsPage.ErrorIsDisplayed(_settingsPage.PollIntervalField));

            _settingsPage.EnterPollInterval("");
            _settingsPage.SaveDeviceSettings();

            Assert.True(_settingsPage.ErrorIsDisplayed(_settingsPage.PollIntervalField));
        }

        [Test]
        public void PollingIntervalMinValue()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickSetingsTab();

            Assert.False(_settingsPage.ErrorIsDisplayed(_settingsPage.PollIntervalField));

            _settingsPage.EnterPollInterval("4");
            _settingsPage.SaveDeviceSettings();

            Assert.True(_settingsPage.ErrorIsDisplayed(_settingsPage.PollIntervalField));
        }

        [Test]
        public void PollingIntervalMaxValue()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickSetingsTab();

            Assert.False(_settingsPage.ErrorIsDisplayed(_settingsPage.PollIntervalField));

            _settingsPage.EnterPollInterval("121");
            _settingsPage.SaveDeviceSettings();

            Assert.True(_settingsPage.ErrorIsDisplayed(_settingsPage.PollIntervalField));
        }


        [Test]
        public void PollingIntervalValid()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickSetingsTab();

            int currentValue = _settingsPage.GetPollingInterval();
            int expectedValue = currentValue + 1;

            _settingsPage.EnterPollInterval(expectedValue.ToString());
            _settingsPage.SaveDeviceSettings();

            Assert.False(_settingsPage.ErrorIsDisplayed(_settingsPage.PollIntervalField));

            int actual = _settingsPage.GetPollingInterval();
            Assert.AreEqual(expectedValue, actual);
        }

        [Test]
        public void ServerEmpty()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickSetingsTab();

            Assert.False(_settingsPage.ErrorIsDisplayed(_settingsPage.IpAddressField));

            _settingsPage.EnterIpAddress("");
            _settingsPage.SaveDeviceSettings();

            Assert.True(_settingsPage.ErrorIsDisplayed(_settingsPage.IpAddressField));
        }

        [TestCase("serverabc")]
        [TestCase("1.1.1.256")]
        public void InvalidIp(string ip)
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickSetingsTab();

            Assert.False(_settingsPage.ErrorIsDisplayed(_settingsPage.IpAddressField));

            _settingsPage.EnterIpAddress(ip);
            _settingsPage.SaveDeviceSettings();

            Assert.True(_settingsPage.ErrorIsDisplayed(_settingsPage.IpAddressField));
        }

        [Test]
        public void PortEmpty()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickSetingsTab();

            Assert.False(_settingsPage.ErrorIsDisplayed(_settingsPage.PortNumberField));

            _settingsPage.EnterPortNumber("");
            _settingsPage.SaveDeviceSettings();

            Assert.True(_settingsPage.ErrorIsDisplayed(_settingsPage.PortNumberField));
        }

        [TestCase(4499)]
        [TestCase(5001)]
        [TestCase(123)]
        [TestCase(50001)]
        public void PortRange(int port)
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickSetingsTab();

            Assert.False(_settingsPage.ErrorIsDisplayed(_settingsPage.PortNumberField));

            _settingsPage.EnterPortNumber(port.ToString());
            _settingsPage.SaveDeviceSettings();

            Assert.True(_settingsPage.ErrorIsDisplayed(_settingsPage.PortNumberField));
        }
    }
}