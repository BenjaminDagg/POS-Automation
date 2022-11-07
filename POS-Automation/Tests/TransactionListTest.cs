using NUnit.Framework;
using System;
using System.Threading;
using POS_Automation.Model;
using System.Threading.Tasks;
using POS_Automation.Pages.Payout;
using POS_Automation.Pages;
using System.Linq;
using POS_Automation.Model.Payout;
using System.Collections.Generic;

namespace POS_Automation
{
    public class TransactionListTest : BaseTest
    {
        private LoginPage _loginPage;
        private VoucherNumPad _numPad;
        private PayoutPage _payoutPage;
        private CashDrawer _cashDrawer;
        private TransactionList _transList;
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
        public void ReadList()
        {
            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);
            var vouchers = _payoutPage.CurrentTransactionList.GetVouchers();

            foreach(var voucher in vouchers)
            {
                Console.WriteLine(voucher.Barcode);
                Console.WriteLine(voucher.Amount);
                Console.WriteLine(voucher.VoucherId);
                Console.WriteLine(voucher.CreatedDate);
                Console.WriteLine(voucher.Location);
            }
        }

        //Verify voucher is added to the list once scanned
        [Test]
        public void VoucherList_AddVoucher()
        {
            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var countBefore = _payoutPage.CurrentTransactionList.RowCount;
            Assert.Zero(countBefore);

            _payoutPage.NumPad.EnterBarcode(barcode);

            var countAfter = _payoutPage.CurrentTransactionList.RowCount;
            var voucherCount = _payoutPage.CurrentTransactionList.VoucherCount;

            Assert.AreEqual(1, countAfter);
            Assert.AreEqual(1, voucherCount);
        }

        [Test]
        public void VoucherList_RemoveVoucher_Prompt()
        {
            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);
            Thread.Sleep(2000);

            _payoutPage.CurrentTransactionList.ClickRemoveVoucherByBarcode(barcode);

            Assert.True(_payoutPage.CurrentTransactionList.RemoveVoucherPrompt.IsOpen);

            string text = _payoutPage.CurrentTransactionList.RemoveVoucherPrompt.AlertText;
            string expectedText = $"Are you sure you want to remove voucher {barcode}, with amount $5.00 from the transaction?";

