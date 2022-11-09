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
using POS_Automation.Custom_Elements.Alerts;
using POS_Automation.Pages.Payout;
using POS_Automation.Model.Payout;
using System.Collections.Generic;
using POS_Automation.Custom_Elements;

namespace POS_Automation.Pages.Reports
{
    public class ReportPage : BasePage
    {
        public ReportMenu ReportMenu;
        public SaveFileModule SaveFileWindow;
        

        public ReportPage(WindowsDriver<WindowsElement> _driver) : base(_driver)
        {
            ReportMenu = new ReportMenu(driver);
            SaveFileWindow = new SaveFileModule(SessionManager.DesktopDriver);
        }
    }
}
