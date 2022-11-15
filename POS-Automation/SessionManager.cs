using NUnit.Framework;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;   //DesiredCapabilities
using System;
using Appium;
using OpenQA.Selenium.Appium;   //Appium Options
using System.Threading;
using OpenQA.Selenium;
using System.Diagnostics;
using System.Configuration;

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

        private static WindowsDriver<WindowsElement> desktopDriver;

        public static WindowsDriver<WindowsElement> DesktopDriver
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
                string DriverUrl = ConfigurationManager.AppSettings["WinAppDriverUrl"];        //found by starting WinAppDriver.exe
                string AppPath = ConfigurationManager.AppSettings["ApplicationPath"];
                string appName = ConfigurationManager.AppSettings["ApplicationName"];
                string AppDriverPath = @"C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe";


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
                desktopDriver = new WindowsDriver<WindowsElement>(new Uri(DriverUrl), desktopCapabilities);

                //find window of application to test and click the window into focus
                var appWindow = desktopDriver.FindElementByName(appName);
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
            //Close POS app and any sub windows that are open
            foreach(var process in Process.GetProcessesByName("POS"))
            {
                process.Kill();
            }

            //close any open excel files
            foreach (var process in Process.GetProcessesByName("Excel"))
            {
                process.Kill();
            }

            //Close driver (app)
            if (driver != null)
            {
                
                driver.Quit();
            }

            if(desktopDriver != null)
            {
                desktopDriver.Quit();
            }

            driver = null;
            desktopDriver = null;
        }
    }
}
