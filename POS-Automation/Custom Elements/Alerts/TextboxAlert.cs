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
    public class TextboxAlert : MultiChoiceAlertWindow
    {
        private By TextBoxSelector;
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

        public TextboxAlert(WindowsDriver<WindowsElement> _driver, By windowSelector, By textboxSelector) : base(_driver, windowSelector)
        {
            TextBoxSelector = textboxSelector;
        }

        public void EnterInput(string text)
        {
            wait.Until(d => driver.FindElement(TextBoxSelector));

            driver.FindElement(TextBoxSelector).SendKeys(text);
        }

        public override void Confirm()
        {
            base.Confirm();
            Thread.Sleep(2000);
        }
    }
}
