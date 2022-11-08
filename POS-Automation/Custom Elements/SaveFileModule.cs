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

namespace POS_Automation.Custom_Elements
{
    public class SaveFileModule
    {
        protected WindowsDriver<WindowsElement> driver;
        protected DefaultWait<WindowsDriver<WindowsElement>> wait;
        protected int DefaultWaitTimeoutSeconds = 5;
        private By WindowSelector;
        private By FilepathEdit;
        private By FilenameEdit;
        private By SaveButton;

        public SaveFileModule(WindowsDriver<WindowsElement> _driver)
        {
            driver = _driver;
            wait = new DefaultWait<WindowsDriver<WindowsElement>>(driver);
            wait.Timeout = TimeSpan.FromSeconds(DefaultWaitTimeoutSeconds);
            wait.IgnoreExceptionTypes(typeof(WebDriverException), typeof(InvalidOperationException));

            WindowSelector = By.XPath("//*[@Name='Save As']");
            FilepathEdit = By.XPath("//*[contains(@Name,'Address')]");
            FilenameEdit = By.XPath("//Edit[@Name='File name:']");
            SaveButton = By.XPath("//*[@Name='Save']");
        }

        public void EnterFilepath(string filepath)
        {
            Thread.Sleep(2000);
            WindowsElement zeroBtn = driver.FindElement(FilepathEdit);
            driver.Mouse.MouseMove(zeroBtn.Coordinates, 300, 20);
            driver.Mouse.Click(null);
            driver.Keyboard.SendKeys(filepath);
            driver.Keyboard.SendKeys(Keys.Enter);
        }

        public void EnterFileName(string filename)
        {
            driver.FindElement(FilenameEdit).Click();
            driver.FindElement(FilenameEdit).SendKeys(Keys.Control + "a");
            driver.FindElement(FilenameEdit).SendKeys(Keys.Backspace);
            driver.FindElement(FilenameEdit).SendKeys(filename);
        }

        public void Save()
        {
            driver.FindElement(WindowSelector).FindElement(SaveButton).Click();
        }
    }
}
