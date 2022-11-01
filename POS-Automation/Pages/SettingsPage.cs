using NUnit.Framework;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;   //DesiredCapabilities
using System;
using Appium;
using OpenQA.Selenium.Appium;   //Appium Options
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using POS_Automation.Custom_Elements;

namespace POS_Automation.Pages
{
    public class SettingsPage : BasePage
    {

        public ByAccessibilityId IpAddressField;
        public ByAccessibilityId PortNumberField;
        public ByAccessibilityId PollIntervalField;
        private By PrinterSaveButton;
        private By SettingsSaveButon;
        private By PrinterElement;
        public DropdownElement PrinterDropdown;

        public SettingsPage(WindowsDriver<WindowsElement> _driver) : base(_driver)
        {
            IpAddressField = new ByAccessibilityId("ServerIPAddress");
            PortNumberField = new ByAccessibilityId("ServerPort");
            PollIntervalField = new ByAccessibilityId("PollingInterval");
            PrinterSaveButton = By.XPath("(//Button[@AutomationId='Save'])[1]");
            SettingsSaveButon = By.XPath("(//Button[@AutomationId='Save'])[2]");
            PrinterElement = By.ClassName("ComboBox");

            PrinterDropdown = new DropdownElement(PrinterElement,driver);
        }

        public void EnterIpAddress(string text)
        {
            wait.Until(d => driver.FindElement(IpAddressField));

            driver.FindElement(IpAddressField).Clear();
            driver.FindElement(IpAddressField).SendKeys(text);
        }

        public string GetIpAddress()
        {
            wait.Until(d => driver.FindElement(IpAddressField));

            return driver.FindElement(IpAddressField).Text;
        }

        public void EnterPortNumber(string text)
        {
            wait.Until(d => driver.FindElement(PortNumberField));

            driver.FindElement(PortNumberField).Clear();
            driver.FindElement(PortNumberField).SendKeys(text);
        }

        public int GetPortNumber()
        {
            wait.Until(d => driver.FindElement(PortNumberField));

            return int.Parse(driver.FindElement(PortNumberField).Text);
        }

        public void EnterPollInterval(string text)
        {
            wait.Until(d => driver.FindElement(PollIntervalField));

            driver.FindElement(PollIntervalField).Clear();
            driver.FindElement(PollIntervalField).SendKeys(text);
        }

        public int GetPollingInterval()
        {
            wait.Until(d => driver.FindElement(PollIntervalField));

            return int.Parse(driver.FindElement(PollIntervalField).Text);
        }

        public void SavePrinterSettings()
        {
            wait.Until(d => driver.FindElement(PrinterSaveButton));
            driver.FindElement(PrinterSaveButton).Click();
        }

        public void SaveDeviceSettings()
        {
            wait.Until(d => driver.FindElement(SettingsSaveButon));
            driver.FindElement(SettingsSaveButon).Click();
        }
    }
}
