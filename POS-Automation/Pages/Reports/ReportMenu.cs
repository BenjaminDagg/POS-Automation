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
using POS_Automation.Model.Reports;

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
        private ByAccessibilityId BackButton;
        private ByAccessibilityId PrintButton;
        private ByAccessibilityId ZoomDropdownSelect;
        public ReportDropdownElement ZoomDropdown;
        private ByAccessibilityId ToggleParametersButton;

        //page select buttons
        private ByAccessibilityId FirstPageButton;
        private ByAccessibilityId PrevPageButton;
        private ByAccessibilityId CurrentPageTextBox;
        private ByAccessibilityId NextPageButton;
        private ByAccessibilityId LastPageButton;

        public ReportMenu(WindowsDriver<WindowsElement> _driver) : base(_driver)
        {
            ExportDropdownSelect = new ByAccessibilityId("PART_exportControl");
            ExportDropdown = new ReportDropdownElement(ExportDropdownSelect, driver);
            StartDateField = By.XPath("(//*[@ClassName='DateTimeEdit'])[1]");
            StartDateDropdown = By.XPath("(//*[@Name='Show Calendar'])[1]");
            EndDateField = By.XPath("(//*[@ClassName='DateTimeEdit'])[2]");
            EndDateDropdown = By.XPath("(//*[@Name='Show Calendar'])[2]");
            ViewReportButton = new ByAccessibilityId("PART_btnViewReport");
            BackButton = new ByAccessibilityId("Back");
            PrintButton = new ByAccessibilityId("PART_buttonPrint");
            ZoomDropdownSelect = new ByAccessibilityId("PART_comboBoxPageZoom");
            ZoomDropdown = new ReportDropdownElement(ZoomDropdownSelect, driver);
            ToggleParametersButton = new ByAccessibilityId("PART_buttonParameters");

            FirstPageButton = new ByAccessibilityId("PART_buttonFirst");
            PrevPageButton = new ByAccessibilityId("PART_buttonPrevious");
            CurrentPageTextBox = new ByAccessibilityId("PART_textBoxCurrentPage");
            NextPageButton = new ByAccessibilityId("PART_buttonNext");
            LastPageButton = new ByAccessibilityId("PART_buttonLast");
        }

        public bool ParametersAreHidden
        {
            get
            {
                try
                {
                    wait.Until(d => driver.FindElement(StartDateDropdown));
                    return false;
                }
                catch (Exception ex)
                {
                    return true;
                }
            }
        }

        public void ExportReport(ReportExportOptions exportType)
        {
            int index = (int)exportType;

            ExportDropdown.SelectByIndex(index);
        }

        public void EnterStartDate(string date)
        {
            wait.Until(d => driver.FindElement(StartDateField));
            wait.Until(d => driver.FindElements(By.XPath("//*[@ClassName='ProgressBar']")).Count == 0);
            
            string text = driver.FindElement(StartDateField).Text;

            if(text.IndexOf(':') == -1)
            {
                driver.FindElement(StartDateField).Click();
            }

            //month
            int month = int.Parse(date.Substring(0, date.IndexOf('/')));

            driver.FindElement(StartDateField).SendKeys(month.ToString());
            if (month < 10)
            {
                driver.FindElement(StartDateField).SendKeys(Keys.ArrowRight);
            }

            //day
            int monthStartSlash = date.IndexOf('/');
            int monthEndSlash = date.LastIndexOf('/');

            if (monthStartSlash == -1 || monthEndSlash == -1)
            {
                return;
            }

            int length = (monthEndSlash - monthStartSlash) - 1;
            int dayVal = int.Parse(date.Substring(monthStartSlash + 1, length));

            driver.FindElement(StartDateField).SendKeys(dayVal.ToString());
            if (dayVal < 10)
            {
                driver.FindElement(StartDateField).SendKeys(Keys.ArrowRight);
            }

            //year
            int year = int.Parse(date.Substring(monthEndSlash + 1, 4));

            driver.FindElement(StartDateField).SendKeys(year.ToString());
            driver.FindElement(StartDateField).SendKeys(Keys.ArrowRight);
        }

        public void EnterEndDate(string date)
        {
            wait.Until(d => driver.FindElement(EndDateField));
            wait.Until(d => driver.FindElements(By.XPath("//*[@ClassName='ProgressBar']")).Count == 0);
            driver.FindElement(EndDateField).Click();

            //month
            int month = int.Parse(date.Substring(0,date.IndexOf('/')));

            driver.FindElement(EndDateDropdown).SendKeys(month.ToString());
            if(month < 10)
            {
                driver.FindElement(EndDateDropdown).SendKeys(Keys.ArrowRight);
            }

            //day
            int monthStartSlash = date.IndexOf('/');
            int monthEndSlash = date.LastIndexOf('/');
            
            if (monthStartSlash == -1 || monthEndSlash == -1)
            {
                return;
            }

            int length = (monthEndSlash - monthStartSlash) - 1;
            int dayVal = int.Parse(date.Substring(monthStartSlash + 1, length));

            driver.FindElement(EndDateDropdown).SendKeys(dayVal.ToString());
            if (dayVal < 10)
            {
                driver.FindElement(EndDateDropdown).SendKeys(Keys.ArrowRight);
            }

            //year
            int year = int.Parse(date.Substring(monthEndSlash + 1, 4));

            driver.FindElement(EndDateDropdown).SendKeys(year.ToString());
            driver.FindElement(EndDateDropdown).SendKeys(Keys.ArrowRight);

        }

        public void RunReport()
        {
            wait.Until(d => driver.FindElement(ViewReportButton));
            driver.FindElement(ViewReportButton).Click();
            Thread.Sleep(3000);
            wait.Until(d => driver.FindElements(By.XPath("//*[@ClassName='ProgressBar']")).Count == 0);
        }

        public void Back()
        {
            wait.Until(d => driver.FindElement(BackButton));
            driver.FindElement(BackButton).Click();
        }

        public void ClickPrint()
        {
            Thread.Sleep(3000);
            wait.Until(d => driver.FindElement(PrintButton));
            driver.FindElement(PrintButton).Click();
        }

        public void ClickFirstPage()
        {
            wait.Until(d => driver.FindElement(FirstPageButton));
            driver.FindElement(FirstPageButton).Click();
        }

        public void ClickPrevPage()
        {
            wait.Until(d => driver.FindElement(PrevPageButton));
            driver.FindElement(PrevPageButton).Click();
        }

        public void EnterCurrentPage(int pageNum)
        {
            wait.Until(d => driver.FindElement(CurrentPageTextBox));
            driver.FindElement(CurrentPageTextBox).SendKeys(pageNum.ToString());
        }

        public int GetCurrentPage()
        {
            wait.Until(d => driver.FindElement(CurrentPageTextBox));
            return int.Parse(driver.FindElement(CurrentPageTextBox).Text);
        }

        public void ClickNextPage()
        {
            wait.Until(d => driver.FindElement(NextPageButton));
            driver.FindElement(NextPageButton).Click();
        }

        public void ClickLastPage()
        {
            wait.Until(d => driver.FindElement(LastPageButton));
            driver.FindElement(LastPageButton).Click();
        }

        public void ShowParameters()
        {
            if (ParametersAreHidden)
            {
                driver.FindElement(ToggleParametersButton).Click();
            }
        }

        public void HideParameters()
        {
            if (!ParametersAreHidden)
            {
                driver.FindElement(ToggleParametersButton).Click();
            }
        }
    }
}
