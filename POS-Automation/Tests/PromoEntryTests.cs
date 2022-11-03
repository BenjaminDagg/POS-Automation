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
        private TransactionPortalClient GameClient;
        private TransactionPortalClient PosClient;
        private PromoTicketRepository promoTicketRepository;

        [SetUp]
        public void Setup()
        {
            _loginPage = new LoginPage(driver);
            _devicePage = new DeviceManagementPage(driver);
            _settingsPage = new SettingsPage(driver);

            promoTicketRepository = new PromoTicketRepository();

            DatabaseManager.ResetTestMachine();

            GameClient = new TransactionPortalClient(TestData.TransactionPortalIpAddress, 4550);
            GameClient.Connect();

            PosClient = new TransactionPortalClient(TestData.TransactionPortalIpAddress, 4551);
            PosClient.Connect();

            //GameClient.Listen().Wait();
        }

        [TearDown]
        public void TearDown()
        {
            //DatabaseManager.ResetTestMachine();
            GameClient.CLose();
            PosClient.CLose();
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
        public void PromoEnabledDbEntry()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            _devicePage.TurnPromoTicketsOn();
            bool isEnabled = promoTicketRepository.PrintPromoTicketsEnabled();

            _devicePage.TurnPromoTicketsOff();

            Assert.True(isEnabled);
        }

        [Test]
        public void PromoDisabledDbEntry()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            _devicePage.TurnPromoTicketsOff();
            bool isEnabled = promoTicketRepository.PrintPromoTicketsEnabled();

            Assert.False(isEnabled);
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
        public async Task GetAllMachines()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            string res = PosClient.Execute($"3,Z,{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},GetAllMachines");
            

            Assert.True(res.Contains("GetAllMachines"));
            Assert.True(res.Contains(TestData.DefaultMachineNumber));
            Assert.True(res.Contains(TestData.DefaultIPAddress));
        }

        
        //Response sent to game after disabling promo
        [Test]
        public void EnablePromoTpMessage()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            bool isEnabled = _devicePage.PromoTicketsAreEnabled();
            if (isEnabled)
            {
                _devicePage.TurnPromoTicketsOff();
                GameClient.Read();
            }
            GameClient.Read();
            _devicePage.TurnPromoTicketsOn();

            var response = GameClient.Read();
            Console.WriteLine(response);
            Assert.True(response.Contains("EntryTicketOn"));

            _devicePage.TurnPromoTicketsOff();
        }

        //Verify the response sent back to the POS client after sending request to turn off promo
        [Test]
        public void EnablePromoTpcResponseMessage()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            var response = PosClient.Execute($"3,Z,{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},EntryTicketOn");
            Assert.True(response.Contains(",0,,0,EntryTicketOn"));
        }

        //Verify response sent to machine after turning off promo
        [Test]
        public void DisablePromoTpMessage()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            bool isEnabled = _devicePage.PromoTicketsAreEnabled();
            if (!isEnabled)
            {
                _devicePage.TurnPromoTicketsOn();
                GameClient.Read();
            }

            _devicePage.TurnPromoTicketsOff();

            var response = GameClient.Read();
            Console.Write(response);
            Assert.True(response.Contains("EntryTicketOff"));

            _devicePage.TurnPromoTicketsOff();
        }

        //Verify the response sent back to the POS client after sending request to turn off promo
        [Test]
        public void DisablePromoTpcResponseMessage()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            var response = PosClient.Execute($"3,Z,{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},EntryTicketOff");
            Assert.True(response.Contains(",0,,0,EntryTicketOff"));
        }
    }
}