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
    public class CashDrawerTest : BaseTest
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

        

        

        [Test]
        public void AutoCashDrawerDisabled_Prompt()
        {
            _transRepo.SetAutoCashDrawerFlag(false);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            Assert.True(_cashDrawer.CashDrawerConnectedPrompt.IsOpen);
        }

        //User should not be taken to payout page if invalid starting balance is entered
        [Test]
        public void StartingBalanceZero()
        {
            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            Assert.True(_payoutPage.CashDrawer.StartingBalancePrompt.IsOpen);

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput("0");
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            Assert.True(_payoutPage.CashDrawer.StartingBalancePrompt.IsOpen);
        }

        //If valid starting balance is entered, go to payout page and verify cash drawer has correct balance
        [Test]
        public void StartingBalanceValid()
        {
            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            Assert.True(_payoutPage.CashDrawer.StartingBalancePrompt.IsOpen);

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput("1000");
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var actualBalance = _payoutPage.CashDrawer.StartingBalance;

            Assert.AreEqual(1000, actualBalance);
        }

        //If user cancels starting balance return to starting screen
        [Test]
        public void StartingBalance_CancelPrompt()
        {
            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            Assert.True(_payoutPage.CashDrawer.StartingBalancePrompt.IsOpen);

            _payoutPage.CashDrawer.StartingBalancePrompt.Cancel();

            Assert.True(_payoutPage.CashDrawer.IsHidden);
            Assert.True(_payoutPage.CashDrawer.StartingBalanceErrorAlert.IsOpen);
        }

        //Cash drawer should be hidden if user selects 'No' on cash drawer prompt
        [Test]
        public void CashDrawerPrompt_Hidden()
        {
            _transRepo.SetAutoCashDrawerFlag(false);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            Assert.True(_cashDrawer.CashDrawerConnectedPrompt.IsOpen);
            _cashDrawer.CashDrawerConnectedPrompt.Cancel();

            Assert.True(_payoutPage.CashDrawer.IsHidden);
        }

        //cash drawer should not be hidden if user selects 'Yes' on cash drawer prompt
        [Test]
        public void CashDrawerPrompt_Hidden_False()
        {
            _transRepo.SetAutoCashDrawerFlag(false);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            Assert.True(_cashDrawer.CashDrawerConnectedPrompt.IsOpen);

            _cashDrawer.CashDrawerConnectedPrompt.Confirm();
            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput("1000");
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            Assert.False(_payoutPage.CashDrawer.IsHidden);
        }

        //Verify starting balance field gets set to the value the user entered
        [Test]
        public void CashDrawerPrompt_StartingBalance()
        {

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput("12345");
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var actualStartingBalance = _payoutPage.CashDrawer.StartingBalance;

            Assert.AreEqual(12345, actualStartingBalance);
        }

        //Verify before any transactions occur, the current balance is equal to the starting balance
        [Test]
        public void CashDrawerPrompt_InitialCurrentBalance()
        {

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput("12345");
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var actualCurrentBalance = _payoutPage.CashDrawer.CurrentBalance;

            Assert.AreEqual(12345, actualCurrentBalance);
        }

        //Verify after adding cash the current balance increments
        [Test]
        public void CashDrawerPrompt_CurrentBalance_AddCash()
        {

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.CashDrawer.AddCash("100", TestData.CashierPassword);

            var currentBalance = _payoutPage.CashDrawer.CurrentBalance;
            var expectedBalance = startingBalance + 100;

            Assert.AreEqual(expectedBalance, currentBalance);
        }

        //Verify current balance decreases after removing cash
        [Test]
        public void CashDrawerPrompt_CurrentBalance_RemoveCash()
        {

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.CashDrawer.RemoveCash("100", TestData.CashierPassword);

            var currentBalance = _payoutPage.CashDrawer.CurrentBalance;
            var expectedBalance = startingBalance - 100;

            Assert.AreEqual(expectedBalance, currentBalance);
        }

        //Verify after payout of a voucher the current balance decreases by the voucher amount
        [Test]
        public void CashDrawerPrompt_CurrentBalance_Payout()
        {
            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 10000);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);
            _payoutPage.Payout();

            int expectedBalance = startingBalance - 100;

            var balance = _payoutPage.CashDrawer.CurrentBalance;
            Assert.AreEqual(expectedBalance,balance);
        }

        //Verify cash added field increments after adding cash
        [Test]
        public void CashDrawerPrompt_CashAdded()
        {

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var cashAddedCountBefore = _payoutPage.CashDrawer.CashAdded;

            _payoutPage.CashDrawer.AddCash("100", TestData.CashierPassword);

            var cashAddedCountAfter = _payoutPage.CashDrawer.CashAdded;
            var expectedAddedCash = 100;

            Assert.AreEqual(expectedAddedCash, cashAddedCountAfter);
        }

        //Verify cash removed field gets incremented after removing cash
        [Test]
        public void CashDrawerPrompt_CashRemoved()
        {

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.CashDrawer.RemoveCash("100", TestData.CashierPassword);

            var cashRemovedCountAfter = _payoutPage.CashDrawer.CashRemoved;
            var expectedCashRemoved = 100;

            Assert.AreEqual(expectedCashRemoved, cashRemovedCountAfter);
        }

        [Test]
        public void CashDrawerPrompt_TotalPayout_SingleTransaction()
        {
            //create a voucher for $100
            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 10000);
            StartingAmountCredits -= 10000;

            var barcode2= TpService.GetVoucher(StartingAmountCredits, 10000);
            StartingAmountCredits -= 10000;

            var barcode3 = TpService.GetVoucher(StartingAmountCredits, 10000);

            var barcodes = new List<string>() { barcode1, barcode2, barcode3 };
       
            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            foreach(var barcode in barcodes)
            {
                _payoutPage.NumPad.EnterBarcode(barcode);
                
            }
            _payoutPage.Payout();

            int expectedBalance = startingBalance - 300;
            int expectedPayout = 300;

            var balance = _payoutPage.CashDrawer.CurrentBalance;
            var totalPayout = _payoutPage.CashDrawer.TotalPayout;

            Assert.AreEqual(expectedBalance, balance);
            Assert.AreEqual(expectedPayout, totalPayout);
        }

        [Test]
        public void CashDrawerPrompt_TotalPayout_MultipleTransaction()
        {
            //create a voucher for $100
            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 10000);
            StartingAmountCredits -= 10000;

            var barcode2 = TpService.GetVoucher(StartingAmountCredits, 10000);
            StartingAmountCredits -= 10000;

            var barcode3 = TpService.GetVoucher(StartingAmountCredits, 10000);

            var barcodes = new List<string>() { barcode1, barcode2, barcode3 };

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            foreach (var barcode in barcodes)
            {
                _payoutPage.NumPad.EnterBarcode(barcode);
                _payoutPage.Payout();
            }
            

            int expectedBalance = startingBalance - 300;
            int expectedPayout = 300;

            var balance = _payoutPage.CashDrawer.CurrentBalance;
            var payout = _payoutPage.CashDrawer.TotalPayout;

            Assert.AreEqual(expectedBalance, balance);
            Assert.AreEqual(expectedPayout, payout);
        }

        //Verify balance doesn't change if the payout is cancelled
        [Test]
        public void CashDrawerPrompt_Cancel_Payout()
        {
            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 10000);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);
            _payoutPage.CancelTransaction();


            int expectedBalance = startingBalance;

            var balance = _payoutPage.CashDrawer.CurrentBalance;
            Assert.AreEqual(expectedBalance, balance);
        }

        //Verify an error is displayed if the incorrect password is entered on Add Cash Prompt
        [Test]
        public void CashDrawerPrompt_AddCash_IncorrectPassword()
        {

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.CashDrawer.ClickAddCash();

            Assert.False(_payoutPage.ErrorIsDisplayed(_payoutPage.CashDrawer.AddCashPrompt.PasswordTextbox));

            _payoutPage.CashDrawer.AddCashPrompt.EnterAmount("100");
            _payoutPage.CashDrawer.AddCashPrompt.EnterPassword("Diamond1!");
            _payoutPage.CashDrawer.AddCashPrompt.Confirm();

            Assert.True(_payoutPage.ErrorIsDisplayed(_payoutPage.CashDrawer.AddCashPrompt.PasswordTextbox));
        }

        //verify error is displayed if password is left blank on add cash prompt
        [Test]
        public void CashDrawerPrompt_AddCash_EmptyPassword()
        {

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.CashDrawer.ClickAddCash();

            Assert.False(_payoutPage.ErrorIsDisplayed(_payoutPage.CashDrawer.AddCashPrompt.PasswordTextbox));

            _payoutPage.CashDrawer.AddCashPrompt.EnterAmount("100");
            _payoutPage.CashDrawer.AddCashPrompt.EnterPassword("");
            _payoutPage.CashDrawer.AddCashPrompt.Confirm();

            Assert.True(_payoutPage.ErrorIsDisplayed(_payoutPage.CashDrawer.AddCashPrompt.PasswordTextbox));
        }

        //Verify error is displayed if cash drawer has insufficient funds to payout the voucher
        [Test]
        public void CashDrawerPrompt_InsufficientBalanceForVoucher()
        {

            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 4;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);

            Assert.True(_payoutPage.PayoutError.IsOpen);
        }

        //Enter voucher which amount exceeds the cash drawer balance. Add cash then try to payout the voucher
        [Test]
        public void CashDrawerPrompt_InsufficientBalanceForVoucher_AddCash()
        {

            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 4;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);

            Assert.True(_payoutPage.PayoutError.IsOpen);
            _payoutPage.PayoutError.Confirm();

            _payoutPage.CashDrawer.AddCash("1", TestData.CashierPassword);
            _payoutPage.NumPad.EnterBarcode(barcode);

            Assert.False(_payoutPage.PayoutError.IsOpen);

            _payoutPage.Payout();

            Assert.Zero(_payoutPage.CashDrawer.CurrentBalance);
        }

        //Verify cash added and current balance increase after adding cash
        [Test]
        public void CashDrawerPrompt_AddCash()
        {

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var cashAddedCountBefore = _payoutPage.CashDrawer.CashAdded;
            var balanceBefore = _payoutPage.CashDrawer.CurrentBalance;

            _payoutPage.CashDrawer.AddCash("100", TestData.CashierPassword);

            var cashAddedCountAfter = _payoutPage.CashDrawer.CashAdded;
            var balanceAfter = _payoutPage.CashDrawer.CurrentBalance;

            var expectedAddedCash = 100;
            var expectedBalance = startingBalance + 100;

            Assert.AreEqual(expectedAddedCash, cashAddedCountAfter);
            Assert.AreEqual(expectedBalance, balanceAfter);
        }

        //Verify an error is displayed if the user enters the wrong password on the Remove Cash screen
        [Test]
        public void CashDrawerPrompt_RemoveCash_IncorrectPassword()
        {

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.CashDrawer.ClickRemoveCash();

            Assert.False(_payoutPage.ErrorIsDisplayed(_payoutPage.CashDrawer.RemoveCashPrompt.PasswordTextbox));

            _payoutPage.CashDrawer.RemoveCashPrompt.EnterAmount("100");
            _payoutPage.CashDrawer.RemoveCashPrompt.EnterPassword("Diamond1!");
            _payoutPage.CashDrawer.RemoveCashPrompt.Confirm();

            Assert.True(_payoutPage.ErrorIsDisplayed(_payoutPage.CashDrawer.RemoveCashPrompt.PasswordTextbox));
        }

        //verify error is displayed if password is left empty on remove cash prompt
        [Test]
        public void CashDrawerPrompt_RemoveCash_EmptyPassword()
        {

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.CashDrawer.ClickRemoveCash();

            Assert.False(_payoutPage.ErrorIsDisplayed(_payoutPage.CashDrawer.RemoveCashPrompt.PasswordTextbox));

            _payoutPage.CashDrawer.RemoveCashPrompt.EnterAmount("100");
            _payoutPage.CashDrawer.RemoveCashPrompt.EnterPassword("");
            _payoutPage.CashDrawer.RemoveCashPrompt.Confirm();

            Assert.True(_payoutPage.ErrorIsDisplayed(_payoutPage.CashDrawer.RemoveCashPrompt.PasswordTextbox));
        }

        //Verify an error is displayed if the removed amount is greater than the current balance
        [Test]
        public void CashDrawerPrompt_RemoveCash_GreaterThanBalance()
        {

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 4;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.CashDrawer.ClickRemoveCash();

            Assert.False(_payoutPage.ErrorIsDisplayed(_payoutPage.CashDrawer.RemoveCashPrompt.AmountTextbox));

            _payoutPage.CashDrawer.RemoveCashPrompt.EnterAmount("5");
            _payoutPage.CashDrawer.RemoveCashPrompt.EnterPassword(TestData.CashierPassword);
            _payoutPage.CashDrawer.RemoveCashPrompt.Confirm();

            Assert.True(_payoutPage.CashDrawer.RemoveCashPrompt.IsOpen);
        }

        //Add a voucher then remove cash so the balance is insufficient to payout the voucher. Verify an error is displayed
        //and the user cannot payout the voucher
        [Test]
        public void CashDrawerPrompt_RemoveCash_PayoutVoucher()
        {

            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 5;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);

            _payoutPage.CashDrawer.RemoveCash("1",TestData.CashierPassword);
            _payoutPage.ClickPayout();

            int voucherCount = _payoutPage.CurrentTransactionList.RowCount;

            Assert.AreEqual(1, voucherCount);
            Assert.False(_payoutPage.PayoutConfirmationAlert.IsOpen);
        }

        //Verify current balance decreases and cash removed increases after removing cash
        [Test]
        public void CashDrawerPrompt_RemoveCash()
        {

            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 5;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var balanceBefore = _payoutPage.CashDrawer.CurrentBalance;
            var removedBefore = _payoutPage.CashDrawer.CashRemoved;

            Assert.Zero(removedBefore);

            _payoutPage.CashDrawer.RemoveCash("1", TestData.CashierPassword);

            var balanceAfter = _payoutPage.CashDrawer.CurrentBalance;
            var expectedBalance = 4;

            var removedAfter = _payoutPage.CashDrawer.CashRemoved;
            var expectedRemoved = 1;

            Assert.AreEqual(expectedRemoved,removedAfter);
            Assert.AreEqual(expectedBalance,balanceAfter);
        }

        //Verify starting balance cannot be over 250,000
        [Test]
        public void CashDrawerPrompt_StartingBalance_Max()
        {

            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = TestData.CashDrawerMaxBalance + 1;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            Assert.True(_payoutPage.CashDrawer.StartingBalancePrompt.IsOpen);
        }

        //Add amount of cash greater than max allowed limit of 250,000
        [Test]
        public void CashDrawerPrompt_AddCash_Max()
        {

            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.CashDrawer.ClickAddCash();
            _payoutPage.CashDrawer.AddCashPrompt.EnterAmount(TestData.CashDrawerMaxBalance.ToString());
            _payoutPage.CashDrawer.AddCashPrompt.EnterPassword(TestData.CashierPassword);
            _payoutPage.CashDrawer.AddCashPrompt.Confirm();

            Assert.True(_payoutPage.CashDrawer.AddCashPrompt.IsOpen);
        }

        //enter amount more than 250,000 when removing cash
        [Test]
        public void CashDrawerPrompt_RemoveCash_Max()
        {

            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 250000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.CashDrawer.ClickRemoveCash();
            _payoutPage.CashDrawer.RemoveCashPrompt.EnterAmount((TestData.CashDrawerMaxBalance + 1).ToString());
            _payoutPage.CashDrawer.RemoveCashPrompt.EnterPassword(TestData.CashierPassword);
            _payoutPage.CashDrawer.RemoveCashPrompt.Confirm();

            Assert.True(_payoutPage.CashDrawer.RemoveCashPrompt.IsOpen);
        }

        //Verify Starting Balance transaction appears in cash drawer history
        [Test]
        public void CashDrawerPrompt_CashDrawerHistory_StartingBalance()
        {

            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.CashDrawer.ClickCashDrawerHistory();
            var trans = _payoutPage.CashDrawer.CashDrawerHistory.GetTransactions();

            var currentBalanceTrans = trans.SingleOrDefault(x => x.TransactionType == CashDrawerTransactionType.StartingBalance);
            Assert.NotNull(currentBalanceTrans);

            Assert.AreEqual(startingBalance, currentBalanceTrans.Amount);
        }

        //verify add cash transaction get recorded to cash drawer history
        [Test]
        public void CashDrawerPrompt_CashDrawerHistory_AddCash()
        {

            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.CashDrawer.AddCash("500", TestData.CashierPassword);

            _payoutPage.CashDrawer.ClickCashDrawerHistory();
            var trans = _payoutPage.CashDrawer.CashDrawerHistory.GetTransactions();
            var currentBalance = _payoutPage.CashDrawer.CashDrawerHistory.CurrentBalance;

            var addCashTrans = trans.FirstOrDefault(x => x.TransactionType == CashDrawerTransactionType.CashAdded);
            
            Assert.AreEqual(500,addCashTrans.Amount);
            Assert.AreEqual(1500,currentBalance);
        }

        //Verify remove cash transaction gets recorded in cash drawer history
        [Test]
        public void CashDrawerPrompt_CashDrawerHistory_RemoveCash()
        {

            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.CashDrawer.RemoveCash("500", TestData.CashierPassword);

            _payoutPage.CashDrawer.ClickCashDrawerHistory();
            var trans = _payoutPage.CashDrawer.CashDrawerHistory.GetTransactions();
            var currentBalance = _payoutPage.CashDrawer.CashDrawerHistory.CurrentBalance;

            var removeCashTrans = trans.FirstOrDefault(x => x.TransactionType == CashDrawerTransactionType.CashRemoved);

            Assert.AreEqual(500, removeCashTrans.Amount);
            Assert.AreEqual(500, currentBalance);
        }

        //Verify remove cash transaction gets recorded in cash drawer history
        [Test]
        public void CashDrawerPrompt_CashDrawerHistory_MultipleRecords()
        {

            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.CashDrawer.RemoveCash("500", TestData.CashierPassword);
            _payoutPage.CashDrawer.AddCash("200", TestData.CashierPassword);
            _payoutPage.CashDrawer.AddCash("200",TestData.CashierPassword);
            _payoutPage.CashDrawer.RemoveCash("100",TestData.CashierPassword);

            _payoutPage.CashDrawer.ClickCashDrawerHistory();
            var trans = _payoutPage.CashDrawer.CashDrawerHistory.GetTransactions();
            var currentBalance = _payoutPage.CashDrawer.CashDrawerHistory.CurrentBalance;

            Assert.AreEqual(5,trans.Count);
            var start = trans.FirstOrDefault(x => x.TransactionType == CashDrawerTransactionType.StartingBalance && x.Amount == 1000);
            Assert.NotNull(start);
            var rem1 = trans.FirstOrDefault(x => x.TransactionType == CashDrawerTransactionType.CashRemoved && x.Amount == 500);
            Assert.NotNull(rem1);
            var add1 = trans.FirstOrDefault(x => x.TransactionType == CashDrawerTransactionType.CashAdded && x.Amount == 200);
            Assert.NotNull(add1);
            var add2 = trans.FirstOrDefault(x => x.TransactionType == CashDrawerTransactionType.CashAdded && x.Amount == 200);
            Assert.NotNull(add2);
            var rem2 = trans.FirstOrDefault(x => x.TransactionType == CashDrawerTransactionType.CashRemoved && x.Amount == 100);
            Assert.NotNull(rem2);

            Assert.AreEqual(800, currentBalance);
        }

        [Test]
        public void Test()
        {
            

            var reader = new ExcelReader();
            //reader.Open(@"C:\Users\Ben\Downloads\20221107083023.xlsx");
            reader.Open(@"C:\Users\Ben\Downloads\Cashier Balance_drops_and_Sessions.xlsx");
            var report = reader.ParseCashierBalanceReport(includeVouchers: true);

            Console.WriteLine("Title: " + report.Title);
            Console.WriteLine("Runtime: " + report.RunDate);
            Console.WriteLine("Period: " + report.ReportPeriod);

            var sessions = report.Data;
            int count = 0;
            foreach(var session in sessions)
            {
                if(count > 5)
                {
                    break;
                }

                Console.WriteLine("==========================");
                Console.WriteLine(session.SessionId);
                Console.WriteLine("Start: " + session.StartDate);
                Console.WriteLine("End: " + session.EndDate);
                Console.WriteLine("Start Balance: " + session.StartBalance);
                Console.WriteLine("Total Payout: " + session.TotalPayoutAmount);
                Console.WriteLine("Added: " + session.TotalAmountAdded);
                Console.WriteLine("Removed: " + session.TotalAmountRemoved);
                Console.WriteLine("End Balance: " + session.EndBalance);

                count++;
            }

            Console.WriteLine();
            Console.WriteLine("========= Totals ============");
            Console.WriteLine("Start: " + report.TotalStartingBalance);
            Console.WriteLine("Payout: " + report.TotalPayoutAmount);
            Console.WriteLine("Added: " + report.TotalAmountAdded);
            Console.WriteLine("Removed: " + report.TotalAmountRemoved);
            Console.WriteLine("End: " + report.TotalEndBalance);
            Console.WriteLine();

            Console.WriteLine("=========== Vouchers ================");
            var vouchers = report.UnpaidVouchers;
            foreach(var voucher in vouchers)
            {
                Console.WriteLine("===================");
                Console.WriteLine(voucher.VoucherNumber + ", " + voucher.Amount + ", " + voucher.CreatedDate);
            }
            Console.WriteLine("======= VOucher Totals ===========");
            Console.WriteLine(report.TotalUnpaidVoucherAmount);
        }
    }
}