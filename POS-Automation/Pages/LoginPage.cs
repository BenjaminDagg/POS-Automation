using NUnit.Framework;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;   //DesiredCapabilities
using System;
using Appium;
using OpenQA.Selenium.Appium;   //Appium Options
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace POS_Automation
{
    public class LoginPage : BasePage
    {
        private ByAccessibilityId UsernameField;
        private ByAccessibilityId PasswordField;
        private ByAccessibilityId LoginButton;
        private By LoginView;
        private By ErrorButton;

        public LoginPage(WindowsDriver<WindowsElement> _driver) : base(_driver)
        {
            driver = _driver;

            UsernameField = new ByAccessibilityId("UserName");
            PasswordField = new ByAccessibilityId("PasswordTextBox");
            LoginButton = new ByAccessibilityId("Login");
            LoginView = By.ClassName("LoginView");
            ErrorButton = By.XPath("/Pane[@ClassName=\"#32769\"][@Name=\"Desktop 1\"]/Window[@ClassName=\"Window\"][@Name=\"POS\"]/Custom[@ClassName=\"POSMainContentView\"]/Custom[@ClassName=\"LoginView\"]/Button[@AutomationId=\"PART_CloseButton\"]");
        }


        public void EnterUserName(string username)
        {
            waitForElement(UsernameField, 5);
            WindowsElement usernameField = (WindowsElement)wait.Until(d => driver.FindElement(UsernameField));
            usernameField.Clear();
            usernameField.SendKeys(username);
        }


        public void EnterPassword(string password)
        {
            driver.FindElement(PasswordField).SendKeys(password);
        }


        public void ClickLoginButton()
        {
            driver.FindElement(LoginButton).Click();
        }


        public void Login(string username, string password)
        {
            EnterUserName(username);
            EnterPassword(password);
            ClickLoginButton();
        }


        public bool ErrorIsShown()
        {
            try
            {
                Thread.Sleep(2000);
                
                Actions action = new Actions(driver);
                action.MoveByOffset(922,448);
                action.Perform();
                WindowsElement error = driver.FindElement(ErrorButton);
                error.Click();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

    }
}
