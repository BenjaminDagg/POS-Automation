using NUnit.Framework;
using System;
using System.Threading;
using POS_Automation.Model;
using System.Threading.Tasks;
using POS_Automation.Pages.Payout;

namespace POS_Automation
{
    public class CashDrawerTest : BaseTest
    {
        private LoginPage _loginPage;
        private VoucherNumPad _numPad;
        private PayoutPage _payoutPage;
        private CashDrawer _cashDrawer;

        [SetUp]
        public void Setup()
        {
            _loginPage = new LoginPage(driver);
            _numPad = new VoucherNumPad(driver);
            _payoutPage = new PayoutPage(driver);
            _cashDrawer = new CashDrawer(driver);
        }

        [TearDown]
        public void TearDown()
        {

        }

        [Test]
        public void Labels()
        {
            _loginPage.Login("bdagg", "Diamond4!");
            NavigationTabs.ClickPayoutTab();

            _payoutPage.StartingBalancePrompt.EnterInput("100");
            _payoutPage.StartingBalancePrompt.Confirm();
            Thread.Sleep(2000);

            var currentBalance = _cashDrawer.CurrentBalance;
            Console.WriteLine("Current b = " + currentBalance);
            Console.WriteLine("start b = " + _cashDrawer.StartingBalance);
            Console.WriteLine("added b = " + _cashDrawer.CashAdded);
            Console.WriteLine("removed b = " + _cashDrawer.CashRemoved);
            Console.WriteLine("total b = " + _cashDrawer.TotalPayout);
        }

        [Test]
        public void AddCash()
        {
            _loginPage.Login("bdagg", "Diamond4!");
            NavigationTabs.ClickPayoutTab();

            _payoutPage.StartingBalancePrompt.EnterInput("100");
            _payoutPage.StartingBalancePrompt.Confirm();
            Thread.Sleep(2000);

            _cashDrawer.ClickAddCash();
            Assert.True(_cashDrawer.AddCashPrompt.IsOpen);

            _cashDrawer.AddCashPrompt.EnterAmount("1");
            _cashDrawer.AddCashPrompt.EnterPassword("Diamond4!");
            Thread.Sleep(5000);

            _cashDrawer.AddCashPrompt.Confirm();

            var val = _cashDrawer.CashAdded;
            Assert.AreEqual(1,val);
        }

        [Test]
        public void RemoveCash()
        {
            _loginPage.Login("bdagg", "Diamond4!");
            NavigationTabs.ClickPayoutTab();

            _payoutPage.StartingBalancePrompt.EnterInput("100");
            _payoutPage.StartingBalancePrompt.Confirm();
            Thread.Sleep(2000);

            _cashDrawer.ClickRemoveCash();
            Assert.True(_cashDrawer.AddCashPrompt.IsOpen);

            _cashDrawer.RemoveCashPrompt.EnterAmount("1");
            _cashDrawer.RemoveCashPrompt.EnterPassword("Diamond4!");
            Thread.Sleep(5000);

            _cashDrawer.RemoveCashPrompt.Confirm();

            var val = _cashDrawer.CashRemoved;
            Assert.AreEqual(1, val);
        }

        [Test]
        public void CashDrawerHistory()
        {
            _loginPage.Login("bdagg", "Diamond4!");
            NavigationTabs.ClickPayoutTab();

            _payoutPage.StartingBalancePrompt.EnterInput("100");
            _payoutPage.StartingBalancePrompt.Confirm();
            Thread.Sleep(2000);

            _cashDrawer.ClickCashDrawerHistory();
            Assert.True(_cashDrawer.CashDrawerHistory.IsOpen);
            _cashDrawer.CashDrawerHistory.ReadTable();
            Console.WriteLine("Current balance = " + _cashDrawer.CashDrawerHistory.CurrentBalance);
            _cashDrawer.CashDrawerHistory.Close();
        }
    }
}