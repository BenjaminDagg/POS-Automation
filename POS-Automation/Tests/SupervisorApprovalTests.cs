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
    public class SupervisorApprovalTests : BaseTest
    {
        private LoginPage _loginPage;
        private VoucherNumPad _numPad;
        private PayoutPage _payoutPage;
        private CashDrawer _cashDrawer;
        private TransactionPortalService TpService;
        private DatabaseManager _databaseManager;
        private VoucherTransactionRepository _transRepo;
        private decimal StartingAmountDollar = 1000.00m;
        private int StartingAmountCredits = 100000;

        [SetUp]
        public void Setup()
        {
            _loginPage = new LoginPage(driver);
            _numPad = new VoucherNumPad(driver);
            _payoutPage = new PayoutPage(driver);
            _cashDrawer = new CashDrawer(driver);

            _transRepo = new VoucherTransactionRepository();

            _databaseManager = new DatabaseManager();
            _databaseManager.ResetTestMachine(StartingAmountDollar);

            TpService = new TransactionPortalService(_logService);
            TpService.Connect();
        }

        [TearDown]
        public void TearDown()
        {
            TpService.Disconnect();

            _transRepo.SetAutoCashDrawerFlag(true);

            StartingAmountDollar = 1000.00m;
            StartingAmountCredits = 100000;
        }

        
        //Verify if scaneed barcode is a jackpot voucher and the amount is over the LOCKUP_AMOUNT, then voucher requires approval
        [Test]
        public void PayoutTest_RequiresApproval_Jackpot()
        {

            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 49900, true);
            StartingAmountCredits -= 49900;
            Console.WriteLine(barcode1);
            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 10001;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode1);

            var barcodes = _payoutPage.CurrentTransactionList.GetVouchers();

            Assert.True(barcodes[0].NeedsApproval);
        }

        [Test]
        public void PayoutTest_SupervisorApproval_EmptyUsername()
        {

            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 49900, true);
            StartingAmountCredits -= 49900;

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 10001;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode1);

            var barcodes = _payoutPage.CurrentTransactionList.GetVouchers();

            Assert.True(barcodes[0].NeedsApproval);

            _payoutPage.ClickPayout();
            _payoutPage.SupervisorApprovalPrompt.EnterPassword(TestData.SupervisorPassword);
            _payoutPage.SupervisorApprovalPrompt.Confirm();

            Assert.True(_payoutPage.SupervisorApprovalPrompt.UsernameErrorIsDisplayed());
        }

        [Test]
        public void PayoutTest_SupervisorApproval_EmptyPassword()
        {

            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 49900, true);
            StartingAmountCredits -= 49900;

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 10001;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode1);

            var barcodes = _payoutPage.CurrentTransactionList.GetVouchers();

            Assert.True(barcodes[0].NeedsApproval);

            _payoutPage.ClickPayout();
            _payoutPage.SupervisorApprovalPrompt.EnterUsername(TestData.SupervisorUsername);
            _payoutPage.SupervisorApprovalPrompt.EnterPassword("");
            _payoutPage.SupervisorApprovalPrompt.Confirm();

            Assert.True(_payoutPage.SupervisorApprovalPrompt.PasswordErrorIsDisplayed());
        }

        [Test]
        public void PayoutTest_SupervisorApproval_IncorrectPassword()
        {

            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 49900, true);
            StartingAmountCredits -= 49900;

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 10001;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode1);

            var barcodes = _payoutPage.CurrentTransactionList.GetVouchers();

            Assert.True(barcodes[0].NeedsApproval);

            _payoutPage.ClickPayout();
            _payoutPage.SupervisorApprovalPrompt.EnterUsername(TestData.SupervisorUsername);
            _payoutPage.SupervisorApprovalPrompt.EnterPassword("Diamond9!");
            _payoutPage.SupervisorApprovalPrompt.Confirm();

            Assert.True(_payoutPage.SupervisorApprovalPrompt.IsOpen);
        }

        //Verify only an admin or supervisor user can approve and an error is displayed if a cashier user credentials are entered
        [Test]
        public void PayoutTest_SupervisorApproval_WrongPermission()
        {

            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 49900, true);
            StartingAmountCredits -= 49900;

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 10001;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode1);

            var barcodes = _payoutPage.CurrentTransactionList.GetVouchers();

            Assert.True(barcodes[0].NeedsApproval);

            _payoutPage.ClickPayout();
            _payoutPage.SupervisorApprovalPrompt.EnterUsername(TestData.CashierUsername2);
            _payoutPage.SupervisorApprovalPrompt.EnterPassword(TestData.CashierPassword2);
            _payoutPage.SupervisorApprovalPrompt.Confirm();

            Assert.True(_payoutPage.SupervisorApprovalPrompt.IsOpen);
        }

        [Test]
        public void PayoutTest_SupervisorApproval_Success_AdminUser()
        {

            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 49900, true);
            StartingAmountCredits -= 49900;

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 10001;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode1);

            var barcodes = _payoutPage.CurrentTransactionList.GetVouchers();

            Assert.True(barcodes[0].NeedsApproval);

            _payoutPage.ClickPayout();
            _payoutPage.SupervisorApprovalPrompt.EnterUsername(TestData.AdminUsername);
            _payoutPage.SupervisorApprovalPrompt.EnterPassword(TestData.AdminPassword);
            _payoutPage.SupervisorApprovalPrompt.Confirm();

            Assert.True(_payoutPage.PayoutConfirmationAlert.IsOpen);
        }

        [Test]
        public void PayoutTest_SupervisorApproval_Success_SupervisorUser()
        {

            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 49900, true);
            StartingAmountCredits -= 49900;

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 10001;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode1);

            var barcodes = _payoutPage.CurrentTransactionList.GetVouchers();

            Assert.True(barcodes[0].NeedsApproval);

            _payoutPage.ClickPayout();
            _payoutPage.SupervisorApprovalPrompt.EnterUsername(TestData.SupervisorUsername);
            _payoutPage.SupervisorApprovalPrompt.EnterPassword(TestData.SupervisorPassword);
            _payoutPage.SupervisorApprovalPrompt.Confirm();

            Assert.True(_payoutPage.PayoutConfirmationAlert.IsOpen);
        }

        [Test]
        public void PayoutTest_SupervisorApproval_Cancel()
        {

            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 49900, true);
            StartingAmountCredits -= 49900;

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 10001;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode1);

            var barcodes = _payoutPage.CurrentTransactionList.GetVouchers();

            Assert.True(barcodes[0].NeedsApproval);

            _payoutPage.ClickPayout();
            _payoutPage.SupervisorApprovalPrompt.EnterUsername(TestData.AdminUsername);
            _payoutPage.SupervisorApprovalPrompt.EnterPassword(TestData.AdminPassword);
            _payoutPage.SupervisorApprovalPrompt.Cancel();

            Assert.False(_payoutPage.SupervisorApprovalPrompt.IsOpen);
            Assert.AreEqual(1, _payoutPage.CurrentTransactionList.VoucherCount);
        }
    }
}