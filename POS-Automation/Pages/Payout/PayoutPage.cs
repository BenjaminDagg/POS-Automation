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
    internal class PayoutPage : BasePage
    {
        
        private By PayoutButton;
        private By CancelTransactionButton;
        private By CancelTransactionWindowSelector;
        public MultiChoiceAlertWindow CancelTransactionPrompt;
        private By PayoutSuccessWindowSelector;
        public SingleChoiceAlertWindow PayoutConfirmationAlert;
        private By PayoutErrorWindowSelector;
        public SingleChoiceAlertWindow PayoutError;

        public CashDrawer CashDrawer;
        public VoucherNumPad NumPad;
        public TransactionList CurrentTransactionList;

        public PayoutPage(WindowsDriver<WindowsElement> _driver) : base(_driver)
        {
            driver = _driver;

            PayoutButton = By.XPath("//*[@ClassName='CurrentTransactionView']/Button[@Name='Payout']");
            CancelTransactionButton = By.Name("Cancel Transaction");
            
            CancelTransactionWindowSelector = By.Name("Confirm Action");
            CancelTransactionPrompt = new MultiChoiceAlertWindow(driver, CancelTransactionWindowSelector);

            PayoutSuccessWindowSelector = By.Name("Success");
            PayoutConfirmationAlert = new SingleChoiceAlertWindow(driver, PayoutSuccessWindowSelector);

            CashDrawer = new CashDrawer(driver);
            NumPad = new VoucherNumPad(driver);
            CurrentTransactionList = new TransactionList(driver);

            PayoutErrorWindowSelector = By.Name("Error");
            PayoutError = new SingleChoiceAlertWindow(driver, PayoutErrorWindowSelector);
        }

        public void CancelTransaction()
        {
            driver.FindElement(CancelTransactionButton).Click();
            driver.FindElement(CancelTransactionButton).Click();
            CancelTransactionPrompt.Confirm();
        }

        public void Payout()
        {
            Thread.Sleep(1000);

            wait.Until(d => driver.FindElement(PayoutButton));
            driver.FindElement(PayoutButton).Click();
            driver.FindElement(PayoutButton).Click();
            PayoutConfirmationAlert.Confirm();
        }

        public void ClickPayout()
        {
            Thread.Sleep(1000);

            wait.Until(d => driver.FindElement(PayoutButton));
            driver.FindElement(PayoutButton).Click();
            driver.FindElement(PayoutButton).Click();
            
        }

        public bool PayoutIsHidden()
        {
            try
            {
                wait.Until(d => driver.FindElement(PayoutButton));
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }
    }
}
