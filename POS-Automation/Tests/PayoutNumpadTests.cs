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
        private TransactionPortalService TpService;
        private DatabaseManager _databaseManager;
        private decimal StartingAmountDollar = 1000.00m;
        private int StartingAmountCredits = 100000;

        [SetUp]
        public void Setup()
        {
            _loginPage = new LoginPage(driver);
            _numPad = new VoucherNumPad(driver);
            _payoutPage = new PayoutPage(driver);

            _databaseManager = new DatabaseManager();
            _databaseManager.ResetTestMachine(StartingAmountDollar);

            TpService = new TransactionPortalService(_logService);
            TpService.Connect();
        }

        [TearDown]
        public void TearDown()
        {
            TpService.Disconnect();

            StartingAmountDollar = 1000.00m;
            StartingAmountCredits = 100000;
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

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput("100");
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();
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

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput("100");
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();
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

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput("100");
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();
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

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput("100");
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();
            Thread.Sleep(2000);

            _numPad.EnterBarcode("1234");

            string enteredText = _numPad.GetBarcode();
            Assert.AreEqual(4, enteredText.Length);

            _numPad.PressEnter();
        }

        //Verify input accepts only numeric values
        [Test]
        public void NonNumeric()
        {
            _loginPage.Login("bdagg", "Diamond4!");
            NavigationTabs.ClickPayoutTab();

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput("100");
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();
            Thread.Sleep(2000);

            _numPad.EnterBarcode("abc!@#<>");

            var value = _numPad.GetBarcode();
            Assert.True(string.IsNullOrEmpty(value));
        }

        //Verify keypad doesn't accept dashes
        [Test]
        public void Dashes()
        {
            _loginPage.Login("bdagg", "Diamond4!");
            NavigationTabs.ClickPayoutTab();

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput("100");
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();
            Thread.Sleep(2000);

            _numPad.EnterBarcode("123-456");

            var value = _numPad.GetBarcode();
            Assert.AreEqual("123456",value);
        }

        //Verify the barcode input clears when barcode has been added to the tranaction
        [Test]
        public void InputClearOnValidation()
        {
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login("bdagg", "Diamond4!");
            NavigationTabs.ClickPayoutTab();

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput("100");
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();
            Thread.Sleep(2000);

            int barcodeLength = barcode.Length;
            string barcodeSubstr = barcode.Substring(0, barcodeLength - 1);

            _numPad.EnterBarcode(barcodeSubstr);

            var value = _numPad.GetBarcode();
            Assert.AreEqual(barcodeLength - 1,value.Length);
            
            _numPad.EnterBarcode(barcode);
            Thread.Sleep(1000);
            value = _numPad.GetBarcode();

            Assert.AreEqual(0, value.Length);
        }

        //Verify error is displayed if barcode is not correct length
        [Test]
        public void InvalidBarcodeLength()
        {
          
            _loginPage.Login("bdagg", "Diamond4!");
            NavigationTabs.ClickPayoutTab();

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput("100");
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();
            Thread.Sleep(2000);

            string barcode = new string('1', 17);
            _numPad.EnterBarcode(barcode);
            _numPad.PressEnter();

            Assert.AreEqual(17, _numPad.GetBarcode().Length);
            Assert.AreEqual(0,_payoutPage.CurrentTransactionList.VoucherCount);
        }
    }
}