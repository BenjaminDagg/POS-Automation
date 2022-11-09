using NUnit.Framework;
using System;
using System.Threading;
using POS_Automation.Model;
using System.Threading.Tasks;
using POS_Automation.Pages.Reports;
using OpenQA.Selenium;
using System.Linq;

namespace POS_Automation
{
    public class ReportListTest : BaseTest
    {
        private LoginPage _loginPage;
        private ReportListPage _reportList;
        private ReportPage _reportPage;

        [SetUp]
        public void Setup()
        {
            _loginPage = new LoginPage(driver);
            _reportList = new ReportListPage(driver);
            _reportPage = new ReportPage(driver);
        }

        [TearDown]
        public void TearDown()
        {

        }

        [Test]
        public void ReportOptions()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickReportsTab();

            var reports = _reportList.GetReportOptions();
            foreach(var report in reports)
            {
                Console.WriteLine(report.ReportName);
                Console.WriteLine(report.LastRun?.ToString());
            }
        }

        [Test]
        public void ReportListColumns()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickReportsTab();

            Assert.AreEqual("Report Name", _reportList.GetHeader(0));
            Assert.AreEqual("Last Run", _reportList.GetHeader(1));
        }

        [Test]
        public void ReportLastRun()
        {
            
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickReportsTab();

            var rep = _reportList.GetReportOptions();
            var cashierActivityReport = rep.SingleOrDefault(r => r.ReportName == "Daily Cashier Activity");
            var lastRunBefore = cashierActivityReport.LastRun;

            _reportList.ClickReportByReportName(cashierActivityReport.ReportName);

            _reportPage.ReportMenu.Back();

            var caRReportAfter = _reportList.GetReportOptions().Where(r => r.ReportName == "Daily Cashier Activity").ToList()[0];
            var lastRunAfter = caRReportAfter.LastRun;

            Assert.Greater(lastRunAfter, lastRunBefore);
        }

        [Test]
        public void ExportOptions()
        {

            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickReportsTab();

            _reportList.ClickReportByReportName("Daily Cashier Activity");

            var exportOptions = _reportPage.ReportMenu.ExportDropdown.Options;
            foreach(var option in exportOptions)
            {
                Console.WriteLine(option);
            }
            Console.WriteLine("got " + exportOptions.Count + " options in test");
            Assert.True(exportOptions.Any(e => e == "PDF"));
            Assert.True(exportOptions.Any(e => e == "Excel"));
        }

        [Test]
        public void ZoomOptions()
        {

            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickReportsTab();

            _reportList.ClickReportByReportName("Daily Cashier Activity");

            var zoomOptoins = _reportPage.ReportMenu.ZoomDropdown.Options;

            Assert.AreEqual(7, zoomOptoins.Count);
            Assert.True(zoomOptoins.Any(z => z == "25%"));
            Assert.True(zoomOptoins.Any(z => z == "50%"));
            Assert.True(zoomOptoins.Any(z => z == "100%"));
            Assert.True(zoomOptoins.Any(z => z == "200%"));
            Assert.True(zoomOptoins.Any(z => z == "300%"));
            Assert.True(zoomOptoins.Any(z => z == "400%"));
            Assert.True(zoomOptoins.Any(z => z == "500%"));
        }

        [Test]
        public void DefaultZoomOption()
        {

            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickReportsTab();

            _reportList.ClickReportByReportName("Daily Cashier Activity");

            var selected = _reportPage.ReportMenu.ZoomDropdown.SelectedOption;
            Assert.AreEqual("100%",selected);
        }

        [Test]
        public void NextPageButton()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickReportsTab();

            var today = DateTime.Now;
            var firstDayDate = new DateTime(today.Year, today.Month, 1);
            var lastDayDate = firstDayDate.AddDays(14);

            _reportList.ClickReportByReportName("Daily Cashier Activity");
            _reportPage.ReportMenu.EnterStartDate("1/1/2022");
            _reportPage.ReportMenu.EnterEndDate("11/9/2022");
            _reportPage.ReportMenu.RunReport();
            
            _reportPage.ReportMenu.RunReport();
            _reportPage.ReportMenu.ClickNextPage();

            Assert.AreEqual(2, _reportPage.ReportMenu.GetCurrentPage());
            
        }

        [Test]
        public void PreviousPageButton()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickReportsTab();

            var today = DateTime.Now;
            var firstDayDate = new DateTime(today.Year, today.Month, 1);
            var lastDayDate = firstDayDate.AddDays(14);

            _reportList.ClickReportByReportName("Daily Cashier Activity");
            _reportPage.ReportMenu.EnterStartDate(firstDayDate.ToString("M/d/yyyy"));
            _reportPage.ReportMenu.EnterEndDate(lastDayDate.ToString("M/d/yyyy"));
            _reportPage.ReportMenu.RunReport();
            _reportPage.ReportMenu.ClickNextPage();

            Assert.AreEqual(2, _reportPage.ReportMenu.GetCurrentPage());

            _reportPage.ReportMenu.ClickPrevPage();

            Assert.AreEqual(1, _reportPage.ReportMenu.GetCurrentPage());
        }

        [Test]
        public void FirstPageButton()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickReportsTab();

            var today = DateTime.Now;
            var firstDayDate = new DateTime(today.Year, today.Month, 1);
            var lastDayDate = firstDayDate.AddDays(14);

            _reportList.ClickReportByReportName("Daily Cashier Activity");
            _reportPage.ReportMenu.EnterStartDate(firstDayDate.ToString("M/d/yyyy"));
            _reportPage.ReportMenu.EnterEndDate(lastDayDate.ToString("M/d/yyyy"));
            _reportPage.ReportMenu.RunReport();
            _reportPage.ReportMenu.ClickNextPage();

            Assert.AreEqual(2, _reportPage.ReportMenu.GetCurrentPage());

            _reportPage.ReportMenu.ClickFirstPage();

            Assert.AreEqual(1, _reportPage.ReportMenu.GetCurrentPage());
        }

        [Test]
        public void LastPageButton()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickReportsTab();

            var today = DateTime.Now;
            var firstDayDate = new DateTime(today.Year, today.Month, 1);
            var lastDayDate = firstDayDate.AddDays(14);

            _reportList.ClickReportByReportName("Daily Cashier Activity");
            _reportPage.ReportMenu.EnterStartDate(firstDayDate.ToString("M/d/yyyy"));
            _reportPage.ReportMenu.EnterEndDate(lastDayDate.ToString("M/d/yyyy"));
            _reportPage.ReportMenu.RunReport();

            Assert.AreEqual(1, _reportPage.ReportMenu.GetCurrentPage());

            _reportPage.ReportMenu.ClickLastPage();

            Assert.Greater(_reportPage.ReportMenu.GetCurrentPage(),1);
        }

        [Test]
        public void ParameterVisibilityDefault()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickReportsTab();

            _reportList.ClickReportByReportName("Daily Cashier Activity");

            Assert.False(_reportPage.ReportMenu.ParametersAreHidden);
        }

        [Test]
        public void HideParameters()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickReportsTab();

            _reportList.ClickReportByReportName("Daily Cashier Activity");

            Assert.False(_reportPage.ReportMenu.ParametersAreHidden);
            _reportPage.ReportMenu.HideParameters();
            Assert.True(_reportPage.ReportMenu.ParametersAreHidden);
        }

        [Test]
        public void ShowParameters()
        {
            _loginPage.Login(TestData.AdminUsername, TestData.AdminPassword);
            NavigationTabs.ClickReportsTab();

            _reportList.ClickReportByReportName("Daily Cashier Activity");

            Assert.False(_reportPage.ReportMenu.ParametersAreHidden);
            _reportPage.ReportMenu.HideParameters();
            Assert.True(_reportPage.ReportMenu.ParametersAreHidden);
            _reportPage.ReportMenu.ShowParameters();
            Assert.False(_reportPage.ReportMenu.ParametersAreHidden);
        }
    }
}