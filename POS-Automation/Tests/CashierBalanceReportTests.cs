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
using System.IO;
using System.Globalization;

namespace POS_Automation
{
    public class CashierBalanceReportTests : BaseTest
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
        private GameSimulator GameSimulator;

        private decimal StartingAmountDollar = 1000.00m;
        private int StartingAmountCredits = 100000;
        private int CashDrawerStartingBalanceDollars = 2000;
        private List<string> TestVouchers = new List<string>();
        string ReportFileName = String.Empty;
        public int TotalVoucherValue = 30;
        public string UnpaidBarcode = string.Empty;
        public string ReportStartDate;
        public string ReportEndDate;
        public string SessionId;

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

            GameSimulator = new GameSimulator(_logService);

            string filepath = TestData.DownloadPath + @"\" + ReportFileName;
            bool exists = File.Exists(filepath);

            if (!exists)
            {
                GoToPayoutPage();
                PerformTransactions();
                EndSession();
                GenerateReport();

                filepath = TestData.DownloadPath + @"\" + ReportFileName;
                exists = File.Exists(filepath);

                while (!exists)
                {
                    Thread.Sleep(1000);
                    exists = File.Exists(filepath);

                    if (exists)
                    {
                        break;
                    }
                }
            }
            
        }

        [TearDown]
        public void TearDown()
        {
            TpService.Disconnect();

            _transRepo.SetAutoCashDrawerFlag(true);

            StartingAmountDollar = 1000.00m;
            StartingAmountCredits = 100000;

        }

        
        private void GoToPayoutPage()
        {
            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(CashDrawerStartingBalanceDollars.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            SessionId = _transRepo.GetCurrentUserSession(TestData.CashierUsername);
        }

        private void PerformTransactions()
        {
            //create a voucher for $100
            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;

            var barcode2 = TpService.GetVoucher(StartingAmountCredits, 1000);
            StartingAmountCredits -= 1000;

            var barcode3 = TpService.GetVoucher(StartingAmountCredits, 1500);
            StartingAmountCredits -= 1500;

            var barcode4 = TpService.GetVoucher(StartingAmountCredits, 1500);
            UnpaidBarcode = barcode4;

            var param = GameSimulator.gameplayParams;
            param.Count1Dollar = 1;
            param.Count5Dollar = 1;
            param.count10Dollar = 1;
            param.Count20Dollar = 1;
            param.Count50Dollar = 1;
            param.Count100Dollar = 2;
            param.DollarsInCredits = 28600;
            TpService.TransD(GameSimulator.gameplayParams);

            var vouchers = new List<string>() { barcode1, barcode2, barcode3 };
            TestVouchers = vouchers;


            //add cash
            _payoutPage.CashDrawer.AddCash("300", TestData.CashierPassword);

            //payout vouchers
            _payoutPage.NumPad.EnterBarcode(vouchers[0]);
            _payoutPage.Payout();

            //add cash
            _payoutPage.CashDrawer.AddCash("100", TestData.CashierPassword);

            //payout remaining vouchers
            _payoutPage.NumPad.EnterBarcode(vouchers[1]);
            _payoutPage.Payout();

            //remove cash
            _payoutPage.CashDrawer.RemoveCash("50", TestData.CashierPassword);

            //payout last voucher
            _payoutPage.NumPad.EnterBarcode(vouchers[2]);
            _payoutPage.Payout();

            //add cash
            _payoutPage.CashDrawer.AddCash("1000", TestData.CashierPassword);

            //remove cash
            _payoutPage.CashDrawer.RemoveCash("700", TestData.CashierPassword);

        }

        private void EndSession()
        {
            _payoutPage.Logout();

            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            NavigationTabs.ClickPayoutTab();

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(CashDrawerStartingBalanceDollars.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();
        }

        private void GenerateReport()
        {
            NavigationTabs.ClickReportsTab();

            string startDate = DateTime.Now.AddDays(-1).ToString("M/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("M/d/yyyy");

            ReportStartDate = startDate;
            ReportEndDate = endDate;

            _reportList.ClickReportByReportName("Cashier Balance");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.RunReport();

            _reportPage.ReportMenu.ExportDropdown.SelectByIndex(1);
            _reportPage.SaveFileWindow.EnterFilepath(TestData.DownloadPath);

            string filename = DateTime.Now.ToString("HHmmssfff") + ".xlsx";
            string filepath = TestData.DownloadPath + @"\" + filename;
            ReportFileName = filename;

            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            
        }

        
        [Test]
        public void CashierBalance_VerifyHeaders()
        {
            string filepath = TestData.DownloadPath + @"\" + ReportFileName;
            bool exists = File.Exists(filepath);

            if (!exists)
            {
                Assert.Fail();
            }

            var reader = new ExcelReader();
            //reader.Open(@"C:\Users\Ben\Downloads\20221107083023.xlsx");
            reader.Open(filepath);
            var report = reader.ParseCashierBalanceReport(includeVouchers: false);

            string title = report.Title;
            string period = report.ReportPeriod;

            Assert.AreEqual("Cashier Balance Report", title);
            Assert.True(period.ToLower().Contains(ReportStartDate.ToLower()));
            Assert.True(period.ToLower().Contains(ReportEndDate.ToLower()));
        }

        [Test]
        public void CashierBalance_VerifySessionCreated()
        {
            string filepath = TestData.DownloadPath + @"\" + ReportFileName;
            bool exists = File.Exists(filepath);

            if (!exists)
            {
                Assert.Fail();
            }

            var reader = new ExcelReader();
            //reader.Open(@"C:\Users\Ben\Downloads\20221107083023.xlsx");
            reader.Open(filepath);
            var report = reader.ParseCashierBalanceReport(includeVouchers: false);

            var session = report.Data.Where(r => r.SessionId == SessionId).FirstOrDefault();
            Assert.NotNull(session);

            Assert.AreEqual(SessionId, session.SessionId);
            Assert.False(string.IsNullOrEmpty(session.StartDate.ToString()));
            Assert.AreNotEqual(default(DateTime), session.EndDate);
        }

        [Test]
        public void CashierBalance_VerifySessionStartingBalance()
        {
            string filepath = TestData.DownloadPath + @"\" + ReportFileName;
            bool exists = File.Exists(filepath);

            if (!exists)
            {
                Assert.Fail();
            }

            var reader = new ExcelReader();
            //reader.Open(@"C:\Users\Ben\Downloads\20221107083023.xlsx");
            reader.Open(filepath);
            var report = reader.ParseCashierBalanceReport(includeVouchers: false);

            var session = report.Data.Where(r => r.SessionId == SessionId).FirstOrDefault();
            Assert.NotNull(session);

            var startBalance = session.StartBalance;
            Assert.AreEqual(CashDrawerStartingBalanceDollars,startBalance);
        }

        [Test]
        public void CashierBalance_VerifySessionPayoutAmount()
        {
            string filepath = TestData.DownloadPath + @"\" + ReportFileName;
            bool exists = File.Exists(filepath);

            if (!exists)
            {
                Assert.Fail();
            }

            var reader = new ExcelReader();
            //reader.Open(@"C:\Users\Ben\Downloads\20221107083023.xlsx");
            reader.Open(filepath);
            var report = reader.ParseCashierBalanceReport(includeVouchers: false);

            var session = report.Data.Where(r => r.SessionId == SessionId).FirstOrDefault();
            Assert.NotNull(session);

            var payoutAmount = session.TotalPayoutAmount;
            Assert.AreEqual(TotalVoucherValue, payoutAmount);
        }

        [Test]
        public void CashierBalance_VerifyTotalAmountAded()
        {
            string filepath = TestData.DownloadPath + @"\" + ReportFileName;
            bool exists = File.Exists(filepath);

            if (!exists)
            {
                Assert.Fail();
            }

            var reader = new ExcelReader();
            //reader.Open(@"C:\Users\Ben\Downloads\20221107083023.xlsx");
            reader.Open(filepath);
            var report = reader.ParseCashierBalanceReport(includeVouchers: false);

            var session = report.Data.Where(r => r.SessionId == SessionId).FirstOrDefault();
            Assert.NotNull(session);

            var addAmount = session.TotalAmountAdded;
            Assert.AreEqual(1400, addAmount);
        }

        [Test]
        public void CashierBalance_VerifyTotalAmountRemoved()
        {
            string filepath = TestData.DownloadPath + @"\" + ReportFileName;
            bool exists = File.Exists(filepath);

            if (!exists)
            {
                Assert.Fail();
            }

            var reader = new ExcelReader();
            //reader.Open(@"C:\Users\Ben\Downloads\20221107083023.xlsx");
            reader.Open(filepath);
            var report = reader.ParseCashierBalanceReport(includeVouchers: false);

            var session = report.Data.Where(r => r.SessionId == SessionId).FirstOrDefault();
            Assert.NotNull(session);

            var removedAmount = session.TotalAmountRemoved;
            Assert.AreEqual(750, removedAmount);
        }

        [Test]
        public void CashierBalance_VerifyEndBalance()
        {
            string filepath = TestData.DownloadPath + @"\" + ReportFileName;
            bool exists = File.Exists(filepath);

            if (!exists)
            {
                Assert.Fail();
            }

            var reader = new ExcelReader();
            //reader.Open(@"C:\Users\Ben\Downloads\20221107083023.xlsx");
            reader.Open(filepath);
            var report = reader.ParseCashierBalanceReport(includeVouchers: false);

            var session = report.Data.Where(r => r.SessionId == SessionId).FirstOrDefault();
            Assert.NotNull(session);

            var endBalance = session.EndBalance;
            Assert.AreEqual(2620, endBalance);
        }

        [Test]
        public void CashierBalance_VerifyVouchers()
        {
            string filepath = TestData.DownloadPath + @"\" + ReportFileName;
            bool exists = File.Exists(filepath);

            if (!exists)
            {
                Assert.Fail();
            }

            var reader = new ExcelReader();
            //reader.Open(@"C:\Users\Ben\Downloads\20221107083023.xlsx");
            reader.Open(filepath);
            var report = reader.ParseCashierBalanceReport(includeVouchers: true);

            var session = report.Data.Where(r => r.SessionId == SessionId).FirstOrDefault();
            Assert.NotNull(session);

            string expectedVoucher = new string('*', 13) + UnpaidBarcode.Substring(13, 5);
            var unpaidVoucher = report.UnpaidVouchers.FirstOrDefault(v => v.VoucherNumber == expectedVoucher && v.Amount == 15);
            
            Assert.NotNull(unpaidVoucher);
            Assert.AreEqual(15,unpaidVoucher.Amount);
        }

        [Test]
        public void CashierBalance_VerifySessionTotals()
        {
            string filepath = TestData.DownloadPath + @"\" + ReportFileName;
            bool exists = File.Exists(filepath);

            if (!exists)
            {
                Assert.Fail();
            }

            var reader = new ExcelReader();
            //reader.Open(@"C:\Users\Ben\Downloads\20221107083023.xlsx");
            reader.Open(filepath);
            var report = reader.ParseCashierBalanceReport(includeVouchers: false);

            var sessions = report.Data;
            decimal startBalanceCount = 0;
            decimal payoutAmountCount = 0;
            decimal removedAmountCount = 0;
            decimal addAmountCount = 0;
            decimal endBalanceCount = 0;

            foreach(var session in sessions)
            {
                startBalanceCount += session.StartBalance;
                payoutAmountCount += session.TotalPayoutAmount;
                addAmountCount += session.TotalAmountAdded;
                removedAmountCount += session.TotalAmountRemoved;
                endBalanceCount += session.EndBalance;
            }

            Assert.AreEqual(startBalanceCount,report.TotalStartingBalance);
            Assert.AreEqual(payoutAmountCount,report.TotalPayoutAmount);
            Assert.AreEqual(addAmountCount,report.TotalAmountAdded);
            Assert.AreEqual(removedAmountCount,report.TotalAmountRemoved);
            Assert.AreEqual(endBalanceCount,report.TotalEndBalance);
        }

        [Test]
        public void CashierBalance_VerifyUnpaidVoucherTotal()
        {
            string filepath = TestData.DownloadPath + @"\" + ReportFileName;
            bool exists = File.Exists(filepath);

            if (!exists)
            {
                Assert.Fail();
            }

            var reader = new ExcelReader();
            //reader.Open(@"C:\Users\Ben\Downloads\20221107083023.xlsx");
            reader.Open(filepath);
            var report = reader.ParseCashierBalanceReport(includeVouchers: true);

            var vouchers = report.UnpaidVouchers;
            decimal amount = 0;

            foreach(var voucher in vouchers)
            {
                amount += voucher.Amount;
            }

            Assert.AreEqual(amount,report.TotalUnpaidVoucherAmount);
        }

        [Test]
        public void EmptyReport()
        {

            if (_loginPage.IsLoggedIn)
            {
                _payoutPage.NavigationTabs.ClickReportsTab();
                _reportPage.ReportMenu.Back();
                _reportList.ClickReportByReportName("Cashier Balance");
            }
            else
            {
                _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
                 NavigationTabs.ClickPayoutTab();
                _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(CashDrawerStartingBalanceDollars.ToString());
                _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();
                NavigationTabs.ClickReportsTab();
                _reportList.ClickReportByReportName("Cashier Balance");
            }

            string startDate = DateTime.Now.AddDays(3).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(4).ToString("MM/d/yyyy");
            
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

            bool exists = File.Exists(fullPath);
            while (!exists)
            {
                Thread.Sleep(1000);
                exists = File.Exists(fullPath);

                if (exists)
                {
                    break;
                }
            }

            var reader = new ExcelReader();
            //reader.Open(@"C:\Users\Ben\Downloads\20221107083023.xlsx");
            reader.Open(fullPath);
            var report = reader.ParseCashierBalanceReport(includeVouchers: false);

            Assert.AreEqual("Cashier Balance Report", report.Title);
            Assert.True(report.ReportPeriod.ToLower().Contains(startDate.ToLower()));
            Assert.True(report.ReportPeriod.ToLower().Contains(endDate.ToLower()));
            Assert.Zero(report.Data.Count);
            Assert.Zero(report.UnpaidVouchers.Count);
        }

        [Test]
        public void CashierBalance_VerifySessionPeriods()
        {
            string filepath = TestData.DownloadPath + @"\" + ReportFileName;
            bool exists = File.Exists(filepath);

            if (!exists)
            {
                Assert.Fail();
            }

            var reader = new ExcelReader();
            //reader.Open(@"C:\Users\Ben\Downloads\20221107083023.xlsx");
            reader.Open(filepath);
            var report = reader.ParseCashierBalanceReport(includeVouchers: false);

            var sessions = report.Data;
            var reportStartDate = DateTime.ParseExact(ReportStartDate, "M/d/yyyy",CultureInfo.InvariantCulture);
            var reportEndDate = DateTime.ParseExact(ReportEndDate, "M/d/yyyy", CultureInfo.InvariantCulture);

            foreach(var sesh in sessions)
            {
                if(sesh.StartDate != default(DateTime))
                {
                    var startDateString = sesh.StartDate.ToString("M/d/yyyy");
                    var startDate = DateTime.ParseExact(startDateString, "M/d/yyyy", CultureInfo.InvariantCulture);
                    
                    Assert.True((startDate >= reportStartDate));
                }

                if(sesh.EndDate != default(DateTime))
                {
                    var endDateString = sesh.EndDate.ToString("M/d/yyyy");
                    var endDate = DateTime.ParseExact(endDateString, "M/d/yyyy", CultureInfo.InvariantCulture);
                    
                    Assert.True(endDate <= reportEndDate);
                }
            }
        }


        [Test]
        public void CashierBalance_CashRemoved_Drop()
        {

            string filepath = TestData.DownloadPath + @"\" + ReportFileName;
            bool exists = File.Exists(filepath);

            if (!exists)
            {
                Assert.Fail();
            }

            var reader = new ExcelReader();
            //reader.Open(@"C:\Users\Ben\Downloads\20221107083023.xlsx");
            reader.Open(filepath);
            var report = reader.ParseCashierBalanceReport(includeVouchers: false);

            var drops = report.CashDrops;
            Assert.Greater(drops.Count, 0);

            decimal expectedTotal = 286;
            var targetDrop = drops.LastOrDefault(d => d.MachineNumber == TestData.DefaultMachineNumber && d.CashRemoved == expectedTotal);
            

            Assert.NotNull(targetDrop);
            Assert.AreEqual(TestData.DefaultMachineNumber, targetDrop.MachineNumber);
            Assert.AreEqual(expectedTotal, targetDrop.CashRemoved);

            //verify total cashdrop value
            decimal dropCount = 0;
            foreach(var d in drops)
            {
                dropCount += d.CashRemoved;
            }

            Assert.AreEqual(dropCount,report.TotalDropAmount);
        }
    }
}