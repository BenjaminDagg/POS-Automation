using NUnit.Framework;
using System;
using System.Threading;
using POS_Automation.Model;
using System.Threading.Tasks;
using POS_Automation.Pages.Reports;
using OpenQA.Selenium;

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

            string filename = DateTime.Now.ToString("yyyyMMddhhmmss");

            _reportList.ClickReportByReportName("Daily Cashier Activity");
            Thread.Sleep(10000);
            _reportPage.ReportMenu.EnterStartDate("11/5/2022");
            _reportPage.ReportMenu.EnterEndDate("11/8/2022");
            _reportPage.ReportMenu.RunReport();
            _reportPage.ReportMenu.ExportDropdown.SelectByIndex(1);
            Thread.Sleep(3000);
            
            _reportPage.SaveFileWindow.EnterFilepath(@"C:\Users\bdagg\Documents");
            Thread.Sleep(3000);
            _reportPage.SaveFileWindow.EnterFileName(filename);
            _reportPage.SaveFileWindow.Save();

            Thread.Sleep(3000);
            string fullPath = @"C:\Users\bdagg\Documents\" + filename;
            ExcelReader reader = new ExcelReader();
            
            reader.Open(fullPath);
            reader.Read();
            reader.FindTotal();
            reader.Close();
            
        }
    }
}