using NUnit.Framework;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;   //DesiredCapabilities
using System;
using Appium;
using OpenQA.Selenium.Appium;   //Appium Options
using System.Threading;
using OpenQA.Selenium;

namespace POS_Automation

{
    public abstract class BasePage
    {
        protected WindowsDriver<WindowsElement> driver;

        private By ExitConfirmationWindow;
        private ByAccessibilityId CloseWindowButton;
        private By ExitConfirmationConfirmButton;
        private By ExitConfirmationCancelButton;

        public BasePage(WindowsDriver<WindowsElement> _driver)
        {
            driver = _driver;

            ExitConfirmationWindow = By.Name("Confirm Action");
            CloseWindowButton = new ByAccessibilityId("PART_CloseButton");
            ExitConfirmationConfirmButton = By.XPath("//Window[@Name='Confirm Action']/Button[@Name='Yes']");
            ExitConfirmationCancelButton = By.XPath("//Window[@Name='Confirm Action']/Button[@Name='No']");
        }


        public void CloseApplication()
        {
            driver.FindElement(CloseWindowButton).Click();
            driver.FindElement(ExitConfirmationConfirmButton).Click();
        }
    }
}
