﻿using NUnit.Framework;
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
    public class CashBankActivityReportTests : BaseTest
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
        public void CashBankActivityTest()
        {

            var reader = new ExcelReader();
            //reader.Open(@"C:\Users\Ben\Downloads\20221107083023.xlsx");
            reader.Open(@"C:\Users\bdagg\Downloads\Cash Bank Activityold.xlsx");
            var report = reader.ParseCashBankActivityReport();

            Console.WriteLine("Title: " + report.Title);
            Console.WriteLine("Run Time: " + report.RunDate);
            Console.WriteLine("Period: " + report.ReportPeriod);

            foreach (var cashier in report.Data)
            {
                Console.WriteLine("===================");
                Console.WriteLine(cashier.CreatedBy);

                foreach (var session in cashier.Sessions)
                {
                    Console.WriteLine(" " + session.SessionId);

                    foreach (var trans in session.Transactions)
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
            _reportPage.SaveFileWindow.EnterFilepath(@"C:\Users\bdagg\Downloads");

            string filename = DateTime.Now.ToString("HHmmssfff") + ".xlsx";
            string filepath = @"C:\Users\bdagg\Downloads\" + filename;
            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            Assert.True(_reportPage.SaveFileWindow.FileDownloaded(filepath));

            var reader = new ExcelReader();
            //reader.Open(@"C:\Users\Ben\Downloads\20221107083023.xlsx");
            reader.Open(@"C:\Users\bdagg\Downloads\" + filename);
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
        public void CashBankActivityReport_PayoutVoucher()
        {
            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;
            var sessionId = _transRepo.GetCurrentUserSession(TestData.CashierUsername);

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);
            _payoutPage.Payout();
            _payoutPage.NavigationTabs.ClickDeviceTab();

            _payoutPage.Logout();
            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");
            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Cash Bank Activity");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.RunReport();

            string filename = DateTime.Now.ToString("HHmmssfff") + ".xlsx";
            string filepath = @"C:\Users\bdagg\Downloads";
            string fullPath = filepath + @"\" + filename;

            _reportPage.ReportMenu.ExportDropdown.SelectByIndex(1);
            _reportPage.SaveFileWindow.EnterFilepath(filepath);
            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            Assert.True(_reportPage.SaveFileWindow.FileDownloaded(fullPath));

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashBankActivityReport();

            var session = report.GetSession(sessionId);
            Assert.Greater(session.Transactions.Count, 0);

            var startTrans = session.Transactions.FirstOrDefault(t => t.TransType == "Start");
            Assert.NotNull(startTrans);
            decimal sessionStartBalance = startTrans.Money;

            var payoutTrans = session.Transactions.FirstOrDefault(t => t.TransType == "Payout");
            Assert.NotNull(payoutTrans);
            Assert.AreEqual(payoutTrans.Payout, 5);
            Assert.AreEqual(payoutTrans.VoucherNumber, barcode);

            var endTrans = session.Transactions.FirstOrDefault(t => t.TransType == "End");
            Assert.NotNull(endTrans);
            decimal endBalance = endTrans.Money;

            Assert.AreEqual(sessionStartBalance - payoutTrans.Payout, endBalance);
            Assert.AreEqual(endBalance,session.TotalMoney);
        }

        [Test]
        public void CashBankActivityReport_PayoutMultipleVoucher()
        {

            //create a voucher for $100
            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;

            var barcode2 = TpService.GetVoucher(StartingAmountCredits, 1000);
            StartingAmountCredits -= 1000;

            var barcode3 = TpService.GetVoucher(StartingAmountCredits, 1500);
            StartingAmountCredits -= 1500;

            var vouchers = new List<string>() { barcode1, barcode2, barcode3 };

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;
            var sessionId = _transRepo.GetCurrentUserSession(TestData.CashierUsername);

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            foreach(var barcode in vouchers)
            {
                _payoutPage.NumPad.EnterBarcode(barcode);
            }
            _payoutPage.Payout();
            _payoutPage.NavigationTabs.ClickDeviceTab();

            _payoutPage.Logout();
            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");
            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Cash Bank Activity");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.RunReport();

            string filename = DateTime.Now.ToString("HHmmssfff") + ".xlsx";
            string filepath = @"C:\Users\bdagg\Downloads";
            string fullPath = filepath + @"\" + filename;

            _reportPage.ReportMenu.ExportDropdown.SelectByIndex(1);
            _reportPage.SaveFileWindow.EnterFilepath(filepath);
            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            Assert.True(_reportPage.SaveFileWindow.FileDownloaded(fullPath));

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashBankActivityReport();

            var session = report.GetSession(sessionId);
            Assert.Greater(session.Transactions.Count, 0);

            var startTrans = session.Transactions.FirstOrDefault(t => t.TransType == "Start");
            Assert.NotNull(startTrans);
            decimal sessionStartBalance = startTrans.Money;
            Assert.AreEqual(1000,sessionStartBalance);

            var payoutTrans = session.Transactions.Where(t => t.TransType == "Payout").ToList();
            Assert.AreEqual(3, payoutTrans.Count);

            foreach(var g in vouchers)
            {
                Console.WriteLine(g);
            }
            foreach(var t in payoutTrans)
            {
                Console.WriteLine(t.VoucherNumber + "," + t.Payout);
            }
            Assert.True(payoutTrans.Any(t => t.Payout == 5));
            Assert.True(payoutTrans.Any(t => t.Payout == 10));
            Assert.True(payoutTrans.Any(t => t.Payout == 15));

            var endTrans = session.Transactions.FirstOrDefault(t => t.TransType == "End");
            Assert.NotNull(endTrans);
            decimal endBalance = endTrans.Money;

            Assert.AreEqual(sessionStartBalance - 30, endBalance);
            Assert.AreEqual(endBalance, session.TotalMoney);
        }

        [Test]
        public void CashBankActivityReport_AddCash()
        {

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;
            var sessionId = _transRepo.GetCurrentUserSession(TestData.CashierUsername);

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.CashDrawer.AddCash("100", TestData.CashierPassword);
            _payoutPage.NavigationTabs.ClickDeviceTab();

            _payoutPage.Logout();
            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");
            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Cash Bank Activity");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.RunReport();

            string filename = DateTime.Now.ToString("HHmmssfff") + ".xlsx";
            string filepath = @"C:\Users\bdagg\Downloads";
            string fullPath = filepath + @"\" + filename;

            _reportPage.ReportMenu.ExportDropdown.SelectByIndex(1);
            _reportPage.SaveFileWindow.EnterFilepath(filepath);
            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            Assert.True(_reportPage.SaveFileWindow.FileDownloaded(fullPath));

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashBankActivityReport();

            var session = report.GetSession(sessionId);
            Assert.Greater(session.Transactions.Count, 0);

            var startTrans = session.Transactions.FirstOrDefault(t => t.TransType == "Start");
            Assert.NotNull(startTrans);
            decimal sessionStartBalance = startTrans.Money;
            Assert.AreEqual(1000, sessionStartBalance);

            var addTrans = session.Transactions.FirstOrDefault(t => t.TransType == "Add");
            Assert.NotNull(addTrans);
            Assert.AreEqual(100,addTrans.Money);

            var endTrans = session.Transactions.FirstOrDefault(t => t.TransType == "End");
            Assert.NotNull(endTrans);
            decimal endBalance = endTrans.Money;

            Assert.AreEqual(1100, endBalance);
            Assert.AreEqual(endBalance,session.TotalMoney);
        }

        [Test]
        public void CashBankActivityReport_RemoveCash()
        {

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;
            

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();
            var sessionId = _transRepo.GetCurrentUserSession(TestData.CashierUsername);

            _payoutPage.CashDrawer.RemoveCash("100", TestData.CashierPassword);
            _payoutPage.NavigationTabs.ClickDeviceTab();

            _payoutPage.Logout();
            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");
            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Cash Bank Activity");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.RunReport();

            string filename = DateTime.Now.ToString("HHmmssfff") + ".xlsx";
            string filepath = @"C:\Users\bdagg\Downloads";
            string fullPath = filepath + @"\" + filename;

            _reportPage.ReportMenu.ExportDropdown.SelectByIndex(1);
            _reportPage.SaveFileWindow.EnterFilepath(filepath);
            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            Assert.True(_reportPage.SaveFileWindow.FileDownloaded(fullPath));

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashBankActivityReport();

            var session = report.GetSession(sessionId);
            Assert.AreEqual(session.Transactions.Count, 3);

            var startTrans = session.Transactions.FirstOrDefault(t => t.TransType == "Start");
            Assert.NotNull(startTrans);
            decimal sessionStartBalance = startTrans.Money;
            Assert.AreEqual(1000, sessionStartBalance);

            var removeTrans = session.Transactions.FirstOrDefault(t => t.TransType == "Remove");
            Assert.NotNull(removeTrans);
            Assert.AreEqual(100, removeTrans.Money);

            var endTrans = session.Transactions.FirstOrDefault(t => t.TransType == "End");
            Assert.NotNull(endTrans);
            decimal endBalance = endTrans.Money;

            Assert.AreEqual(900, endBalance);
            Assert.AreEqual(endBalance, session.TotalMoney);
        }

        [Test]
        public void CashBankActivityReport_MultipleTransaction()
        {

            //create a voucher for $100
            var barcode1 = TpService.GetVoucher(StartingAmountCredits, 500);
            StartingAmountCredits -= 500;

            var barcode2 = TpService.GetVoucher(StartingAmountCredits, 1000);
            StartingAmountCredits -= 1000;

            var barcode3 = TpService.GetVoucher(StartingAmountCredits, 1500);
            StartingAmountCredits -= 1500;

            var vouchers = new List<string>() { barcode1, barcode2, barcode3 };

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;
           
            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();
            var sessionId = _transRepo.GetCurrentUserSession(TestData.CashierUsername);

            //perform transactions
            _payoutPage.NumPad.EnterBarcode(barcode1);
            _payoutPage.Payout();
            _payoutPage.CashDrawer.AddCash("500", TestData.CashierPassword);
            _payoutPage.NumPad.EnterBarcode(barcode2);
            _payoutPage.Payout();
            _payoutPage.CashDrawer.RemoveCash("200",TestData.CashierPassword);
            _payoutPage.NumPad.EnterBarcode(barcode3);
            _payoutPage.Payout();

            _payoutPage.NavigationTabs.ClickDeviceTab();

            _payoutPage.Logout();
            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");
            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Cash Bank Activity");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.RunReport();

            string filename = DateTime.Now.ToString("HHmmssfff") + ".xlsx";
            string filepath = @"C:\Users\bdagg\Downloads";
            string fullPath = filepath + @"\" + filename;

            _reportPage.ReportMenu.ExportDropdown.SelectByIndex(1);
            _reportPage.SaveFileWindow.EnterFilepath(filepath);
            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            Assert.True(_reportPage.SaveFileWindow.FileDownloaded(fullPath));

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashBankActivityReport();

            var session = report.GetSession(sessionId);
            Assert.Greater(session.Transactions.Count, 0);

            var startTrans = session.Transactions.FirstOrDefault(t => t.TransType == "Start");
            Assert.NotNull(startTrans);
            decimal sessionStartBalance = startTrans.Money;
            Assert.AreEqual(1000, sessionStartBalance);

            var payoutTrans = session.Transactions.Where(t => t.TransType == "Payout").ToList();
            Assert.AreEqual(3, payoutTrans.Count);

            Assert.True(payoutTrans.Any(t => t.Payout == 5));
            Assert.True(payoutTrans.Any(t => t.Payout == 10));
            Assert.True(payoutTrans.Any(t => t.Payout == 15));

            var addCashTrans = session.Transactions.Where(t => t.TransType == "Add").ToList();
            Assert.AreEqual(1,addCashTrans.Count);
            Assert.AreEqual(500, addCashTrans[0].Money);

            var removeCashTrans = session.Transactions.Where(t => t.TransType == "Remove").ToList();
            Assert.AreEqual(1,removeCashTrans.Count);
            Assert.AreEqual(200, removeCashTrans[0].Money);

            var endTrans = session.Transactions.FirstOrDefault(t => t.TransType == "End");
            Assert.NotNull(endTrans);
            decimal endBalance = endTrans.Money;

            Assert.AreEqual(1270, endBalance);
            Assert.AreEqual(endBalance, session.TotalMoney);
        }

        [Test]
        public void CashBankActivityReport_MultipleTransactions()
        {
            //create a voucher for $100
            var barcode = TpService.GetVoucher(StartingAmountCredits, 500);

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;
            var sessionId = _transRepo.GetCurrentUserSession(TestData.CashierUsername);

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.NumPad.EnterBarcode(barcode);
            _payoutPage.Payout();
            _payoutPage.NavigationTabs.ClickDeviceTab();

            _payoutPage.Logout();
            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");
            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Cash Bank Activity");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.RunReport();

            string filename = DateTime.Now.ToString("HHmmssfff") + ".xlsx";
            string filepath = @"C:\Users\bdagg\Downloads";
            string fullPath = filepath + @"\" + filename;

            _reportPage.ReportMenu.ExportDropdown.SelectByIndex(1);
            _reportPage.SaveFileWindow.EnterFilepath(filepath);
            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            Assert.True(_reportPage.SaveFileWindow.FileDownloaded(fullPath));

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashBankActivityReport();

            var session = report.GetSession(sessionId);
            Assert.Greater(session.Transactions.Count, 0);

            var startTrans = session.Transactions.FirstOrDefault(t => t.TransType == "Start");
            Assert.NotNull(startTrans);
            decimal sessionStartBalance = startTrans.Money;

            var payoutTrans = session.Transactions.FirstOrDefault(t => t.TransType == "Payout");
            Assert.NotNull(payoutTrans);
            Assert.AreEqual(payoutTrans.Payout, 5);
            Assert.AreEqual(payoutTrans.VoucherNumber, barcode);

            var endTrans = session.Transactions.FirstOrDefault(t => t.TransType == "End");
            Assert.NotNull(endTrans);
            decimal endBalance = endTrans.Money;

            Assert.AreEqual(sessionStartBalance - payoutTrans.Payout, endBalance);
            Assert.AreEqual(endBalance, session.TotalMoney);
        }

        [Test]
        public void CashBankActivityReport_ReportTotals()
        {

            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;
            
            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var sessionId = _transRepo.GetCurrentUserSession(TestData.SuperUserUsername);

            _payoutPage.CashDrawer.AddCash("100", TestData.SuperUserPassword);
            _payoutPage.NavigationTabs.ClickDeviceTab();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");
            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Cash Bank Activity");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.RunReport();

            string filename = DateTime.Now.ToString("HHmmssfff") + ".xlsx";
            string filepath = @"C:\Users\bdagg\Downloads";
            string fullPath = filepath + @"\" + filename;

            _reportPage.ReportMenu.ExportDropdown.SelectByIndex(1);
            _reportPage.SaveFileWindow.EnterFilepath(filepath);
            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            Assert.True(_reportPage.SaveFileWindow.FileDownloaded(fullPath));

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashBankActivityReport();

            decimal totalMoney = 0;
            decimal totalPayout = 0;
            foreach(var cashier in report.Data)
            {
                foreach(var session in cashier.Sessions)
                {
                    totalMoney += session.TotalMoney;

                    var transactions = session.Transactions;
                    foreach(var trans in transactions)
                    {
                        if (trans.TransType == "Payout")
                        {
                            totalPayout += trans.Payout;
                        }
                    }
                }
            }

            Assert.AreEqual(report.TotalMoney, totalMoney);
            Assert.AreEqual(report.TotalPayout, totalPayout);
        }

        [Test]
        public void CashBankActivityReport_UserTotals()
        {

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var sessionId = _transRepo.GetCurrentUserSession(TestData.CashierUsername);

            _payoutPage.CashDrawer.AddCash("100", TestData.CashierPassword);
            _payoutPage.NavigationTabs.ClickDeviceTab();
            _payoutPage.Logout();

            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            NavigationTabs.ClickPayoutTab();

            startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            _payoutPage.CashDrawer.AddCash("100", TestData.SuperUserPassword);
            _payoutPage.NavigationTabs.ClickDeviceTab();
            _payoutPage.NavigationTabs.ClickReportsTab();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");
            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Cash Bank Activity");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.RunReport();

            string filename = DateTime.Now.ToString("HHmmssfff") + ".xlsx";
            string filepath = @"C:\Users\bdagg\Downloads";
            string fullPath = filepath + @"\" + filename;

            _reportPage.ReportMenu.ExportDropdown.SelectByIndex(1);
            _reportPage.SaveFileWindow.EnterFilepath(filepath);
            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            Assert.True(_reportPage.SaveFileWindow.FileDownloaded(fullPath));

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashBankActivityReport();

            var cashierData = report.Data.SingleOrDefault(r => r.CreatedBy == TestData.CashierUsername);

            decimal totalMoney = 0;
            decimal totalPayout = 0;
            foreach (var session in cashierData.Sessions)
            {
                totalMoney += session.TotalMoney;

                var transactions = session.Transactions;
                foreach (var trans in transactions)
                {
                    if (trans.TransType == "Payout")
                    {
                        totalPayout += trans.Payout;
                    }
                }
            }

            Assert.AreEqual(cashierData.TotalMoney, totalMoney);
            Assert.AreEqual(cashierData.TotalPayout, totalPayout);
        }

        [Test]
        public void CashBankActivityReport_SessionNotEnded()
        {

            _loginPage.Login(TestData.CashierUsername, TestData.CashierPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            var sessionId = _transRepo.GetCurrentUserSession(TestData.CashierUsername);

            _payoutPage.Logout();

            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);

            startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();
            _payoutPage.NavigationTabs.ClickReportsTab();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");
            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Cash Bank Activity");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.RunReport();

            string filename = DateTime.Now.ToString("HHmmssfff") + ".xlsx";
            string filepath = @"C:\Users\bdagg\Downloads";
            string fullPath = filepath + @"\" + filename;

            _reportPage.ReportMenu.ExportDropdown.SelectByIndex(1);
            _reportPage.SaveFileWindow.EnterFilepath(filepath);
            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            Assert.True(_reportPage.SaveFileWindow.FileDownloaded(fullPath));

            ExcelReader reader = new ExcelReader();
            reader.Open(fullPath);
            var report = reader.ParseCashBankActivityReport();

            var session = report.GetSession(sessionId);
            Assert.AreEqual(2, session.Transactions.Count);

            Assert.True(session.Transactions.Any(t => t.TransType == "Start"));
            Assert.True(session.Transactions.Any(t => t.TransType == "Session Not Ended"));
            Assert.False(session.Transactions.Any(t => t.TransType == "End"));
        }


        [Test]
        public void CashBankActivityReport_VerifyHeader()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickReportsTab();

            string startDate = DateTime.Now.AddDays(2).ToString("MM/dd/yyyy");
            string endDate = DateTime.Now.AddDays(3).ToString("MM/dd/yyyy");
            _reportList.ClickReportByReportName("Cash Bank Activity");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.RunReport();

            _reportPage.ReportMenu.ExportDropdown.SelectByIndex(1);
            _reportPage.SaveFileWindow.EnterFilepath(@"C:\Users\bdagg\Downloads");

            string filename = DateTime.Now.ToString("HHmmssfff") + ".xlsx";
            string filepath = @"C:\Users\bdagg\Downloads\" + filename;
            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            Assert.True(_reportPage.SaveFileWindow.FileDownloaded(filepath));

            var reader = new ExcelReader();
            //reader.Open(@"C:\Users\Ben\Downloads\20221107083023.xlsx");
            reader.Open(@"C:\Users\bdagg\Downloads\" + filename);
            var report = reader.ParseCashBankActivityReport();

            string title = report.Title;
            string period = report.ReportPeriod;
            string expectedPeriod = "Reporting Period: " + startDate + " to " + endDate + " 11:59:59 PM";

            Assert.AreEqual("Cash Bank Activity", title);
            Assert.AreEqual(expectedPeriod, period);
        }
    }
}