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

        public BaseTest()
        {

        }



        [SetUp]
        public void Setup()
        {
            SessionManager.Init();
            driver = SessionManager.Driver;
        }


        [TearDown]
        public void EndTest()
        {
            //click exit button
            //SessionManager.CloseDriver();

            //Click yes on confirmation
            //ConfirmCloseApplication();

            //set driver to null
            //SessionManager.QuitDriver();
        }


        //Press Yes button on confirmation prompt asking to confirm close application
        protected void ConfirmCloseApplication()
        {
            string yesBtnXpath = "//Window[@ClassName=\"Window\"][@Name=\"Confirm Action\"]/Button[@Name=\"Yes\"]";
            driver.FindElementByXPath("//Window[@Name='Confirm Action']/Button[@Name='Yes']").Click();
        }
    }
}
