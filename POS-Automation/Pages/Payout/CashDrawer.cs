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
    public class CashDrawer : BasePage
    {
        private By CurrentBalanceLabel;
        private By StartingBalanceLabel;
        private By CashAddedLabel;
        private By CashRemovedLabel;
        private By TotalPayoutLabel;

        private ByAccessibilityId AddCashButton;
        private ByAccessibilityId RemoveCashButton;

        private By AddCashWindowSelector;
        public CashDrawerEditPrompt AddCashPrompt;

        private By RemoveCashWindowSelector;
        public CashDrawerEditPrompt RemoveCashPrompt;

        public By CashDrawerHistoryButton;
        public CashDrawerHistory CashDrawerHistory;

        public By CashDrawerConnectedPromptSelector;
        public MultiChoiceAlertWindow CashDrawerConnectedPrompt;
        private By StartingBalanceWindowSelector;
        public TextboxAlert StartingBalancePrompt;
        private ByAccessibilityId StartingBalanceTextbox;

        private By StartingBalanceNotSetPromptSelector;
        public SingleChoiceAlertWindow StartingBalanceErrorAlert;

        public CashDrawer(WindowsDriver<WindowsElement> _driver) : base(_driver)
        {
            driver = _driver;

            CurrentBalanceLabel = By.XPath("//*[contains(@Name,'Current Balance:')]");
            StartingBalanceLabel = By.XPath("//*[contains(@Name,'Starting Balance:')]");
            CashAddedLabel = By.XPath("//*[contains(@Name,'Cash Added:')]");
            CashRemovedLabel = By.XPath("//*[contains(@Name,'Cash Removed :')]");
            TotalPayoutLabel = By.XPath("//*[contains(@Name,'Total Payout:')]");
            AddCashButton = new ByAccessibilityId("Add");
            RemoveCashButton = new ByAccessibilityId("Remove");
            CashDrawerHistoryButton = By.XPath("//*[@ClassName='GroupBox']/Button");
            StartingBalanceTextbox = new ByAccessibilityId("StartingBalance");
            StartingBalanceWindowSelector = By.Name("Cash Drawer Starting Balance");
            StartingBalancePrompt = new TextboxAlert(driver, StartingBalanceWindowSelector, StartingBalanceTextbox);

            AddCashWindowSelector = By.Name("Add Cash");
            AddCashPrompt = new CashDrawerEditPrompt(driver, AddCashWindowSelector);

            RemoveCashWindowSelector = By.Name("Remove Cash");
            RemoveCashPrompt = new CashDrawerEditPrompt(driver, RemoveCashWindowSelector);

            CashDrawerHistory = new CashDrawerHistory(driver);

            CashDrawerConnectedPromptSelector = By.XPath("//Window[@Name='Cash Drawer']");
            CashDrawerConnectedPrompt = new MultiChoiceAlertWindow(driver, CashDrawerConnectedPromptSelector);

            StartingBalanceNotSetPromptSelector = By.Name("Unable to initialize for Payout.");
            StartingBalanceErrorAlert = new SingleChoiceAlertWindow(driver,StartingBalanceNotSetPromptSelector);
        }

        public decimal CurrentBalance
        {
            get
            {
                string text = driver.FindElement(CurrentBalanceLabel).Text;
                int colonIndex = text.IndexOf(':');
                string amountString = text.Substring(colonIndex + 1).Trim();

                return decimal.Parse(amountString,NumberStyles.Currency);
            }
        }

        public decimal StartingBalance
        {
            get
            {
                string text = driver.FindElement(StartingBalanceLabel).Text;
                int colonIndex = text.IndexOf(':');
                string amountString = text.Substring(colonIndex + 1).Trim();

                return decimal.Parse(amountString, NumberStyles.Currency);
            }
        }

        public decimal CashAdded
        {
            get
            {
                string text = driver.FindElement(CashAddedLabel).Text;
                int colonIndex = text.IndexOf(':');
                string amountString = text.Substring(colonIndex + 1).Trim();

                return decimal.Parse(amountString, NumberStyles.Currency);
            }
        }

        public decimal CashRemoved
        {
            get
            {
                string text = driver.FindElement(CashRemovedLabel).Text;
                text = text.Replace("(", "");
                text = text.Replace(")", "");
                int colonIndex = text.IndexOf(':');
                string amountString = text.Substring(colonIndex + 1).Trim();

                return decimal.Parse(amountString, NumberStyles.Currency);
            }
        }

        public decimal TotalPayout
        {
            get
            {
                string text = driver.FindElement(TotalPayoutLabel).Text;
                text = text.Replace("(", "");
                text = text.Replace(")", "");

                int colonIndex = text.IndexOf(':');
                string amountString = text.Substring(colonIndex + 1).Trim();

                return decimal.Parse(amountString, NumberStyles.Currency);
            }
        }

        public bool IsHidden
        {
            get
            {
                try
                {
                    wait.Until(d => driver.FindElement(AddCashButton));

                    return false;
                }
                catch(Exception ex)
                {
                    return true;
                }
            }
        }

        public void ClickAddCash()
        {
            wait.Until(d => driver.FindElement(AddCashButton));
            driver.FindElement(AddCashButton).Click();
        }

        public void ClickRemoveCash()
        {
            wait.Until(d => driver.FindElement(RemoveCashButton));
            driver.FindElement(RemoveCashButton).Click();
        }

        public void ClickCashDrawerHistory()
        {
            driver.FindElement(CashDrawerHistoryButton).Click();
        }

        public void AddCash(string amount, string password)
        {
            ClickAddCash();
            AddCashPrompt.EnterAmount(amount);
            AddCashPrompt.EnterPassword(password);
            AddCashPrompt.Confirm();
        }

        public void RemoveCash(string amount, string password)
        {
            ClickRemoveCash();
            RemoveCashPrompt.EnterAmount(amount);
            RemoveCashPrompt.EnterPassword(password);
            RemoveCashPrompt.Confirm();
        }
    }
}
