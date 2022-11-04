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

namespace POS_Automation.Pages.Payout
{
    public class CashDrawerHistory : DataGridPage
    {
        private ByAccessibilityId PrintCashHistoryButton;
        private ByAccessibilityId CancelButton;
        private By WindowSelector;
        private By CurrentBalanceLabel;
        private ByAccessibilityId HistoryTable;

        public CashDrawerHistory(WindowsDriver<WindowsElement> _driver) : base(_driver)
        {
            driver = _driver;

            PrintCashHistoryButton = new ByAccessibilityId("Ok");
            CancelButton = new ByAccessibilityId("Cancel");
            WindowSelector = By.Name("Cash Drawer History");
            CurrentBalanceLabel = By.XPath("//*[@Name='Cash Drawer History']/Text[contains(@Name,'Current Balance:')]");

            HistoryTable = new ByAccessibilityId("VouchersGrid");
        }

        public override By DataGrid
        {
            get
            {
                return HistoryTable;
            }
        }

        public bool IsOpen
        {
            get
            {
                try
                {
                    wait.Until(d => driver.FindElement(WindowSelector));
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }

        public decimal CurrentBalance
        {
            get
            {
                string text = driver.FindElement(CurrentBalanceLabel).Text;
                int colonIndex = text.IndexOf(':');
                string amountString = text.Substring(colonIndex + 1).Trim();

                return decimal.Parse(amountString, NumberStyles.Currency);
            }
        }

        public void ReadTable()
        {
            if (!IsOpen)
            {
                return;
            }

            var rows = driver.FindElement(HistoryTable).FindElements(RowSelector);
            foreach (var row in rows)
            {
                var transType = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[1]")).Text;
                var amount = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[2]")).Text;
                var date = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[3]")).Text;

                Console.WriteLine("================");
                Console.WriteLine($"Trans Type: {transType}, Amount: {amount}, Date: {date}");
            }
        }

        public void PrintHistory()
        {
            if (IsOpen)
            {
                driver.FindElement(PrintCashHistoryButton).Click();
            }
        }

        public void Close()
        {
            if (IsOpen)
            {
                driver.FindElement(CancelButton).Click();
            }
        }
    }
}
