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
using POS_Automation.Pages.Reports;
using System.Globalization;
using System.IO;

namespace POS_Automation
{
    public class SessionTrackingTests : BaseTest
    {
        private LoginPage _loginPage;
        private VoucherNumPad _numPad;
        private PayoutPage _payoutPage;
        private CashDrawer _cashDrawer;
        private ReportListPage _reportList;
        private ReportPage _reportPage;
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
            _reportList = new ReportListPage(driver);
            _reportPage = new ReportPage(driver);

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

        //Session is created when user enters the Payout menu
        [Test]
        public void SessionTracking_BeginSession()
        {
            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var sessionId = _transRepo.GetCurrentUserSession(TestData.CashierUsername);
            Assert.False(string.IsNullOrEmpty(sessionId));
        }

        //Begin session is recorded in report
        [Test]
        public void SessionTracking_ReportBeginSession()
        {
            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var sessionId = _transRepo.GetCurrentUserSession(TestData.SuperUserUsername);
            _payoutPage.NavigationTabs.ClickDeviceTab();
            _payoutPage.NavigationTabs.ClickReportsTab();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");

            _reportList.ClickReportByReportName("Cash Bank Activity");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.RunReport();

            string filename = DateTime.Now.ToString("HHmmssfff") + ".xlsx";
            string filepath = TestData.DownloadPath;
            string fullPath = filepath + @"\" + filename;

            _reportPage.ReportMenu.ExportDropdown.SelectByIndex(1);
            _reportPage.SaveFileWindow.EnterFilepath(filepath);
            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashBankActivityReport();

            var session = report.GetSession(sessionId);

            Assert.AreEqual(session.SessionId,sessionId);

            var beginSessionTransaction = session.Transactions.SingleOrDefault(t => t.TransType == "Start");
            Assert.AreEqual(startingBalance, beginSessionTransaction.Money);
        }

        //Navigating away from the Payout screen should end the users' current session
        [Test]
        public void SessionTracking_EndSession_NavigateAway()
        {
            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var sessionId = _transRepo.GetCurrentUserSession(TestData.SuperUserUsername);

            //navigate away to end session
            _payoutPage.NavigationTabs.ClickDeviceTab();
            Thread.Sleep(3000);
            _payoutPage.NavigationTabs.ClickReportsTab();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");

            _reportList.ClickReportByReportName("Cash Bank Activity");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.RunReport();

            string filename = DateTime.Now.ToString("HHmmssfff") + ".xlsx";
            string filepath = TestData.DownloadPath;
            string fullPath = filepath + @"\" + filename;

            _reportPage.ReportMenu.ExportDropdown.SelectByIndex(1);
            _reportPage.SaveFileWindow.EnterFilepath(filepath);
            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashBankActivityReport();

            var session = report.GetSession(sessionId);

            Assert.AreEqual(session.SessionId, sessionId);

            var endSessionTrans = session.Transactions.FirstOrDefault(t => t.TransType == "End");
            Assert.NotNull(endSessionTrans);

            Assert.AreEqual(startingBalance,endSessionTrans.Money);
            Assert.AreEqual(startingBalance, session.TotalMoney);
        }

        //Session should end when user logs out
        [Test]
        public void SessionTracking_EndSession_Logout()
        {
            _loginPage.Login(TestData.CashierUsername2, TestData.CashierPassword2);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var sessionId = _transRepo.GetCurrentUserSession(TestData.CashierUsername2);

            _payoutPage.Logout();

            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();
            _payoutPage.NavigationTabs.ClickReportsTab();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");

            _reportList.ClickReportByReportName("Cash Bank Activity");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.RunReport();

            string filename = DateTime.Now.ToString("HHmmssfff") + ".xlsx";
            string filepath = TestData.DownloadPath;
            string fullPath = filepath + @"\" + filename;

            _reportPage.ReportMenu.ExportDropdown.SelectByIndex(1);
            _reportPage.SaveFileWindow.EnterFilepath(filepath);
            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashBankActivityReport();

            var session = report.GetSession(sessionId);

            Assert.AreEqual(session.SessionId, sessionId);

            var endSessionTrans = session.Transactions.FirstOrDefault(t => t.TransType == "End");
            Assert.NotNull(endSessionTrans);

            Assert.AreEqual(startingBalance, endSessionTrans.Money);
            Assert.AreEqual(startingBalance, session.TotalMoney);
        }