            Assert.AreEqual(expectedText,text);
        }

        //verify when the transaction is payed out the list is cleared
        [Test]
        public void VoucherList_ListCount_Payout()
        {
            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var count = _payoutPage.CurrentTransactionList.RowCount;
            Assert.Zero(count);

            _payoutPage.NumPad.EnterBarcode(barcode);

            count = _payoutPage.CurrentTransactionList.RowCount;
            Assert.AreEqual(1, count);

            _payoutPage.Payout();

            count = _payoutPage.CurrentTransactionList.RowCount;
            Assert.Zero(count);
        }

        //Verify when transaction is cancelled, the list is cleared
        [Test]
        public void VoucherList_ListCount_CancelTransaction()
        {
            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var count = _payoutPage.CurrentTransactionList.RowCount;
            Assert.Zero(count);

            _payoutPage.NumPad.EnterBarcode(barcode);

            count = _payoutPage.CurrentTransactionList.RowCount;
            Assert.AreEqual(1, count);

            _payoutPage.CancelTransaction();

            count = _payoutPage.CurrentTransactionList.RowCount;
            Assert.Zero(count);
        }

        //Verify voucher ocunt label displays correct amount when ultiple vouchers are in the same transaction
        [Test]
        public void VoucherList_VoucherCount_Multiple()
        {
            //create a voucher for $100
            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;
            var barcode2 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;
            var barcode3 = TpService.GetVoucher(StartingAmountCredits, 500);

            var vouchers = new List<string>() { barcode1,barcode2, barcode3 };

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var count = _payoutPage.CurrentTransactionList.RowCount;
            Assert.Zero(count);

            for(int i = 0; i < vouchers.Count;i++)
            {
                _payoutPage.NumPad.EnterBarcode(vouchers[i]);
            }

            count = _payoutPage.CurrentTransactionList.RowCount;
            Assert.AreEqual(vouchers.Count,count );
        }

        //Verfy coucher count decreases when a voucher is removed from the transaction
        [Test]
        public void VoucherList_VoucherCount_RemoveVoucher()
        {
            //create a voucher for $100
            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;
            var barcode2 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;
            var barcode3 = TpService.GetVoucher(StartingAmountCredits, 500);

            var vouchers = new List<string>() { barcode1, barcode2, barcode3 };

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var count = _payoutPage.CurrentTransactionList.RowCount;
            Assert.Zero(count);

            for (int i = 0; i < vouchers.Count; i++)
            {
                _payoutPage.NumPad.EnterBarcode(vouchers[i]);
            }

            count = _payoutPage.CurrentTransactionList.RowCount;
            Assert.AreEqual(vouchers.Count, count);

            _payoutPage.CurrentTransactionList.RemoveVoucherByBarcode(barcode2);

            Assert.AreEqual(2,_payoutPage.CurrentTransactionList.VoucherCount);
        }

        //Voucher doesn't get removed from transaction if user hits cancel on prompt
        [Test]
        public void VoucherList_RemoveVoucher_Cancel()
        {
            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var count = _payoutPage.CurrentTransactionList.RowCount;
            Assert.Zero(count);

            _payoutPage.NumPad.EnterBarcode(barcode);

            count = _payoutPage.CurrentTransactionList.RowCount;
            Assert.AreEqual(1, count);

            _payoutPage.CurrentTransactionList.ClickRemoveVoucherByBarcode(barcode);
            _payoutPage.CurrentTransactionList.RemoveVoucherPrompt.Cancel();

            count = _payoutPage.CurrentTransactionList.RowCount;
            Assert.AreEqual(1, count);
        }

        //Verify the Total payout label displays the vlaue off all vouchers in the transaction added together
        [Test]
        public void VoucherList_TotalPayout()
        {
            //create a voucher for $100
            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;
            var barcode2 = TpService.GetVoucher(StartingAmountCredits, 1000);
            StartingAmountCredits -= 1000;
            var barcode3 = TpService.GetVoucher(StartingAmountCredits, 1500);

            var vouchers = new List<string>() { barcode1, barcode2, barcode3 };

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var total = _payoutPage.CurrentTransactionList.TotalPayout;
            Assert.Zero(total);

            for (int i = 0; i < vouchers.Count; i++)
            {
                _payoutPage.NumPad.EnterBarcode(vouchers[i]);
            }

            total = _payoutPage.CurrentTransactionList.TotalPayout;
            Assert.AreEqual(30,total);
        }

        //Verify total payout decreases by the voucher amount when a voucher is removed from the transaction
        [Test]
        public void VoucherList_TotalPayout_RemoveVoucher()
        {
            //create a voucher for $100
            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;
            var barcode2 = TpService.GetVoucher(StartingAmountCredits, 1000);
            StartingAmountCredits -= 1000;
            var barcode3 = TpService.GetVoucher(StartingAmountCredits, 1500);

            var vouchers = new List<string>() { barcode1, barcode2, barcode3 };

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var total = _payoutPage.CurrentTransactionList.TotalPayout;
            Assert.Zero(total);

            for (int i = 0; i < vouchers.Count; i++)
            {
                _payoutPage.NumPad.EnterBarcode(vouchers[i]);
            }

            total = _payoutPage.CurrentTransactionList.TotalPayout;
            Assert.AreEqual(30, total);

            _payoutPage.CurrentTransactionList.RemoveVoucherByBarcode(barcode1);
            total = _payoutPage.CurrentTransactionList.TotalPayout;
            Assert.AreEqual(25, total);
        }

        //Verify payout and cancel buttons are disabled when all vouchers are removed from the list
        [Test]
        public void VoucherList_RemoveAll()
        {
            //create a voucher for $100
            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;
            var barcode2 = TpService.GetVoucher(StartingAmountCredits, 1000);
            StartingAmountCredits -= 1000;
            var barcode3 = TpService.GetVoucher(StartingAmountCredits, 1500);

            var vouchers = new List<string>() { barcode1, barcode2, barcode3 };

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();


            for (int i = 0; i < vouchers.Count; i++)
            {
                _payoutPage.NumPad.EnterBarcode(vouchers[i]);
            }

            foreach(var voucher in vouchers)
            {
                _payoutPage.CurrentTransactionList.RemoveVoucherByBarcode(voucher);
            }

            int count = _payoutPage.CurrentTransactionList.RowCount;
            Assert.Zero(count);

            Assert.True(_payoutPage.PayoutIsHidden());
        }

        //Verify the combines value of all vouchers in the transaction get deducted from the cash drawer balance
        [Test]
        public void VoucherList_CashDrawerBalance_MultipleVouchersInTransaction()
        {
            //create a voucher for $100
            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;
            var barcode2 = TpService.GetVoucher(StartingAmountCredits, 1000);
            StartingAmountCredits -= 1000;
            var barcode3 = TpService.GetVoucher(StartingAmountCredits, 1500);

            var vouchers = new List<string>() { barcode1, barcode2, barcode3 };

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();


            for (int i = 0; i < vouchers.Count; i++)
            {
                _payoutPage.NumPad.EnterBarcode(vouchers[i]);
            }

            _payoutPage.Payout();

            var balanceAfter = _payoutPage.CashDrawer.CurrentBalance;
            var expectedBalance = startingBalance - 30;

            Assert.AreEqual(expectedBalance, balanceAfter);
        }

        [Test]
        public void VoucherList_RemoveVoucher()
        {
            //create a voucher for $100
            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;
            var barcode2 = TpService.GetVoucher(StartingAmountCredits, 1000);
            StartingAmountCredits -= 1000;
            var barcode3 = TpService.GetVoucher(StartingAmountCredits, 1500);

            var vouchers = new List<string>() { barcode1, barcode2, barcode3 };

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();


            for (int i = 0; i < vouchers.Count; i++)
            {
                _payoutPage.NumPad.EnterBarcode(vouchers[i]);
            }

            int countBefore = _payoutPage.CurrentTransactionList.VoucherCount;
            Assert.AreEqual(3, countBefore);

            _payoutPage.CurrentTransactionList.RemoveVoucherByBarcode(vouchers[2]);

            int countAfter = _payoutPage.CurrentTransactionList.VoucherCount;

            Assert.AreEqual(2,countAfter);
        }

        //verify if a voucher is removed from the transaction its amount is not deducted from the cash drawer balance
        [Test]
        public void VoucherList_CashDrawerBalance_RemoveVoucher()
        {
            //create a voucher for $100
            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;
            var barcode2 = TpService.GetVoucher(StartingAmountCredits, 1000);
            StartingAmountCredits -= 1000;
            var barcode3 = TpService.GetVoucher(StartingAmountCredits, 1500);

            var vouchers = new List<string>() { barcode1, barcode2, barcode3 };

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();


            for (int i = 0; i < vouchers.Count; i++)
            {
                _payoutPage.NumPad.EnterBarcode(vouchers[i]);
            }

            _payoutPage.CurrentTransactionList.RemoveVoucherByBarcode(barcode1);

            _payoutPage.Payout();

            var balanceAfter = _payoutPage.CashDrawer.CurrentBalance;
            var expectedBalance = startingBalance - 25;

            Assert.AreEqual(expectedBalance, balanceAfter);
        }

        //Verify the scanned voucher data is displayed correctly in the list
        [Test]
        public void VoucherList_VerifyVoucherData()
        {
            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);

            var vouchers = _payoutPage.CurrentTransactionList.GetVouchers();
            var targetVoucher = vouchers[0];

            Assert.AreEqual(barcode,targetVoucher.Barcode);
            Assert.AreEqual(5,targetVoucher.Amount);
            Assert.AreEqual(TestData.DefaultMachineNumber, targetVoucher.Location);
        }
    }
}