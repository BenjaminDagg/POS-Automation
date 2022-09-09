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

//https://diamondgame.visualstudio.com/Diamond%20Game%20Portfolio/_git/AppDev_MOLite?path=/POS/POS/Modules/DeviceManagement/Views/DeviceManagementView.xaml&version=GBPOS_NewTheme&_a=contents

namespace POS_Automation
{
    public class DeviceManagementPage : BasePage
    {

        private By DeviceDataGrid;

        public DeviceManagementPage(WindowsDriver<WindowsElement> _driver) : base(_driver)
        {
            driver = _driver;

            DeviceDataGrid = By.ClassName("DataGrid");
        }


        public void DisplayDeviceList()
        {
            
            WindowsElement deviceList = driver.FindElement(DeviceDataGrid);
            var rows = deviceList.FindElements(By.XPath(".//DataItem"));
            Console.WriteLine(rows.Count);
            
            foreach (var row in rows)
            {
                Console.WriteLine("============");
                var icon = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[1]"));
                var res = driver.ExecuteScript("var items = {}; for (index = 0; index < arguments[0].attributes.length; ++index) { items[arguments[0].attributes[index].name] = arguments[0].attributes[index].value }; return items;", icon);
                
                //Console.WriteLine(icon.GetProperty("Foreground"));
                var machNo = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[2]")).Text;
                Console.WriteLine(machNo);
            }
           
        }
    }
}
