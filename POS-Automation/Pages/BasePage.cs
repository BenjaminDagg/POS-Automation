using NUnit.Framework;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;   //DesiredCapabilities
using System;
using Appium;
using OpenQA.Selenium.Appium;   //Appium Options
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace POS_Automation

{
    public abstract class BasePage
    {
        protected WindowsDriver<WindowsElement> driver;
        protected DefaultWait<WindowsDriver<WindowsElement>> wait;
        protected int DefaultWaitTimeoutSeconds = 7;
        private By ExitConfirmationWindow;
        private ByAccessibilityId CloseWindowButton;
        private By ExitConfirmationConfirmButton;
        private By ExitConfirmationCancelButton;
        public NavTabs NavigationTabs;

        public BasePage(WindowsDriver<WindowsElement> _driver)
        {
            driver = _driver;
            wait = new DefaultWait<WindowsDriver<WindowsElement>>(driver);
            wait.Timeout = TimeSpan.FromSeconds(DefaultWaitTimeoutSeconds);
            wait.IgnoreExceptionTypes(typeof(WebDriverException), typeof(InvalidOperationException));

            ExitConfirmationWindow = By.Name("Confirm Action");
            CloseWindowButton = new ByAccessibilityId("PART_CloseButton");
            ExitConfirmationConfirmButton = By.XPath("//Window[@Name='Confirm Action']/Button[@Name='Yes']");
            ExitConfirmationCancelButton = By.XPath("//Window[@Name='Confirm Action']/Button[@Name='No']");

            NavigationTabs = new NavTabs(driver);
        }


        public void CloseApplication()
        {
            driver.FindElement(CloseWindowButton).Click();
            driver.FindElement(ExitConfirmationConfirmButton).Click();
        }

        protected WindowsElement waitForElement(By by, int time)
        {
            WindowsElement element = null;
            int t = 0;
            while (t < time)
            {
                Thread.Sleep(1000);

                try
                {
                    element = driver.FindElement(by);

                    if (element != null)
                    {
                        return element;
                    }
                }
                catch (Exception e)
                {

                }


                t++;
            }

            return element;
        }
    }
}
