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
    public class PayoutTests : BaseTest
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

        //Verify an error is displayed to the user if the scanned barcode is already in the transaction
        [Test]
        public void PayoutTest_VoucherAlreadyInTransaction()
        {

            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);
            Assert.AreEqual(1, _payoutPage.CurrentTransactionList.VoucherCount);

            _payoutPage.NumPad.EnterBarcode(barcode);
            Assert.True(_payoutPage.PayoutError.IsOpen);
        }

        //Verify only 1 jackpot voucher can be in a transaction
        [Test]
        public void PayoutTest_JackpotVoucherMultiple()
        {

            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;

            var jackpot1 = TpService.GetVoucher(StartingAmountCredits,500,true);
            StartingAmountCredits -= 500;

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);

            Assert.False(_payoutPage.PayoutError.IsOpen);

            _payoutPage.NumPad.EnterBarcode(jackpot1);
            Assert.True(_payoutPage.PayoutError.IsOpen);
        }

        //Verify error is displayed if current transaction contains a jackpot voucher and you try to add a regular voucher to the transaction
        [Test]
        public void PayoutTest_AddJackpotToTransaction()
        {

            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;

            var jackpot1 = TpService.GetVoucher(StartingAmountCredits, 500, true);
            StartingAmountCredits -= 500;

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(jackpot1);

            Assert.False(_payoutPage.PayoutError.IsOpen);

            _payoutPage.NumPad.EnterBarcode(barcode);
            Assert.True(_payoutPage.PayoutError.IsOpen);
        }

        [Test]
        public void PayoutTest_MultipleJackptVouchers()
        {

            var jackpot1 = TpService.GetVoucher(StartingAmountCredits, 500, true);
            StartingAmountCredits -= 500;

            var jackpot2 = TpService.GetVoucher(StartingAmountCredits, 500, true);
            StartingAmountCredits -= 500;

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(jackpot1);

            Assert.False(_payoutPage.PayoutError.IsOpen);

            _payoutPage.NumPad.EnterBarcode(jackpot2);
            Assert.True(_payoutPage.PayoutError.IsOpen);
        }

        //Verify a transaction cannot have more than 25 vouchers
        [Test]
        public void PayoutTest_MaxVouchers_ForTransaction()
        {
            var barcodes = new List<string>();

            for(int i = 0; i < 25; i++)
            {
                var barcode = TpService.GetVoucher(StartingAmountCredits, 19);
                StartingAmountCredits -= 19;

                barcodes.Add(barcode);
            }

            var lastBarcode = TpService.GetVoucher(StartingAmountCredits, 19);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            foreach(var barcode in barcodes)
            {
                _payoutPage.NumPad.EnterBarcode(barcode);
            }

            Assert.AreEqual(25,_payoutPage.CurrentTransactionList.VoucherCount);

            _payoutPage.NumPad.EnterBarcode(lastBarcode);

            Assert.True(_payoutPage.PayoutError.IsOpen);
        }

        //Verify error is displayeded if the voucher is expired
        [Test]
        public void PayoutTest_ExpiredVoucher()
        {
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);
            _transRepo.SetVoucherExpired(barcode);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);

            Assert.True(_payoutPage.PayoutError.IsOpen);
        }

        //Verify an error is displayed if the voucher is not found in the database
        [Test]
        public void PayoutTest_VoucherNotFound()
        {
            var barcode = new string('1', 18);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);

            Assert.False(_payoutPage.PayoutError.IsOpen);
            Assert.Zero(_payoutPage.CurrentTransactionList.VoucherCount);
        }

        //User cannot payout if cash drawer is not connected
        [Test]
        public void PayoutTest_NoCashDrawer()
        {
            _transRepo.SetAutoCashDrawerFlag(false);
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);


            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            Assert.True(_cashDrawer.CashDrawerConnectedPrompt.IsOpen);
            _cashDrawer.CashDrawerConnectedPrompt.Cancel();

            _payoutPage.NumPad.EnterBarcode(barcode);
            _payoutPage.ClickPayout();

            Assert.True(_payoutPage.PayoutConfirmationAlert.IsOpen);
        }

        //Verify error is displayed if payout total is greater than current balance in cash drawer
        [Test]
        public void PayoutTest_InsufficientCashDrawerBalance()
        {

            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;

            var barcode2 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;

            var barcode3 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;

            var barcodes = new List<string>() { barcode1, barcode2, barcode3 };

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 10;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            foreach(var barcode in barcodes)
            {
                _payoutPage.NumPad.EnterBarcode(barcode);
            }

            _payoutPage.ClickPayout();

            Assert.False(_payoutPage.PayoutConfirmationAlert.IsOpen);
        }

        [Test]
        public void PayoutTest_VoucherAlreadyRedeemed()
        {

            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;

            var barcode2 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;

            var barcode3 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;

            var barcodes = new List<string>() { barcode1, barcode2, barcode3 };

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 15;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            foreach (var barcode in barcodes)
            {
                _payoutPage.NumPad.EnterBarcode(barcode);
            }

            _transRepo.SetVoucherRedeemedState(barcode2,true);

            _payoutPage.ClickPayout();

            Assert.True(_payoutPage.PayoutError.IsOpen);
        }

        //User is prompted for confirmation after pressing Cancel Transaction button
        [Test]
        public void PayoutTest_CancelTransaction_Prompt()
        {

            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);

            _payoutPage.ClickCancelTransaction();
            string alertText = _payoutPage.CancelTransactionPrompt.AlertText;
            string expectedText = "Are you sure you want to cancel the transaction?  This will remove all vouchers from the transaction.";

            Assert.True(_payoutPage.CancelTransactionPrompt.IsOpen);
            Assert.AreEqual(expectedText, alertText);
        }

        //Verify the transaction list is cleared and Voucher Count and Total Payout are reset after cancelling transaction
        [Test]
        public void PayoutTest_CancelTransaction()
        {

            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);

            int countBefore = _payoutPage.CurrentTransactionList.VoucherCount;
            decimal payoutBefore = _payoutPage.CurrentTransactionList.TotalPayout;

            Assert.AreEqual(1,countBefore);
            Assert.AreEqual(5,payoutBefore);

            _payoutPage.CancelTransaction();

            int countAfter = _payoutPage.CurrentTransactionList.VoucherCount;
            decimal payoutAfter = _payoutPage.CurrentTransactionList.TotalPayout;

            Assert.Zero(countAfter);
            Assert.Zero(payoutAfter);
        }

        [Test]
        public void PayoutTest_CancelTransaction_MultipleVouchers()
        {

            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;

            var barcode2 = TpService.GetVoucher(StartingAmountCredits, 1000);
            StartingAmountCredits -= 1000;

            var barcode3 = TpService.GetVoucher(StartingAmountCredits, 1000);
            StartingAmountCredits -= 1000;

            var barcodes = new List<String>() { barcode1, barcode2, barcode3 };

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            foreach(var barcode in barcodes)
            {
                _payoutPage.NumPad.EnterBarcode(barcode);
            }

            int countBefore = _payoutPage.CurrentTransactionList.VoucherCount;
            decimal payoutBefore = _payoutPage.CurrentTransactionList.TotalPayout;

            Assert.AreEqual(3, countBefore);
            Assert.AreEqual(25, payoutBefore);

            _payoutPage.CancelTransaction();

            int countAfter = _payoutPage.CurrentTransactionList.VoucherCount;
            decimal payoutAfter = _payoutPage.CurrentTransactionList.TotalPayout;

            Assert.Zero(countAfter);
            Assert.Zero(payoutAfter);
        }
    }
}