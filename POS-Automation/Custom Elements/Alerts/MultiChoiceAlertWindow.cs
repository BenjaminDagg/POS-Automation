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
    //Alert window containing two buttons: Yes or No
    public class MultiChoiceAlertWindow : AlertWindow
    {

        protected virtual By ConfirmButton { get; set; }
        protected virtual By CancelButton { get; set; }

        public MultiChoiceAlertWindow(WindowsDriver<WindowsElement> _driver, By windowSelector) : base(_driver, windowSelector)
        {
            this.driver = _driver;

            Window = windowSelector;
            ConfirmButton = new ByAccessibilityId("Yes");
            CancelButton = new ByAccessibilityId("No");
        }


        public virtual void Confirm()
        {
            WindowsElement confirmBtn = (WindowsElement)wait.Until(d => driver.FindElement(ConfirmButton));
            confirmBtn.Click();
        }


        public virtual void Cancel()
        {
            WindowsElement cancelBtn = (WindowsElement)wait.Until(d => driver.FindElement(CancelButton));
            cancelBtn.Click();
        }

    }
}
