using NUnit.Framework;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;   //DesiredCapabilities
using System;
using Appium;
using OpenQA.Selenium.Appium;   //Appium Options
using System.Threading;
using OpenQA.Selenium;
using System.Collections;
using OpenQA.Selenium.Interactions;
using POS_Automation.Pages;
using POS_Automation.Custom_Elements.Alerts;
using System.Text.RegularExpressions;
using System.Globalization;
using POS_Automation.Model;
using POS_Automation.Custom_Elements.Alerts;
using POS_Automation.Pages.Payout;
using POS_Automation.Model.Payout;
using System.Collections.Generic;

namespace POS_Automation.Pages.Reports
{
    public class ReportListPage : DataGridPage
    {
        public ReportListPage(WindowsDriver<WindowsElement> _driver) : base(_driver)
        {

        }

        public override By DataGrid
        {
            get
            {
                return new ByAccessibilityId("ReportsGrid");
            }
        }

        public List<ReportListItem> GetReportOptions()
        {
            var options = new List<ReportListItem>();

            wait.Until(d => driver.FindElements(RowSelector).Count > 0);
            var rows = driver.FindElements(RowSelector);

            foreach(var row in rows)
            {
                string reportName = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[1]")).Text;
                string dateString = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[2]")).Text;
                DateTime? lastRun = null;
                
                if (!string.IsNullOrEmpty(dateString))
                {
                    lastRun = DateTime.ParseExact(dateString, "M/d/yyyy h:m:s tt", CultureInfo.InvariantCulture);
                }

                var report = new ReportListItem();
                report.ReportName = reportName;
                report.LastRun = lastRun;

                options.Add(report);
            }

            return options;
        }

        public void ClickReportByReportName(string targetReport)
        {
            var options = new List<ReportListItem>();

            wait.Until(d => driver.FindElements(RowSelector).Count > 0);
            var rows = driver.FindElements(RowSelector);

            foreach (var row in rows)
            {
                try
                {
                    string reportName = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[1]")).Text;

                    if (reportName == targetReport)
                    {
                        row.Click();
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
