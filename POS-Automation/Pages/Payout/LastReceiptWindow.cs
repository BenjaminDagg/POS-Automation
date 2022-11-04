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
    public class LastReceiptWindow : BasePage
    {
        private ByAccessibilityId ReprintButton;
        private ByAccessibilityId CancelButton;
        private By WindowSelector;
        private By ReceiptNumberElement;
        private By NumVouchersElement;
        private By AmountElement;

        public LastReceiptWindow(WindowsDriver<WindowsElement> _driver) : base(_driver)
        {
            driver = _driver;

            ReprintButton = new ByAccessibilityId("Ok");
            CancelButton = new ByAccessibilityId("Cancel");
            WindowSelector = By.Name("Last Printed Receipt");
            ReceiptNumberElement = By.XPath("//*[@Name='Last Printed Receipt']/Edit[1]");
            NumVouchersElement = By.XPath("//*[@Name='Last Printed Receipt']/Edit[2]");
            AmountElement = By.XPath("//*[@Name='Last Printed Receipt']/Edit[3]");
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

        public decimal Amount
        {
            get
            {
                string text = driver.FindElement(AmountElement).Text;

                return decimal.Parse(text);
            }
        }

        public int NumberOfVouchers
        {
            get
            {
                string text = driver.FindElement(NumVouchersElement).Text;

                return int.Parse(text);
            }
        }

        public int ReceiptNumber
        {
            get
            {
                string text = driver.FindElement(ReceiptNumberElement).Text;

                return int.Parse(text);
            }
        }

        public void Reprint()
        {
            if (IsOpen)
            {
                driver.FindElement(ReprintButton).Click();
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
