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
using System.Linq;

namespace POS_Automation.Custom_Elements
{
    public class ReportDropdownElement : DropdownElement
    {
        public ReportDropdownElement(By dropdownElement, WindowsDriver<WindowsElement> _driver) : base(dropdownElement, _driver)
        {

        }

        public override List<string> Options
        {
            get
            {
                Click();
                WindowsElement dropdownBtn = (WindowsElement)wait.Until(d => driver.FindElement(DropdownButton));

                var options = driver.FindElements(By.XPath("//Window[@ClassName='Popup']/*[@ClassName='ListBoxItem']"));
                
                return options.Select(opt => opt.Text).ToList();
            }
        }

        public override string? SelectedOption
        {
            get
            {
                Click();
                WindowsElement dropdownBtn = (WindowsElement)wait.Until(d => driver.FindElement(DropdownButton));

                var options = driver.FindElements(By.XPath("//Window[@ClassName='Popup']/*[@ClassName='ListBoxItem']"));

                try
                {
                    foreach (var opt in options)
                    {

                        if (opt.GetAttribute("HasKeyboardFocus") == "True")
                        {
                            string text = opt.FindElement(By.XPath("//Text")).Text;

                            Click();
                            return text;
                        }
                    }

                    //nothing selected
                    Click();
                    return null;
                }

                //error
                catch (Exception ex)
                {
                    Click();
                    return null;
                }
            }
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

        public override void SelectByName(string name)
        {
            WindowsElement dropDownBtn = (WindowsElement)wait.Until(d => d.FindElement(DropdownButton));
            dropDownBtn.Click();

            WindowsElement dropdownList = (WindowsElement)wait.Until(d => d.FindElement(By.ClassName("Popup")));
            var options = dropDownBtn.FindElements(By.ClassName("ListBoxItem"));

            foreach (var option in options)
            {
                string text = option.FindElement(By.XPath("//Text")).Text;


                if (text == name)
                {
                    option.Click();
                    return;
                }
            }

            dropDownBtn.Click();
        }
    }
}
