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
    public class LastReceiptTest : BaseTest
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

            _transRepo.SetReceiptReprintEnabled(true);
        }


        //Verify if scaneed barcode is a jackpot voucher and the amount is over the LOCKUP_AMOUNT, then voucher requires approval
        [Test]
        public void LastReceipt_Open()
        {

            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);
            _payoutPage.Payout();
            _payoutPage.CurrentTransactionList.ClickLastReceiptButton();

            Assert.True(_payoutPage.CurrentTransactionList.LastReceiptWindow.IsOpen);
        }

        [Test]
        public void LastReceipt_NumberOfVouchers_Field()
        {
            var vouchers = new List<string>();

            for(int i = 0; i < 5; i++)
            {
                var barcode = TpService.GetVoucher(StartingAmountCredits, 500);
                StartingAmountCredits -= 500;

                vouchers.Add(barcode);
            }

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            foreach(var barcode in vouchers)
            {
                _payoutPage.NumPad.EnterBarcode(barcode);
            }

            _payoutPage.Payout();
            _payoutPage.CurrentTransactionList.ClickLastReceiptButton();

            Assert.AreEqual(vouchers.Count,_payoutPage.CurrentTransactionList.LastReceiptWindow.NumberOfVouchers);
        }

        [Test]
        public void LastReceipt_Amount_Field()
        {
           
            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;

            var barcode2 = TpService.GetVoucher(StartingAmountCredits, 1500);
            StartingAmountCredits -= 1500;

            var barcode3 = TpService.GetVoucher(StartingAmountCredits, 2000);
            StartingAmountCredits -= 2000;

            var vouchers = new List<string>() { barcode1, barcode2, barcode3 };

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            foreach (var barcode in vouchers)
            {
                _payoutPage.NumPad.EnterBarcode(barcode);
            }

            _payoutPage.Payout();
            _payoutPage.CurrentTransactionList.ClickLastReceiptButton();

            Assert.AreEqual(40, _payoutPage.CurrentTransactionList.LastReceiptWindow.Amount);
        }

        [Test]
        public void LastReceipt_Reprint_Enabled()
        {

            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);
            _payoutPage.Payout();

            Assert.False(_payoutPage.IsReadOnly(_payoutPage.CurrentTransactionList.LastReceiptButton));
        }

        [Test]
        public void LastReceipt_Reprint_Disabled()
        {
            _transRepo.SetReceiptReprintEnabled(false);

            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);
            _payoutPage.Payout();

            Assert.True(_payoutPage.IsReadOnly(_payoutPage.CurrentTransactionList.LastReceiptButton));
        }
    }
}