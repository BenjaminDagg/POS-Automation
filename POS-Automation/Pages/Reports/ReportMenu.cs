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
using POS_Automation.Custom_Elements;

namespace POS_Automation.Pages.Reports
{
    public class ReportMenu : BasePage
    {
        private ByAccessibilityId ExportDropdownSelect;
        public ReportDropdownElement ExportDropdown;
        private By StartDateField;
        private By StartDateDropdown;
        private By EndDateField;
        private By EndDateDropdown;
        private ByAccessibilityId ViewReportButton;

        public ReportMenu(WindowsDriver<WindowsElement> _driver) : base(_driver)
        {
            ExportDropdownSelect = new ByAccessibilityId("PART_exportControl");
            ExportDropdown = new ReportDropdownElement(ExportDropdownSelect, driver);
            StartDateField = By.XPath("(//*[@ClassName='DateTimeEdit'])[1]");
            StartDateDropdown = By.XPath("(//*[@Name='Show Calendar'])[1]");
            EndDateField = By.XPath("(//*[@ClassName='DateTimeEdit'])[2]");
            EndDateDropdown = By.XPath("(//*[@Name='Show Calendar'])[2]");
            ViewReportButton = new ByAccessibilityId("PART_btnViewReport");
        }

        public void EnterStartDate(string date)
        {
            int monthStartSlash = date.IndexOf('/');
            int monthEndSlash = date.LastIndexOf('/');

            if (monthStartSlash == -1 || monthEndSlash == -1)
            {
                return;
            }

            int length = (monthEndSlash - monthStartSlash) - 1;
            int dayVal = int.Parse(date.Substring(monthStartSlash + 1, length));

            wait.Until(d => driver.FindElement(StartDateField));
            driver.FindElement(StartDateDropdown).Click();

            try
            {
                driver.FindElement(By.XPath("//*[@Name='" + dayVal.ToString() + "']")).Click();
            }
            catch (Exception ex)
            {

            }
        }

        public void EnterEndDate(string date)
        {
            int monthStartSlash = date.IndexOf('/');
            int monthEndSlash = date.LastIndexOf('/');

            if (monthStartSlash == -1 || monthEndSlash == -1)
            {
                return;
            }

            int length = (monthEndSlash - monthStartSlash) - 1;
            int dayVal = int.Parse(date.Substring(monthStartSlash + 1, length));

            wait.Until(d => driver.FindElement(EndDateField));
            driver.FindElement(EndDateDropdown).Click();

            try
            {
                driver.FindElement(By.XPath("//*[@Name='" + dayVal.ToString() + "']")).Click();
            }
            catch (Exception ex)
            {

            }
        }

        public void RunReport()
        {
            wait.Until(d => driver.FindElement(ViewReportButton));
            driver.FindElement(ViewReportButton).Click();
        }
    }
}
