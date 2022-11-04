﻿using NUnit.Framework;
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
        private ByAccessibilityId StartingBalanceTextbox;
        private By StartingBalanceWindowSelector;
        public TextboxAlert StartingBalancePrompt;
        private By PayoutButton;
        private By CancelTransactionButton;
        private By CancelTransactionWindowSelector;
        public MultiChoiceAlertWindow CancelTransactionPrompt;

        public PayoutPage(WindowsDriver<WindowsElement> _driver) : base(_driver)
        {
            driver = _driver;

            PayoutButton = By.XPath("//*[@ClassName='CurrentTransactionView']/Button[@Name='Payout']");
            CancelTransactionButton = By.Name("Cancel Transaction");
            StartingBalanceTextbox = new ByAccessibilityId("StartingBalance");
            StartingBalanceWindowSelector = By.Name("Cash Drawer Starting Balance");
            StartingBalancePrompt = new TextboxAlert(driver, StartingBalanceWindowSelector,StartingBalanceTextbox);

            CancelTransactionWindowSelector = By.Name("Confirm Action");
            CancelTransactionPrompt = new MultiChoiceAlertWindow(driver, CancelTransactionWindowSelector);
        }

        public void CancelTransaction()
        {
            driver.FindElement(CancelTransactionButton).Click();
            driver.FindElement(CancelTransactionButton).Click();
            CancelTransactionPrompt.Confirm();
        }

        public void Payout()
        {
            driver.FindElement(PayoutButton).Click();
            
        }
    }
}
