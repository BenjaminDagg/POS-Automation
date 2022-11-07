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
    public class SupervisorApprovalPrompt : MultiChoiceAlertWindow
    {

        public ByAccessibilityId UsernameTextbox;
        public ByAccessibilityId PasswordTextbox;

        public SupervisorApprovalPrompt(WindowsDriver<WindowsElement> _driver, By windowSelector) : base(_driver, windowSelector)
        {
            driver = _driver;

            UsernameTextbox = new ByAccessibilityId("UserName");
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

        public void EnterUsername(string text)
        {
            wait.Until(d => driver.FindElement(UsernameTextbox));
            driver.FindElement(UsernameTextbox).SendKeys(text);
        }

        public void EnterPassword(string text)
        {
            wait.Until(d => driver.FindElement(PasswordTextbox));
            driver.FindElement(PasswordTextbox).SendKeys(text);
        }

        public override void Confirm()
        {
            base.Confirm();
            Thread.Sleep(1000);
        }

        private bool ErrorIsDisplayed(By elementSelector)
        {
            Thread.Sleep(1000);
            try
            {
                WindowsElement element = (WindowsElement)wait.Until(d => driver.FindElement(elementSelector));
                string helpText = element.GetAttribute("HelpText");

                if (string.IsNullOrEmpty(helpText))
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UsernameErrorIsDisplayed()
        {
            return ErrorIsDisplayed(UsernameTextbox);
        }

        public bool PasswordErrorIsDisplayed()
        {
            return ErrorIsDisplayed(PasswordTextbox);
        }
    }
}
