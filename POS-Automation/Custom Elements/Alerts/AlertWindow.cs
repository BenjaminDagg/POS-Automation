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

namespace POS_Automation.Custom_Elements.Alerts
{
    public class AlertWindow
    {
        protected WindowsDriver<WindowsElement> driver;
        protected DefaultWait<WindowsDriver<WindowsElement>> wait;
        protected int WAIT_TIMEOUT_SEC = 8;
        protected virtual By Window { get; set; }
        protected virtual ByAccessibilityId CloseButton { get; set; }

        public AlertWindow(WindowsDriver<WindowsElement> _driver, By windowSelector)
        {
            this.driver = _driver;
            wait = new DefaultWait<WindowsDriver<WindowsElement>>(driver);
            wait.Timeout = TimeSpan.FromSeconds(WAIT_TIMEOUT_SEC);
            wait.IgnoreExceptionTypes(typeof(WebDriverException), typeof(InvalidOperationException));

            Window = windowSelector;
            CloseButton = new ByAccessibilityId("PART_CloseButton");
        }


        public virtual bool IsOpen
        {
            get
            {
                try
                {

                    Thread.Sleep(1000);
                    wait.Until(d =>
                    {
                        WindowsElement element = driver.FindElement(Window);

                        return element != null;
                    });
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }


        public virtual string AlertText
        {
            get
            {

                return driver.FindElement(Window).FindElement(By.XPath(".//Text[2]")).Text;
            }
        }


        protected virtual void WaitForWindowOpen()
        {
            try
            {
                wait.Until(d => driver.FindElement(Window));

            }
            catch (Exception ex)
            {

            }
        }


        public void CloseWindow()
        {
            driver.FindElement(CloseButton).Click();
        }
    }
}