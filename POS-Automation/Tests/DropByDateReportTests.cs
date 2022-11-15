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
    public class DropByDateReportTests : BaseTest
    {
        private LoginPage _loginPage;
        private VoucherNumPad _numPad;
        private PayoutPage _payoutPage;
        private CashDrawer _cashDrawer;
        private ReportListPage _reportList;
        private ReportPage _reportPage;
        private DatabaseManager _databaseManager;
        private VoucherTransactionRepository _transRepo;
        private decimal StartingAmountDollar = 1000.00m;
        private int StartingAmountCredits = 100000;
        private GameSimulator GameSimulator;

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
            _databaseManager.ResetTestMachine();

            GameSimulator = new GameSimulator(_logService);
            GameSimulator.StartUp();
        }

        [TearDown]
        public void TearDown()
        {
            StartingAmountDollar = 1000.00m;
            StartingAmountCredits = 100000;

            GameSimulator.ShutDown();
        }


        //Verify the correct # of each bill are recorded in the report
        [Test]
        public async Task VerifyBillAmounts()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();


            int total = 5;
            int billCount = 0;

            //Insert 6 $1 bills
            for(int i = 0; i < 6; i++)
            {
                total += 1;
                billCount++;
                GameSimulator.BillIn(1);
            }

            //Insert 5 $5 bills
            for (int i = 0; i < 5; i++)
            {
                total += 5;
                billCount++;
                GameSimulator.BillIn(5);
            }

            //Insert 4 $10 bills
            for (int i = 0; i < 4; i++)
            {
                total += 10;
                billCount++;
                GameSimulator.BillIn(10);
            }

            //Insert 3 $20 bills
            for (int i = 0; i < 3; i++)
            {
                total += 20;
                billCount++;
                GameSimulator.BillIn(20);
            }

            //Insert 2 $50 bills
            for (int i = 0; i < 2; i++)
            {
                total += 50;
                billCount++;
                GameSimulator.BillIn(50);
            }

            //Insert 1 $100 bills
            for (int i = 0; i < 1; i++)
            {
                total += 100;
                billCount++;
                GameSimulator.BillIn(100);
            }

            GameSimulator.CashDrop();

            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            //save report
            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");
            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Drop by Date Range Report");
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
            var report = reader.ParseDropByDateReport();

            var drop = report.GetLatestDrop(TestData.DefaultMachineNumber);
            Assert.NotNull(drop);

            Assert.AreEqual(6,drop.Amount1Dollar);
            Assert.AreEqual(25,drop.Amount5Dollar);
            Assert.AreEqual(40,drop.Amount10Dollar);
            Assert.AreEqual(60, drop.Amount20Dollar);
            Assert.AreEqual(100, drop.Amount50Dollar);
            Assert.AreEqual(100, drop.Amount100Dollar);
            Assert.AreEqual(total,drop.TotalDropAmount);
            Assert.AreEqual(billCount,drop.TotalBills);
        }

        //Verify ticket amount and ticket count are correct
        [Test]
        public async Task VerifyTicketAmounts()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            GameSimulator.BillIn(10);

            int ticketAmountCounter = 0;

            GameSimulator.Play();
            ticketAmountCounter += GameSimulator.gameplayParams.BetAmount;

            GameSimulator.Loss();
            ticketAmountCounter += GameSimulator.gameplayParams.BetAmount;

            int winAmount = 0;
            GameSimulator.Win(out winAmount);
            ticketAmountCounter += GameSimulator.gameplayParams.BetAmount;
            
            GameSimulator.CashDrop();

            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            //save report
            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");
            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Drop by Date Range Report");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.EnterCasinoName("test");
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
            var report = reader.ParseDropByDateReport();

            var drop = report.GetLatestDrop(TestData.DefaultMachineNumber);
            Assert.NotNull(drop);

            Assert.AreEqual(10,drop.Amount10Dollar);
            Assert.AreEqual(6,drop.TotalTicketAmount);
            Assert.AreEqual(3, drop.TotalTickets);
            
        }

        //Verify report totals are correct
        [Test]
        public async Task VerifyTotals()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            GameSimulator.BillIn(10);
            GameSimulator.CashDrop();

            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            //save report
            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");
            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Drop by Date Range Report");
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
            var report = reader.ParseDropByDateReport();

            var records = report.Data;
            Assert.Greater(records.Count, 0);

            int total1 = 0, total5 = 0, total10 = 0, total20 = 0, total50 = 0, total100 = 0;
            decimal totalTicketAmount = 0;
            int ticketCount = 0;
            int billCount = 0;
            decimal dropAmount = 0;

            foreach (var drop in records)
            {
                total1 += drop.Amount1Dollar;
                total5 += drop.Amount5Dollar;
                total10 += drop.Amount10Dollar;
                total20 += drop.Amount20Dollar;
                total50 += drop.Amount50Dollar;
                total100 += drop.Amount100Dollar;
                totalTicketAmount += drop.TotalTicketAmount;
                billCount += drop.TotalBills;
                dropAmount += drop.TotalDropAmount;
                ticketCount += drop.TotalTickets;
            }

            Assert.AreEqual(report.Total1Dollar,total1);
            Assert.AreEqual(report.Total5Dollar,total5);
            Assert.AreEqual(report.Total10Dollar,total10);
            Assert.AreEqual(report.Total20Dollar,total20);
            Assert.AreEqual(report.Total50Dollar,total50);
            Assert.AreEqual(report.Total100Dollar,total100);
            Assert.AreEqual(report.TotalTicketAmount,totalTicketAmount);
            Assert.AreEqual(report.TotalTicketCount, ticketCount);
            Assert.AreEqual(report.TotalBills, billCount);
            Assert.AreEqual(report.TotalDropAmount,dropAmount);

        }


        [Test]
        public void Test()
        {
            
            _loginPage.Login(TestData.SuperUserUsername, TestData.SuperUserPassword);
            NavigationTabs.ClickPayoutTab();

            int startingBalance = 1000;

            _payoutPage.CashDrawer.StartingBalancePrompt.EnterInput(startingBalance.ToString());
            _payoutPage.CashDrawer.StartingBalancePrompt.Confirm();

            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");
            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Drop by Date Range Report");
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
            var report = reader.ParseDropByDateReport();



            Console.WriteLine("Title: " + report.Title);
            Console.WriteLine("Location: " + report.Location);
            Console.WriteLine("Period: " + report.ReportPeriod);
            Console.WriteLine("Run time: " + report.RunDate);

            foreach(var record in report.Data)
            {
                Console.WriteLine($"{record.TerminalId}, $1: {record.Amount1Dollar}, $5: {record.Amount5Dollar}, $10{record.Amount10Dollar}, $20: {record.Amount20Dollar}, $50: {record.Amount50Dollar}, $100: {record.Amount100Dollar}, $Ticket: {record.TotalTicketAmount}, #Tickets: {record.TotalTickets}, #Bills: {record.TotalBills}, $Drop: {record.TotalDropAmount}, Date: {record.DropTime}, Account: {record.Account}");
            }

            Console.WriteLine($"Totals - $1: {report.Total1Dollar}, $5: {report.Total5Dollar}, $10: {report.Total10Dollar}, $20: {report.Total20Dollar}, $50: {report.Total50Dollar}, $100: {report.Total100Dollar}, $Tickets: {report.TotalTicketAmount}, #Tickets: {report.TotalTicketCount}, Bills: {report.TotalBills}, Drop: {report.TotalDropAmount}");

            var latestDrop = report.GetLatestDrop(TestData.DefaultMachineNumber);
            Assert.NotNull(latestDrop);
            Console.WriteLine("Latest: " + latestDrop.TotalDropAmount);
        }

        //Verify report displays no data if the casino name doesn't exist
        [Test]
        public async Task InvalidCasinoName()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            GameSimulator.BillIn(10);
            GameSimulator.CashDrop();

            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            //save report
            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");

            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Drop by Date Range Report");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.EnterCasinoName("InvalidCasino");
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
            var report = reader.ParseDropByDateReport();

            Assert.Zero(report.Data.Count);

        }

        //Verify the entered casino name gets inserted into the report
        [Test]
        public async Task CainoName()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            string casino = "TestCasino";

            GameSimulator.BillIn(10);
            GameSimulator.CashDrop();

            Thread.Sleep(TestData.PollingIntervalSec * 1000 * 2);

            //save report
            string startDate = DateTime.Now.AddDays(-1).ToString("MM/d/yyyy");
            string endDate = DateTime.Now.AddDays(1).ToString("MM/d/yyyy");

            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Drop by Date Range Report");
            _reportPage.ReportMenu.EnterStartDate(startDate);
            _reportPage.ReportMenu.EnterEndDate(endDate);
            _reportPage.ReportMenu.EnterCasinoName(casino);
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
            var report = reader.ParseDropByDateReport();

            string expected = casino + $" ({TestData.LocationId})";

            Assert.AreEqual(expected,report.Location);

        }

        //Verify header content
        [Test]
        public void VerifyHeaders()
        {

            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            DateTime startDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now.AddDays(1);

            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Drop by Date Range Report");
            _reportPage.ReportMenu.EnterStartDate(startDate.ToString("MM/d/yyyy"));
            _reportPage.ReportMenu.EnterEndDate(endDate.ToString("MM/d/yyyy"));
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
            var report = reader.ParseDropByDateReport();

            string expectedLocation = TestData.DefaultLocationName + $" ({TestData.LocationId})";
            string expectedTitle = "Drop by Date Range Report";
            string expectedPeriod = $"For Drops Performed between {startDate.ToString("MM-dd-yyyy")} 02:00 AM and {endDate.ToString("MM-dd-yyyy")} 02:00 AM";

            Assert.AreEqual(expectedTitle, report.Title);
            Assert.AreEqual(expectedLocation, report.Location);
            Assert.AreEqual(expectedPeriod,report.ReportPeriod);
         
        }

        //Verify no errors are generated if report doesn't have data
        [Test]
        public void EmptyReport()
        {

            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickDeviceTab();

            DateTime startDate = DateTime.Now.AddDays(3);
            DateTime endDate = DateTime.Now.AddDays(4);

            _payoutPage.NavigationTabs.ClickReportsTab();
            _reportList.ClickReportByReportName("Drop by Date Range Report");
            _reportPage.ReportMenu.EnterStartDate(startDate.ToString("MM/d/yyyy"));
            _reportPage.ReportMenu.EnterEndDate(endDate.ToString("MM/d/yyyy"));
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
            var report = reader.ParseDropByDateReport();

            string expectedLocation = TestData.DefaultLocationName + $" ({TestData.LocationId})";
            string expectedTitle = "Drop by Date Range Report";
            string expectedPeriod = $"For Drops Performed between {startDate.ToString("MM-dd-yyyy")} 02:00 AM and {endDate.ToString("MM-dd-yyyy")} 02:00 AM";

            Assert.Zero(report.Data.Count);
            Assert.AreEqual(expectedTitle, report.Title);
            Assert.AreEqual(expectedLocation, report.Location);
            Assert.True(report.ReportPeriod.Contains(startDate.ToString("MM-dd-yyyy")) && report.ReportPeriod.Contains(endDate.ToString("MM-dd-yyyy")));

        }

    }
}