using NUnit.Framework;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;   //DesiredCapabilities
using System;
using Appium;
using OpenQA.Selenium.Appium;   //Appium Options
using System.Threading;
using OpenQA.Selenium;
using System.Diagnostics;

namespace POS_Automation
{
    static class SessionManager
    {
        private static WindowsDriver<WindowsElement> driver;

        public static WindowsDriver<WindowsElement> Driver
        {
            get
            {
                if (driver == null)
                {
                    throw new NullReferenceException("Driver has not been initialized. Call Init before referencing Driver.");
                }

                return driver;
            }
        }

        public static void Init()
        {
            if (driver == null)
            {
                string DriverUrl = "http://127.0.0.1:4723";         //found by starting WinAppDriver.exe
                string AppPath = @"C:\Program Files (x86)\Diamond Game Enterprises\MOL POS 20220222.1\POS.exe";
                string appName = "POS";
                string AppDriverPath = @"C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe";

                /*
                var appiumOptions = new AppiumOptions();
                appiumOptions.AddAdditionalCapability("app", AppPath);
                appiumOptions.AddAdditionalCapability("deviceName", "WindowsPC");

                driver = new WindowsDriver<WindowsElement>(new Uri(DriverUrl), appiumOptions);
                */

                //start app
                var proc = Process.Start(AppPath);
                while (string.IsNullOrEmpty(proc.MainWindowTitle))
                {
                    //wait for window to pop up
                    System.Threading.Thread.Sleep(100);
                    proc.Refresh();
                }

                //start appium session of entire desktop
                var desktopCapabilities = new AppiumOptions();
                desktopCapabilities.AddAdditionalCapability("platformName", "Windows");
                desktopCapabilities.AddAdditionalCapability("app", "Root");
                desktopCapabilities.AddAdditionalCapability("deviceName", "WindowsPC");
                var desktopSessions = new WindowsDriver<WindowsElement>(new Uri(DriverUrl), desktopCapabilities);

                //find window of application to test and click the window into focus
                var appWindow = desktopSessions.FindElementByName(appName);
                appWindow.Click();
                var appHandle = appWindow.GetAttribute("NativeWindowHandle");
                appHandle = (int.Parse(appHandle)).ToString("x");

                //start new session for target window
                var appOptions = new AppiumOptions();
                appOptions.AddAdditionalCapability("appTopLevelWindow", appHandle);
                var appSession = new WindowsDriver<WindowsElement>(new Uri(DriverUrl), appOptions);

                driver = appSession;
            }

        }


        public static void Close()
        {

            //Close driver (app)
            if (driver != null)
            {
                //driver.Close();
                driver.Quit();
            }

            driver = null;
        }
    }
}
