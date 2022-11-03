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
    public class PromoEntryTests : BaseTest
    {
        private LoginPage _loginPage;
        private DeviceManagementPage _devicePage;
        private SettingsPage _settingsPage;
        private TransactionPortalClient tcpClient;

        [SetUp]
        public void Setup()
        {
            _loginPage = new LoginPage(driver);
            _devicePage = new DeviceManagementPage(driver);
            _settingsPage = new SettingsPage(driver);

            //DatabaseManager.ResetTestMachine();

            tcpClient = new TransactionPortalClient(TestData.TransactionPortalIpAddress, 4550);
            tcpClient.Connect();
        }

        [TearDown]
        public void TearDown()
        {
            //DatabaseManager.ResetTestMachine();
            tcpClient.CLose();
        }

        [Test]
        public void PromoToggleButtonTextDisabled()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            _devicePage.TurnPromoTicketsOff();
            string btnText = _devicePage.PromoToggleButtonText();

            Assert.AreEqual("Turn Promo Ticket On", btnText);
        }

        [Test]
        public void PromoToggleButtonTextEnabled()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            _devicePage.TurnPromoTicketsOn();
            string btnText = _devicePage.PromoToggleButtonText();

            Assert.AreEqual("Turn Promo Ticket Off", btnText);

            _devicePage.TurnPromoTicketsOff();
        }

        [Test]
        public void EnablePromoTicketPrompt()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            _devicePage.TurnPromoTicketsOff();
            _devicePage.ClickTogglePromoButton();

            Assert.True(_devicePage.Alert.IsOpen);
            string alertText = _devicePage.Alert.AlertText;

            Assert.AreEqual("Are you sure you want to turn the Promo Ticket Printing on?", alertText);
        }

        [Test]
        public void DisablePromoTicketPrompt()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            _devicePage.TurnPromoTicketsOn();
            _devicePage.ClickTogglePromoButton();

            Assert.True(_devicePage.Alert.IsOpen);
            string alertText = _devicePage.Alert.AlertText;

            Assert.AreEqual("Are you sure you want to turn the Promo Ticket Printing off?", alertText);

            _devicePage.Alert.Confirm();
        }

        [Test]
        public void EnablePromoTicketsCancel()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            bool enabledBefore = _devicePage.PromoTicketsAreEnabled();

            _devicePage.ClickTogglePromoButton();
            _devicePage.Alert.Cancel();

            bool enabledAfter = _devicePage.PromoTicketsAreEnabled();

            Assert.AreEqual(enabledBefore, enabledAfter);
        }

        [Test]
        public void EnablePromoTpMessage()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            bool isEnabled = _devicePage.PromoTicketsAreEnabled();
            if (isEnabled)
            {
                _devicePage.TurnPromoTicketsOff();
                tcpClient.Read();
            }
            tcpClient.Read();
            _devicePage.TurnPromoTicketsOn();

            var response = tcpClient.Read();
            Console.WriteLine(response);
            Assert.True(response.Contains("EntryTicketOn"));

            _devicePage.TurnPromoTicketsOff();
        }

        [Test]
        public void DisablePromoTpMessage()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            bool isEnabled = _devicePage.PromoTicketsAreEnabled();
            if (!isEnabled)
            {
                _devicePage.TurnPromoTicketsOn();
                tcpClient.Read();
            }

            _devicePage.TurnPromoTicketsOff();

            var response = tcpClient.Read();
            Console.Write(response);
            Assert.True(response.Contains("EntryTicketOff"));

            _devicePage.TurnPromoTicketsOff();
        }
    }
}