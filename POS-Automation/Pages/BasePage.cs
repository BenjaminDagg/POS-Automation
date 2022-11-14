using NUnit.Framework;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;   //DesiredCapabilities
using System;
using Appium;
using OpenQA.Selenium.Appium;   //Appium Options
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using POS_Automation.Custom_Elements.Alerts;

namespace POS_Automation

{
    public abstract class BasePage
    {
        protected WindowsDriver<WindowsElement> driver;
        protected DefaultWait<WindowsDriver<WindowsElement>> wait;
        protected int DefaultWaitTimeoutSeconds = 10;
        private By ExitConfirmationWindow;
        private ByAccessibilityId CloseWindowButton;
        private By PromptConfirmButton;
        private By PromptCancelButton;
        public NavTabs NavigationTabs;
        public By LogoutButton;
        private By LogoutConfirmationSelector;
        public MultiChoiceAlertWindow LogoutConfirmation;

        public BasePage(WindowsDriver<WindowsElement> _driver)
        {
            driver = _driver;
            wait = new DefaultWait<WindowsDriver<WindowsElement>>(driver);
            wait.Timeout = TimeSpan.FromSeconds(DefaultWaitTimeoutSeconds);
            wait.IgnoreExceptionTypes(typeof(WebDriverException), typeof(InvalidOperationException));

            ExitConfirmationWindow = By.Name("Confirm Action");
            CloseWindowButton = new ByAccessibilityId("PART_CloseButton");
            PromptConfirmButton = By.XPath("//Window[@Name='Confirm Action']/Button[@Name='Yes']");
            PromptCancelButton = By.XPath("//Window[@Name='Confirm Action']/Button[@Name='No']");
            LogoutButton = By.Name("Logout");
            LogoutConfirmationSelector = By.Name("Confirm Action");
            LogoutConfirmation = new MultiChoiceAlertWindow(driver, LogoutConfirmationSelector);


            NavigationTabs = new NavTabs(driver);
        }


        public void CloseApplication()
        {
            driver.FindElement(CloseWindowButton).Click();
            driver.FindElement(PromptConfirmButton).Click();
        }

        public void Logout()
        {
            driver.FindElement(LogoutButton).Click();
            driver.FindElement(LogoutButton).Click();

            try
            {
                wait.Until(d => LogoutConfirmation.IsOpen);
            }
            catch (Exception ex)
            {

            }

            LogoutConfirmation.Confirm();
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

        public bool ErrorIsDisplayed(By elementSelector)
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

        public virtual bool IsReadOnly(By element)
        {
            return driver.FindElement(element).Enabled == false;
        }
    }
}
