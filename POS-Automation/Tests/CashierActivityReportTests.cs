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
            reader.Open(@"C:\Users\Ben\Downloads\Daily Cashier Activity12432.xlsx");
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
            reader.Open(@"C:\Users\Ben\Downloads\Cash Bank Activity353.xlsx");
            var report = reader.ParseCashBankActivityReport();
            
        }
    }
}