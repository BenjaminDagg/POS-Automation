using NUnit.Framework;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;   //DesiredCapabilities
using System;
using Appium;
using OpenQA.Selenium.Appium;   //Appium Options
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace POS_Automation.Custom_Elements.Alerts
{
    public class CashDrawerEditPrompt : MultiChoiceAlertWindow
    {

        public ByAccessibilityId AmountTextbox;
        public ByAccessibilityId PasswordTextbox;

        public CashDrawerEditPrompt(WindowsDriver<WindowsElement> _driver, By windowSelector) : base(_driver, windowSelector)
        {
            driver = _driver;

            AmountTextbox = new ByAccessibilityId("Amount");
            PasswordTextbox = new ByAccessibilityId("PasswordTextBox");
        }

        protected override By ConfirmButton
        {
            get
            {
                return new ByAccessibilityId("Ok");
            }
        }

        protected override By CancelButton
        {
            get
            {
                return new ByAccessibilityId("Cancel");
            }
        }

        public void EnterAmount(string text)
        {
            if (!IsOpen)
            {
                return;
            }

            wait.Until(d => driver.FindElement(AmountTextbox));
            driver.FindElement(AmountTextbox).SendKeys(text);
        }

        public void EnterPassword(string text)
        {
            if (!IsOpen)
            {
                return;
            }

            wait.Until(d => driver.FindElement(PasswordTextbox));
            driver.FindElement(PasswordTextbox).SendKeys(text);
        }

        public override void Confirm()
        {
            base.Confirm();
            Thread.Sleep(1000);

            try
            {
                wait.Until(d => driver.FindElements(AmountTextbox).Count == 0);
            }
            catch (Exception ex)
            {

            }
        }

        public string GetAmount()
        {
            if (!IsOpen)
            {
                return string.Empty;
            }

            wait.Until(d => driver.FindElement(AmountTextbox));
            return driver.FindElement(AmountTextbox).Text;
        }
    }
}
