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
    public class NavTabs
    {
        private WindowsDriver<WindowsElement> driver;
        protected DefaultWait<WindowsDriver<WindowsElement>> wait;
        protected int DefaultWaitTimeoutSeconds = 7;
        private By PayoutTab;
        private By DeviceManagementTab;
        private By ReportsTab;
        private By SettingsTab;

        public NavTabs(WindowsDriver<WindowsElement> _driver)
        {
            this.driver = _driver;
            wait = new DefaultWait<WindowsDriver<WindowsElement>>(driver);
            wait.Timeout = TimeSpan.FromSeconds(DefaultWaitTimeoutSeconds);
            wait.IgnoreExceptionTypes(typeof(WebDriverException), typeof(InvalidOperationException));
            
            PayoutTab = By.XPath("//*[contains(@Name, 'POS.Modules.Payout.ViewModels.PayoutViewModel')]");
            DeviceManagementTab = By.XPath("//*[contains(@Name, 'POS.Modules.DeviceManagement.ViewModels.DeviceManagementViewMode')]");
            ReportsTab = By.XPath("//*[contains(@Name, 'POS.Modules.Reports.ViewModels.ReportsViewModel')]");
            SettingsTab = By.XPath("//*[contains(@Name, 'POS.Modules.Settings.ViewModels.SettingsViewModel')]");
        }


        public void ClickPayoutTab()
        {
            wait.Until(d => driver.FindElement(PayoutTab));
            driver.FindElement(PayoutTab).Click();
        }

        public void ClickDeviceTab()
        {
            try
            {
                wait.Until(d => driver.FindElement(DeviceManagementTab));
            }
            catch(Exception ex)
            {
                Console.WriteLine("NavTabs.ClickDeviceTab: Timed out waiting for Device Management tab. " + ex.Message);
            }
            driver.FindElement(DeviceManagementTab).Click();
        }

        public void ClickReportsTab()
        {
            wait.Until(d => driver.FindElement(ReportsTab));
            driver.FindElement(ReportsTab).Click();
        }

        public void ClickSetingsTab()
        {
            wait.Until(d => driver.FindElement(SettingsTab));
            driver.FindElement(SettingsTab).Click();
        }

        public bool PayoutIsVisible()
        {
            try
            {
                wait.Until(d => driver.FindElement(PayoutTab));
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public bool SettingsIsVisible()
        {
            try
            {
                wait.Until(d => driver.FindElement(SettingsTab));
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool ReportIsVisible()
        {
            try
            {
                wait.Until(d => driver.FindElement(ReportsTab));
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool DeviceManagementIsVisible()
        {
            try
            {
                wait.Until(d => driver.FindElement(DeviceManagementTab));
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
