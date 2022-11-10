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

namespace POS_Automation
{
    public class CashierActivityReportTests : BaseTest
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

        //Verify an error is displayed to the user if the scanned barcode is already in the transaction
        [Test]
        public void PayoutTest_VoucherAlreadyInTransaction()
        {

            var reader = new ExcelReader();
            //reader.Open(@"C:\Users\Ben\Downloads\20221107083023.xlsx");
            reader.Open(@"C:\Users\bdagg\Downloads\Daily Cashier Activityall.xlsx");
            var report = reader.ParseCashierActivityReport();
            Console.WriteLine("title = " + report.Title);
            Console.WriteLine("ran at " + report.RunDate);
            Console.WriteLine("period = " + report.ReportPeriod);

            foreach(var record in report.Data)
            {
                Console.WriteLine(record.CreatedBy);
                foreach(var activity in record.Activities)
                {
                    Console.WriteLine($"{activity.CreatedBy},{activity.SessionId},{activity.Station},{activity.VoucherNumber},{activity.PayoutAmount},{activity.ReceiptNumber},{activity.Date}");
                }
                Console.WriteLine($"vouchers:{record.TotalVouchers}, amount:{record.TotalAmount},trans:{record.TotalTransactions}");
            }
        }

        [Test]
        public void CashBankActivityTest()
        {

            var reader = new ExcelReader();
            //reader.Open(@"C:\Users\Ben\Downloads\20221107083023.xlsx");
            reader.Open(@"C:\Users\bdagg\Downloads\Cash Bank Activityold.xlsx");
            var report = reader.ParseCashBankActivityReport();

            Console.WriteLine("Title: " + report.Title);
            Console.WriteLine("Run Time: " + report.RunDate);
            Console.WriteLine("Period: " + report.ReportPeriod);

            foreach(var cashier in report.Data)
            {
                Console.WriteLine("===================");
                Console.WriteLine(cashier.CreatedBy);

                foreach(var session in cashier.Sessions)
                {
                    Console.WriteLine(" " + session.SessionId);

                    foreach(var trans in session.Transactions)
                    {
                        Console.WriteLine("  " + $"{trans.Station},{trans.VoucherNumber},{trans.ReferenceNumber},{trans.TransType},{trans.Money},{trans.Payout},{trans.Date}");
                    }

                    Console.WriteLine(" Total: " + session.TotalMoney + ", " + session.TotalPayout);
                }

                Console.WriteLine("User Totals: " + cashier.TotalMoney + ", " + cashier.TotalPayout);
            }
        }

        [Test]
        public void TestOpenReport()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickReportsTab();

            _reportList.ClickReportByReportName("Daily Cashier Activity");
            _reportPage.ReportMenu.EnterStartDate("11/1/2022");
            _reportPage.ReportMenu.EnterEndDate("11/9/2022");
            _reportPage.ReportMenu.RunReport();

            _reportPage.ReportMenu.ExportDropdown.SelectByIndex(1);
            _reportPage.SaveFileWindow.EnterFilepath(TestData.DownloadPath);

            string filename = DateTime.Now.ToString("HHmmssfff") + ".xlsx";
            string filepath = TestData.DownloadPath + @"\" + filename;
            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            Assert.True(_reportPage.SaveFileWindow.FileDownloaded(filepath));

            var reader = new ExcelReader();
            //reader.Open(@"C:\Users\Ben\Downloads\20221107083023.xlsx");
            reader.Open(TestData.DownloadPath + @"\" + filename);
            var report = reader.ParseCashierActivityReport();
            Console.WriteLine("title = " + report.Title);
            Console.WriteLine("ran at " + report.RunDate);
            Console.WriteLine("period = " + report.ReportPeriod);

            foreach (var record in report.Data)
            {
                Console.WriteLine(record.CreatedBy);
                foreach (var activity in record.Activities)
                {
                    Console.WriteLine($"{activity.CreatedBy},{activity.SessionId},{activity.Station},{activity.VoucherNumber},{activity.PayoutAmount},{activity.ReceiptNumber},{activity.Date}");
                }
                Console.WriteLine($"vouchers:{record.TotalVouchers}, amount:{record.TotalAmount},trans:{record.TotalTransactions}");
            }
        }

