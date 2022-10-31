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
    public abstract class BaseTest
    {
        protected WindowsDriver<WindowsElement> driver;
        protected NavTabs NavigationTabs;

        public BaseTest()
        {

        }



        [SetUp]
        public void Setup()
        {
            SessionManager.Init();
            driver = SessionManager.Driver;

            NavigationTabs = new NavTabs(driver);
        }


        [TearDown]
        public void EndTest()
        {
            //close error window if open
            try
            {
                Thread.Sleep(1000);
                driver.FindElementByXPath("//Window[@ClassName='Window'][@Name='POS']/Window[@Name='Critical Error']/TitleBar[@AutomationId='TitleBar']/Button[@Name='Close']").Click();
            }
            catch (Exception ex)
            {

            }

            //close alert window if open
            try
            {
                driver.FindElementByAccessibilityId("PART_CloseButton").Click();
            }
            catch (Exception ex)
            {

            }

            ConfirmCloseApplication();

            SessionManager.Close();
        }


        //Press Yes button on confirmation prompt asking to confirm close application
        protected void ConfirmCloseApplication()
        {
            string yesBtnXpath = "//Window[@ClassName=\"Window\"][@Name=\"Confirm Action\"]/Button[@Name=\"Yes\"]";
            driver.FindElementByXPath("//Window[@Name='Confirm Action']/Button[@Name='Yes']").Click();
        }
    }
}
