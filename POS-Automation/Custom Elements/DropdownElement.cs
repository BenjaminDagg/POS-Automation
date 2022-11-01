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
    public class DropdownElement
    {
        private WindowsDriver<WindowsElement> driver;
        private WebDriverWait wait;
        public By DropdownButton;

        public DropdownElement(By dropdownElement, WindowsDriver<WindowsElement> _driver)
        {
            this.driver = _driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

            DropdownButton = dropdownElement;
        }


        public List<string> Options
        {
            get
            {
                WindowsElement dropDownBtn = (WindowsElement)wait.Until(d => d.FindElement(DropdownButton));
                dropDownBtn.Click();

                WindowsElement dropdownList = (WindowsElement)wait.Until(d => d.FindElement(By.ClassName("Popup")));
                var options = dropdownList.FindElements(By.ClassName("ListBoxItem"));

                List<string> dropdownOptions = new List<string>();

                foreach (var option in options)
                {
                    var text = option.FindElement(By.XPath("//Text")).Text;
                    dropdownOptions.Add(text);
                }

                //Click away from the dropdown to close it
                dropDownBtn.Click();

                return dropdownOptions;
            }
        }


        public string? SelectedOption
        {
            get
            {
                Thread.Sleep(1000);
                Click();

                WindowsElement dropdownList = (WindowsElement)wait.Until(d => d.FindElement(By.ClassName("Popup")));
                var options = dropdownList.FindElements(By.ClassName("ListBoxItem"));

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


        public void Click()
        {
            WindowsElement dropdownBtn = (WindowsElement)wait.Until(d => d.FindElement(DropdownButton));
            dropdownBtn.Click();
        }


        public void SelectByIndex(int index)
        {
            Click();

            WindowsElement dropdownList = (WindowsElement)wait.Until(d => driver.FindElement(By.ClassName("Popup")));
            var options = dropdownList.FindElements(By.ClassName("ListBoxItem"));

            try
            {
                options[index].FindElement(By.ClassName("TextBlock")).Click();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Select Bank: Option at index {index} not fouind");
                Click();
            }
        }


        public void SelectByName(string name)
        {
            WindowsElement dropDownBtn = (WindowsElement)wait.Until(d => d.FindElement(DropdownButton));
            dropDownBtn.Click();

            WindowsElement dropdownList = (WindowsElement)wait.Until(d => d.FindElement(By.ClassName("Popup")));
            var options = dropdownList.FindElements(By.ClassName("ListBoxItem"));

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


        public void SelectByName(string name, bool exactMatch)
        {
            WindowsElement dropDownBtn = (WindowsElement)wait.Until(d => d.FindElement(DropdownButton));
            dropDownBtn.Click();

            WindowsElement dropdownList = (WindowsElement)wait.Until(d => d.FindElement(By.ClassName("Popup")));
            var options = dropdownList.FindElements(By.ClassName("ListBoxItem"));

            foreach (var option in options)
            {
                string text = option.FindElement(By.XPath("//Text")).Text;

                if (exactMatch == true)
                {
                    if (text == name)
                    {
                        option.Click();
                        return;
                    }
                }
                else
                {
                    if (text.ToLower().Contains(name.ToLower()))
                    {
                        option.Click();
                        return;
                    }
                }


            }

            dropDownBtn.Click();
        }
    }
}
