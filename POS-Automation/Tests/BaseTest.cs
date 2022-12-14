using NUnit.Framework;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;   //DesiredCapabilities
using System;
using Appium;
using OpenQA.Selenium.Appium;   //Appium Options
using System.Threading;
using OpenQA.Selenium;
using Framework.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using EGMSimulator.Core.Settings;
using POS_Automation.Model;

namespace POS_Automation
{
    public abstract class BaseTest
    {
        protected WindowsDriver<WindowsElement> driver;
        protected NavTabs NavigationTabs;
        protected ILogService _logService;
        protected ServiceProvider ServiceProvider;
        protected DatabaseManager DatabaseManager;

        public BaseTest()
        {

        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            DatabaseManager = new DatabaseManager();

            var services = new ServiceCollection();
            services.AddSingleton<ISimulatorSettings, SimulatorSettings>();

            ServiceProvider = services.BuildServiceProvider();
            _logService = ServiceProvider.GetService<ILogService>();
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
                Thread.Sleep(2000);
                driver.FindElementByXPath("//Window[@ClassName='Window'][@Name='POS']/Window[@Name='Critical Error']/TitleBar[@AutomationId='TitleBar']/Button[@Name='Close']").Click();
            }
            catch (Exception ex)
            {

            }

            //close alert window if open
            try
            {
                Thread.Sleep(1000);
                WindowsElement btn = (WindowsElement)driver.FindElement(By.XPath("//Button[@AutomationId='Yes'] | //Window[@ClassName='Window'][@Name='POS']/Window[@ClassName='Window'][@Name='Error']/Button[@Name='Ok'][@AutomationId='Ok']"));
                btn.Click();
                btn.Click();
            }
            catch (Exception ex)
            {
                
            }

            //CloseApplication();

            SessionManager.Close();
        }


        //Press Yes button on confirmation prompt asking to confirm close application
        protected void CloseApplication()
        {
            string yesBtnXpath = "//Window[@ClassName=\"Window\"][@Name=\"Confirm Action\"]/Button[@Name=\"Yes\"]";

            driver.FindElementByAccessibilityId("PART_CloseButton").Click();
            driver.FindElementByXPath("//Window[@Name='Confirm Action']/Button[@Name='Yes']").Click();
        }
    }
}