        [Test]
        public void CashierActivityReport_PayoutVoucher()
        {
            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);
            
            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);
            _payoutPage.Payout();

            var session = _transRepo.GetCurrentUserSession(TestData.SuperUserUsername);

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");
            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Daily Cashier Activity");
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

            Assert.True(_reportPage.SaveFileWindow.FileDownloaded(fullPath));

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashierActivityReport();

            var records = report.GetRecordsBySessionsId(session);
            string expectedVoucher = new string('*', 14) + barcode.Substring(14, 4);

            var activity = records.SingleOrDefault(r => r.VoucherNumber == expectedVoucher);
            Assert.NotNull(activity);

            Assert.AreEqual(expectedVoucher, activity.VoucherNumber);
            Assert.AreEqual(activity.PayoutAmount, 5);
            
        }

        [Test]
        public void CashierActivityReport_MultiplePayouts()
        {
            //create a voucher for $100
            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;

            var barcode2 = TpService.GetVoucher(StartingAmountCredits, 1000);
            StartingAmountCredits -= 1000;

            var barcode3 = TpService.GetVoucher(StartingAmountCredits, 1500);
            StartingAmountCredits -= 1500;

            var vouchers = new List<string>() { barcode1,barcode2,barcode3};

            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            foreach(var barcode in vouchers)
            {
                _payoutPage.NumPad.EnterBarcode(barcode);
            }
            _payoutPage.Payout();

            var session = _transRepo.GetCurrentUserSession(TestData.SuperUserUsername);

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");
            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Daily Cashier Activity");
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

            Assert.True(_reportPage.SaveFileWindow.FileDownloaded(fullPath));

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashierActivityReport();

            var records = report.GetRecordsBySessionsId(session);
            Assert.AreEqual(vouchers.Count,records.Count);

            string expectedVoucher1 = new string('*', 14) + vouchers[0].Substring(14, 4);
            string expectedVoucher2 = new string('*', 14) + vouchers[1].Substring(14, 4);
            string expectedVoucher3 = new string('*', 14) + vouchers[2].Substring(14, 4);

            Assert.True(records.Any(r => r.VoucherNumber == expectedVoucher1 && r.PayoutAmount == 5));
            Assert.True(records.Any(r => r.VoucherNumber == expectedVoucher2 && r.PayoutAmount == 10));
            Assert.True(records.Any(r => r.VoucherNumber == expectedVoucher3 && r.PayoutAmount == 15));
        }

        [Test]
        public void CashierActivityReport_VerifySession()
        {
            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);
            _payoutPage.Payout();

            var session = _transRepo.GetCurrentUserSession(TestData.SuperUserUsername);

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");
            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Daily Cashier Activity");
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

            Assert.True(_reportPage.SaveFileWindow.FileDownloaded(fullPath));

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashierActivityReport();

            string expectedVoucher = new string('*', 14) + barcode.Substring(14, 4);
            string actualSession = report.GetSessionByVoucher(expectedVoucher);

            Assert.AreEqual(session, actualSession);
        }

        [Test]
        public void CashierActivityReport_TotalNumberVouchers()
        {
            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);
            _payoutPage.Payout();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");
            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Daily Cashier Activity");
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

            Assert.True(_reportPage.SaveFileWindow.FileDownloaded(fullPath));

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashierActivityReport();

            int actualTotalVouchers = report.TotalVouchers;
            var records = report.Data;

            int count = 0;
            foreach(var record in records)
            {
                count += record.Activities.Count;
            }

            Assert.AreEqual(count, actualTotalVouchers);
        }

        [Test]
        public void CashierActivityReport_TotalPayoutAmount()
        {
            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);
            _payoutPage.Payout();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");
            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Daily Cashier Activity");
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

            Assert.True(_reportPage.SaveFileWindow.FileDownloaded(fullPath));

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashierActivityReport();

            decimal actualTotalPayout = report.TotalAmount;
            var records = report.Data;

            decimal count = 0;
            foreach (var record in records)
            {
                foreach(var trans in record.Activities)
                {
                    count += trans.PayoutAmount;
                }
            }

            Assert.AreEqual(count, actualTotalPayout);
        }

        [Test]
        public void CashierActivityReport_TotalTransactions()
        {
            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);
            _payoutPage.Payout();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");
            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Daily Cashier Activity");
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

            Assert.True(_reportPage.SaveFileWindow.FileDownloaded(fullPath));

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashierActivityReport();

            decimal actualTotalTransactions = report.TotalTransactions;
            var records = report.Data;

            var distinct = new List<string>();

            foreach(var record in records)
            {
                foreach(var trans in record.Activities)
                {
                    string session = trans.SessionId;
                    if (!distinct.Contains(session))
                    {
                        distinct.Add(session);
                    }
                }
            }

            Assert.AreEqual(distinct.Count,actualTotalTransactions);
        }

        [Test]
        public void CashierActivityReport_MultipleCashiers()
        {
            //user 1 pays out a voucher
            //create a voucher for $100
            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode1);
            _payoutPage.Payout();

            _payoutPage.Logout();

            //user 2 pays out a voucher
            //create a voucher for $100
            var barcode2 = TpService.GetVoucher(StartingAmountCredits, 1000);

            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            NavigationTabs.ClickPayoutTab();

            startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode2);
            _payoutPage.Payout();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");
            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Daily Cashier Activity");
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

            Assert.True(_reportPage.SaveFileWindow.FileDownloaded(fullPath));

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashierActivityReport();

            var records = report.Data;

            Assert.True(records.Count >= 2);
            Assert.True(records.Any(r => r.CreatedBy == TestData.SuperUserUsername));
            Assert.True(records.Any(r => r.CreatedBy == TestData.CashierUsername));

            var cashierRecords = records.Where(r => r.CreatedBy == TestData.CashierUsername).FirstOrDefault();
            var cashierTransactions = cashierRecords.Activities;
            string expectedVoucher = new string('*', 14) + barcode1.Substring(14, 4);
            Assert.True(cashierTransactions.Any(r => r.VoucherNumber == expectedVoucher && r.PayoutAmount == 5));

            var cashier2Records = records.Where(r => r.CreatedBy == TestData.SuperUserUsername).FirstOrDefault();
            var cashier2Transactions = cashier2Records.Activities;
            string expectedVoucher2 = new string('*', 14) + barcode2.Substring(14, 4);
            Assert.True(cashier2Transactions.Any(r => r.VoucherNumber == expectedVoucher2 && r.PayoutAmount == 10));
        }

        [Test]
        public void CashierActivityReport_UserTotal()
        {
            //user 1 pays out a voucher
            //create a voucher for $100
            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode1);
            _payoutPage.Payout();

            _payoutPage.Logout();

            //user 2 pays out a voucher
            //create a voucher for $100
            var barcode2 = TpService.GetVoucher(StartingAmountCredits, 1000);

            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            NavigationTabs.ClickPayoutTab();

            startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode2);
            _payoutPage.Payout();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");
            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Daily Cashier Activity");
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

            Assert.True(_reportPage.SaveFileWindow.FileDownloaded(fullPath));

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashierActivityReport();

            var records = report.Data;
            var cashierRecords = records.Where(r => r.CreatedBy == TestData.CashierUsername).SingleOrDefault();

            int cashVoucherCount = cashierRecords.Activities.Count;
            decimal cashPayoutCount = 0;
            var cashUniqueUserSessions = new List<string>();

            foreach(var trans in cashierRecords.Activities)
            {
                cashPayoutCount += trans.PayoutAmount;
                string session = trans.SessionId;

                if (!cashUniqueUserSessions.Contains(session))
                {
                    cashUniqueUserSessions.Add(session);
                }
            }

            Assert.AreEqual(cashierRecords.TotalVouchers, cashVoucherCount);
            Assert.AreEqual(cashierRecords.TotalAmount, cashPayoutCount);
            Assert.AreEqual(cashierRecords.TotalTransactions, cashUniqueUserSessions.Count);

            var user2Records = records.Where(r => r.CreatedBy == TestData.SuperUserUsername).SingleOrDefault();
            int userVoucherCount = user2Records.Activities.Count;
            decimal user2PayoutCount = 0;
            var user2UniqueUserSessions = new List<string>();

            foreach (var trans in user2Records.Activities)
            {
                user2PayoutCount += trans.PayoutAmount;
                string session = trans.SessionId;

                if (!user2UniqueUserSessions.Contains(session))
                {
                    user2UniqueUserSessions.Add(session);
                }
            }

            Assert.AreEqual(user2Records.TotalVouchers, userVoucherCount);
            Assert.AreEqual(user2Records.TotalAmount, user2PayoutCount);
            Assert.AreEqual(user2Records.TotalTransactions, user2UniqueUserSessions.Count);
        }

        [Test]
        public void CashierActivityReport_VerifyHeader()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickReportsTab();

            string startDate = DateTime.Now.AddDays(2).ToString("MM/dd/yyyy");
            string endDate = DateTime.Now.AddDays(3).ToString("MM/dd/yyyy");
            _reportList.ClickReportByReportName("Daily Cashier Activity");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.RunReport();

            _reportPage.ReportMenu.ExportDropdown.SelectByIndex(1);
            _reportPage.SaveFileWindow.EnterFilepath(TestData.DownloadPath);

            string filename = DateTime.Now.ToString("HHmmssfff") + ".xlsx";
            string filepath = TestData.DownloadPath = @"\" + filename;
            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            Assert.True(_reportPage.SaveFileWindow.FileDownloaded(filepath));

            var reader = new ExcelReader();
            //reader.Open(@"C:\Users\Ben\Downloads\20221107083023.xlsx");
            reader.Open(TestData.DownloadPath + @"\" + filename);
            var report = reader.ParseCashierActivityReport();

            string title = report.Title;
            string period = report.ReportPeriod;
            string expectedPeriod = "Reporting Period: " + startDate + " to " + endDate + " 11:59:59 PM";

            Assert.AreEqual("Daily Cashier Activity",title);
            Assert.AreEqual(expectedPeriod,period);
        }
    }
}