        //Session start and end are recorded in report
        [Test]
        public void SessionTracking_Report()
        {
            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var sessionId = _transRepo.GetCurrentUserSession(TestData.CashierUsername);

            _payoutPage.Logout();

            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();
            _payoutPage.NavigationTabs.ClickReportsTab();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");

            _reportList.ClickReportByReportName("Cash Bank Activity");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.RunReport();

            string filename = DateTime.Now.ToString("HHmmssfff") + ".xlsx";
            string filepath = TestData.DownloadPath;
            string fullPath = filepath + @"\" + filename;

            _reportPage.ReportMenu.ExportDropdown.SelectByIndex(1);
            _reportPage.SaveFileWindow.EnterFilepath(filepath);
            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashBankActivityReport();

            var session = report.GetSession(sessionId);

            Assert.AreEqual(session.SessionId, sessionId);

            //verify starting balance and start session are recorded
            var startSession = session.Transactions.FirstOrDefault(t => t.TransType == "Start");
            Assert.NotNull(startSession);
            Assert.AreEqual(startingBalance, startSession.Money);

            var endSessionTrans = session.Transactions.FirstOrDefault(t => t.TransType == "End");
            Assert.NotNull(endSessionTrans);

            Assert.AreEqual(startingBalance, endSessionTrans.Money);
            Assert.AreEqual(startingBalance, session.TotalMoney);
        }

        //Add cash event should be recorded for the session
        [Test]
        public void SessionTracking_AddCash()
        {
            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var sessionId = _transRepo.GetCurrentUserSession(TestData.SuperUserUsername);

            _payoutPage.CashDrawer.AddCash("500", TestData.SuperUserPassword);

            _payoutPage.NavigationTabs.ClickReportsTab();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");

            _reportList.ClickReportByReportName("Cash Bank Activity");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.RunReport();

            string filename = DateTime.Now.ToString("HHmmssfff") + ".xlsx";
            string filepath = TestData.DownloadPath;
            string fullPath = filepath + @"\" + filename;

            _reportPage.ReportMenu.ExportDropdown.SelectByIndex(1);
            _reportPage.SaveFileWindow.EnterFilepath(filepath);
            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashBankActivityReport();

            var session = report.GetSession(sessionId);

            Assert.AreEqual(session.SessionId, sessionId);

            var addTrans = session.Transactions.FirstOrDefault(t => t.TransType == "Add");
            Assert.AreEqual(500, addTrans.Money);
            Assert.AreEqual(1500,session.TotalMoney);
        }

        //Remove cash s recorded for the session
        [Test]
        public void SessionTracking_RemoveCash()
        {
            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var sessionId = _transRepo.GetCurrentUserSession(TestData.SuperUserUsername);

            _payoutPage.CashDrawer.RemoveCash("500", TestData.SuperUserPassword);

            _payoutPage.NavigationTabs.ClickReportsTab();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");

            _reportList.ClickReportByReportName("Cash Bank Activity");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.RunReport();

            string filename = DateTime.Now.ToString("HHmmssfff") + ".xlsx";
            string filepath = TestData.DownloadPath;
            string fullPath = filepath + @"\" + filename;

            _reportPage.ReportMenu.ExportDropdown.SelectByIndex(1);
            _reportPage.SaveFileWindow.EnterFilepath(filepath);
            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashBankActivityReport();

            var session = report.GetSession(sessionId);

            Assert.AreEqual(session.SessionId, sessionId);

            var remTrans = session.Transactions.FirstOrDefault(t => t.TransType == "Remove");
            Assert.AreEqual(500, remTrans.Money);
            Assert.AreEqual(500, session.TotalMoney);
        }

        //Voucher payouts should be recorded for the session
        [Test]
        public void SessionTracking_Payout()
        {
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var sessionId = _transRepo.GetCurrentUserSession(TestData.SuperUserUsername);

            _payoutPage.NumPad.EnterBarcode(barcode);
            _payoutPage.Payout();

            _payoutPage.NavigationTabs.ClickReportsTab();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");

            _reportList.ClickReportByReportName("Cash Bank Activity");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.RunReport();

            string filename = DateTime.Now.ToString("HHmmssfff") + ".xlsx";
            string filepath = TestData.DownloadPath;
            string fullPath = filepath + @"\" + filename;

            _reportPage.ReportMenu.ExportDropdown.SelectByIndex(1);
            _reportPage.SaveFileWindow.EnterFilepath(filepath);
            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashBankActivityReport();

            var session = report.GetSession(sessionId);

            Assert.AreEqual(session.SessionId, sessionId);

            var payout = session.Transactions.FirstOrDefault(t => t.TransType == "Payout");
            Assert.AreEqual(5, payout.Payout);
            Assert.AreEqual(995, session.TotalMoney);
        }

        //Current transaction is voided if user ends session
        [Test]
        public void SessionTracking_EndSession_VoidTransaction()
        {
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername2, TestData.CashierPassword2);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var sessionId = _transRepo.GetCurrentUserSession(TestData.CashierUsername2);

            _payoutPage.NumPad.EnterBarcode(barcode);

            //end session
            _payoutPage.Logout();

            //login with new user
            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);
            _payoutPage.ClickPayout();

            Assert.True(_payoutPage.PayoutConfirmationAlert.IsOpen);
        }
    }
}
