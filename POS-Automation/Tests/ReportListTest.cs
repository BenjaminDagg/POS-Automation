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

        [SetUp]
        public void Setup()
        {
            _loginPage = new LoginPage(driver);
            _reportList = new ReportListPage(driver);
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

           
            _reportList.ClickReportByReportName("Daily Cashier Activity");
            Thread.Sleep(10000);

            var table = driver.FindElement(By.ClassName("ReportingTablixControl"));
            var rows = table.FindElements(By.XPath("//*"))[0].FindElements(By.XPath("//*"));
            Console.WriteLine("found " + rows.Count + " rows");
            Console.WriteLine("class = " + rows[0].GetAttribute("ClassName"));
            foreach(var row in rows)
            {
                
            }
        }
    }
}