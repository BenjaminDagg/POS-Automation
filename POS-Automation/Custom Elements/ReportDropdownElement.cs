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
    public class ReportDropdownElement : DropdownElement
    {
        public ReportDropdownElement(By dropdownElement, WindowsDriver<WindowsElement> _driver) : base(dropdownElement, _driver)
        {

        }

        public override void SelectByIndex(int index)
        {
            Click();
            WindowsElement dropdownBtn = (WindowsElement)wait.Until(d => d.FindElement(DropdownButton));
           
            var options = dropdownBtn.FindElements(By.ClassName("ListBoxItem"));
            Console.WriteLine(options.Count);
            try
            {
                options[index].Click();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Select Bank: Option at index {index} not fouind");
                Click();
            }
        }
    }
}
