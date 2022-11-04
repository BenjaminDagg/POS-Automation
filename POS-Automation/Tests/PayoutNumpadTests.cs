using NUnit.Framework;
using System;
using System.Threading;
using POS_Automation.Model;
using System.Threading.Tasks;
using POS_Automation.Pages.Payout;

namespace POS_Automation
{
    public class PayoutNumpadTests : BaseTest
    {
        private LoginPage _loginPage;
        private VoucherNumPad _numPad;
        private PayoutPage _payoutPage;

        [SetUp]
        public void Setup()
        {
            _loginPage = new LoginPage(driver);
            _numPad = new VoucherNumPad(driver);
            _payoutPage = new PayoutPage(driver);
        }

        [TearDown]
        public void TearDown()
        {

        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        public void PressNumber(int numToType)
        {
            _loginPage.Login("bdagg", "Diamond4!");
            NavigationTabs.ClickPayoutTab();

            _payoutPage.StartingBalancePrompt.EnterInput("100");
            _payoutPage.StartingBalancePrompt.Confirm();
            Thread.Sleep(2000);

            _numPad.PressNumKey(numToType);

            string enteredText = _numPad.GetBarcode();
            int enteredNum = int.Parse(enteredText);

            Assert.AreEqual(numToType, enteredNum);

        }

        [Test]
        public void Backspace()
        {
            _loginPage.Login("bdagg", "Diamond4!");
            NavigationTabs.ClickPayoutTab();

            _payoutPage.StartingBalancePrompt.EnterInput("100");
            _payoutPage.StartingBalancePrompt.Confirm();
            Thread.Sleep(2000);

            _numPad.PressNumKey(1);

            string enteredText = _numPad.GetBarcode();
            Assert.AreEqual(1,enteredText.Length);

            _numPad.PressBackspace();

            enteredText = _numPad.GetBarcode();
            Assert.AreEqual(0, enteredText.Length);

        }

        [Test]
        public void Clear()
        {
            _loginPage.Login("bdagg", "Diamond4!");
            NavigationTabs.ClickPayoutTab();

            _payoutPage.StartingBalancePrompt.EnterInput("100");
            _payoutPage.StartingBalancePrompt.Confirm();
            Thread.Sleep(2000);

            _numPad.EnterBarcode("1234");

            string enteredText = _numPad.GetBarcode();
            Assert.AreEqual(4, enteredText.Length);

            _numPad.Clear();

            enteredText = _numPad.GetBarcode();
            Assert.AreEqual(0, enteredText.Length);
        }

        [Test]
        public void Enter()
        {
            _loginPage.Login("bdagg", "Diamond4!");
            NavigationTabs.ClickPayoutTab();

            _payoutPage.StartingBalancePrompt.EnterInput("100");
            _payoutPage.StartingBalancePrompt.Confirm();
            Thread.Sleep(2000);

            _numPad.EnterBarcode("1234");

            string enteredText = _numPad.GetBarcode();
            Assert.AreEqual(4, enteredText.Length);

            _numPad.PressEnter();
        }
    }
}