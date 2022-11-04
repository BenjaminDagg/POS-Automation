using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_Automation.Custom_Elements.Alerts
{
    //Alert window with only 1 choice: OK button
    public class SingleChoiceAlertWindow : AlertWindow
    {

        private ByAccessibilityId ConfirmButton;

        public SingleChoiceAlertWindow(WindowsDriver<WindowsElement> _driver, By windowSelector) : base(_driver, windowSelector)
        {
            this.driver = _driver;

            Window = windowSelector;
            ConfirmButton = new ByAccessibilityId("Ok");
        }


        public void Confirm()
        {
            WaitForWindowOpen();
            driver.FindElement(ConfirmButton).Click();
        }
    }
}
