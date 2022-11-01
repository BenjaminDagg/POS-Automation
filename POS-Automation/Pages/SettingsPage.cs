using NUnit.Framework;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;   //DesiredCapabilities
using System;
using Appium;
using OpenQA.Selenium.Appium;   //Appium Options
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace POS_Automation.Pages
{
    public class SettingsPage : BasePage
    {

        private ByAccessibilityId IpAddressField;
        private ByAccessibilityId PortNumberField;
        private ByAccessibilityId PollIntervalField;

        public SettingsPage(WindowsDriver<WindowsElement> _driver) : base(_driver)
        {
            IpAddressField = new ByAccessibilityId("ServerIPAddress");
            PortNumberField = new ByAccessibilityId("ServerPort");
            PollIntervalField = new ByAccessibilityId("PollingInterval");
        }

        public void EnterIpAddress(string text)
        {
            wait.Until(d => driver.FindElement(IpAddressField));

            driver.FindElement(IpAddressField).Clear();
            driver.FindElement(IpAddressField).SendKeys(text);
        }
    }
}
