using NUnit.Framework;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;   //DesiredCapabilities
using System;
using Appium;
using OpenQA.Selenium.Appium;   //Appium Options
using System.Threading;
using OpenQA.Selenium;
using System.Collections;
using OpenQA.Selenium.Interactions;
using POS_Automation.Pages;
using POS_Automation.Custom_Elements.Alerts;
using System.Text.RegularExpressions;
using System.Globalization;
using POS_Automation.Model;

namespace POS_Automation.Pages.Payout
{
    internal class VoucherNumPad : BasePage
    {
        private ByAccessibilityId NumPadTextBox;
        private By ClearButton;
        private By EnterButton;
        private By BackspaceButton;

        public VoucherNumPad(WindowsDriver<WindowsElement> _driver) : base(_driver)
        {
            driver = _driver;

            NumPadTextBox = new ByAccessibilityId("VoucherNumberTextBox");
            ClearButton = By.Name("Clear");
            EnterButton = By.Name("Enter");
            BackspaceButton = By.XPath("//Button[@Name='9']/following-sibling::Button");
        }

        public void PressNumKey(int num)
        {
            if (num < 0 || num > 9)
            {
                return;
            }

            wait.Until(d => driver.FindElement(NumPadTextBox));

            string btnXpath = $"//Button[@Name='{num.ToString()}']";

            //zero button hitbox is larger than the button appears. By default it clicks in the middle
            //which misses the actual button. Click on the left side of the button
            if (num == 0)
            {
                WindowsElement zeroBtn = driver.FindElement(By.XPath(btnXpath));
                driver.Mouse.MouseMove(zeroBtn.Coordinates, 24, 24);
                driver.Mouse.Click(null);

                return;
            }
            else
            {
                try
                {
                    driver.FindElement(By.XPath(btnXpath)).Click();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in VoucherNumPad.PressNumKey: Couldn't find button with number " + num);
                }
            }
        }

        public void EnterBarcode(string text)
        {
            wait.Until(d => driver.FindElement(NumPadTextBox));

            driver.FindElement(NumPadTextBox).Clear();
            driver.FindElement(NumPadTextBox).SendKeys(text);

            try
            {
                Thread.Sleep(1000);
                wait.Until(d => driver.FindElement(NumPadTextBox).Text.Length == 0);
            }
            catch(Exception ex)
            {

            }
        }

        public string GetBarcode()
        {
            wait.Until(d => driver.FindElement(NumPadTextBox));

            return driver.FindElement(NumPadTextBox).Text;
        }

        public void Clear()
        {
            wait.Until(d => driver.FindElement(ClearButton));
            driver.FindElement(ClearButton).Click();
        }

        public void PressEnter()
        {
            wait.Until(d => driver.FindElement(EnterButton));
            driver.FindElement(EnterButton).Click();
        }

        public void PressBackspace()
        {
            wait.Until(d => driver.FindElement(BackspaceButton));
            driver.FindElement(BackspaceButton).Click();
        }
    }
}
