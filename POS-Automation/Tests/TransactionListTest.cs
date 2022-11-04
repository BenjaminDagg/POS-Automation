using NUnit.Framework;
using System;
using System.Threading;
using POS_Automation.Model;
using System.Threading.Tasks;
using POS_Automation.Pages.Payout;
using POS_Automation.Pages;

namespace POS_Automation
{
    public class TransactionListTest : BaseTest
    {
        private LoginPage _loginPage;
        private VoucherNumPad _numPad;
        private PayoutPage _payoutPage;
        private CashDrawer _cashDrawer;
        private TransactionList _transList;

        [SetUp]
        public void Setup()
        {
            _loginPage = new LoginPage(driver);
            _numPad = new VoucherNumPad(driver);
            _payoutPage = new PayoutPage(driver);
            _cashDrawer = new CashDrawer(driver);
            _transList = new TransactionList(driver);
        }

        [TearDown]
        public void TearDown()
        {

        }

        [Test]
        public void ReadList()
        {
            _loginPage.Login("bdagg", "Diamond4!");
            NavigationTabs.ClickPayoutTab();

            _payoutPage.StartingBalancePrompt.EnterInput("100");
            _payoutPage.StartingBalancePrompt.Confirm();
            Thread.Sleep(2000);

            _numPad.EnterBarcode("339577064783507892");
            _transList.ReadTable();
        }

        [Test]
        public void RemoveVoucher()
        {
            _loginPage.Login("bdagg", "Diamond4!");
            NavigationTabs.ClickPayoutTab();

            _payoutPage.StartingBalancePrompt.EnterInput("100");
            _payoutPage.StartingBalancePrompt.Confirm();
            Thread.Sleep(2000);

            _numPad.EnterBarcode("339577064783507892");
            _transList.RemoveVoucherByBarcode("339577064783507892");
        }

        [Test]
        public void Labels()
        {
            _loginPage.Login("bdagg", "Diamond4!");
            NavigationTabs.ClickPayoutTab();

            _payoutPage.StartingBalancePrompt.EnterInput("100");
            _payoutPage.StartingBalancePrompt.Confirm();
            Thread.Sleep(2000);

            _numPad.EnterBarcode("339577064783507892");
            Console.WriteLine(_transList.VoucherCount);
            Console.WriteLine(_transList.TotalPayout);
        }

        [Test]
        public void LastReceipt()
        {
            _loginPage.Login("bdagg", "Diamond4!");
            NavigationTabs.ClickPayoutTab();

            _payoutPage.StartingBalancePrompt.EnterInput("100");
            _payoutPage.StartingBalancePrompt.Confirm();
            Thread.Sleep(2000);

            _transList.ClickLastReceiptButton();
            Console.WriteLine(_transList.LastReceiptWindow.ReceiptNumber);
            Console.WriteLine(_transList.LastReceiptWindow.NumberOfVouchers);
            Console.WriteLine(_transList.LastReceiptWindow.Amount);
            _transList.LastReceiptWindow.Close();
        }

        [Test]
        public void CancelTrans()
        {
            _loginPage.Login("bdagg", "Diamond4!");
            NavigationTabs.ClickPayoutTab();

            _payoutPage.StartingBalancePrompt.EnterInput("100");
            _payoutPage.StartingBalancePrompt.Confirm();
            Thread.Sleep(2000);

            _numPad.EnterBarcode("339577064783507892");
            Thread.Sleep(3000);
            _payoutPage.CancelTransaction();
        }

        [Test]
        public void Payout()
        {
            _loginPage.Login("bdagg", "Diamond4!");
            NavigationTabs.ClickPayoutTab();

            _payoutPage.StartingBalancePrompt.EnterInput("100");
            _payoutPage.StartingBalancePrompt.Confirm();
            Thread.Sleep(2000);

            _numPad.EnterBarcode("339577889366320392");
            Thread.Sleep(3000);
            _payoutPage.Payout();
        }
    }
}