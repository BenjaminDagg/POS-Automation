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

//https://diamondgame.visualstudio.com/Diamond%20Game%20Portfolio/_git/AppDev_MOLite?path=/POS/POS/Modules/DeviceManagement/Views/DeviceManagementView.xaml&version=GBPOS_NewTheme&_a=contents

namespace POS_Automation
{
    public class DeviceManagementPage : DataGridPage
    {

        public By DeviceDataGrid
        {
            get
            {
                return DataGrid;
            }

            private set { DataGrid = value; }
        }

        public By SetAllOnlineButton;
        public By SetAllOfflineButton;
        private ByAccessibilityId TogglePromoButton;
        private By ServerLabel;
        private By PollIntervalLabel;
        private By ConnectionStatusLabel;
        private By MachCountLabel;
        private By AlertWindowSelector;
        public MultiChoiceAlertWindow Alert;
        private By ChangeStatusAlertWindowSelector;
        public MultiChoiceAlertWindow ChangeStatusAlert;
        private ByAccessibilityId ReconnectButton;

        public DeviceManagementPage(WindowsDriver<WindowsElement> _driver) : base(_driver)
        {
            driver = _driver;

            DeviceDataGrid = By.ClassName("DataGrid");
            SetAllOnlineButton = By.Name("Set All Online");
            SetAllOfflineButton = By.Name("Set All Offline");
            TogglePromoButton = new ByAccessibilityId("TogglePromoTicketBtn");
            ServerLabel = By.XPath("//*[contains(@Name,'Server:')]");
            PollIntervalLabel = By.XPath("//*[contains(@Name,'Refresh')]");
            ConnectionStatusLabel = By.XPath("//*[contains(@Name,'Status:')]");
            MachCountLabel = By.XPath("//*[contains(@Name,'Machine Count:')]");
            ReconnectButton = new ByAccessibilityId("ToggleConnectionBtn");

            //user for toggling promo
            AlertWindowSelector = By.Name("Please Confirm");
            Alert = new MultiChoiceAlertWindow(driver, AlertWindowSelector);

            //used for setting machine offline/online
            ChangeStatusAlertWindowSelector = By.Name("Are you sure?");
            ChangeStatusAlert = new MultiChoiceAlertWindow(driver,ChangeStatusAlertWindowSelector);
        }


        public string Server
        {
            get
            {
                wait.Until(d => driver.FindElement(ServerLabel));

                string text = driver.FindElement(ServerLabel).Text;
                int colonIndex = text.IndexOf(':');
                string ip = text.Substring(colonIndex + 1).Trim();
                return ip;
            }
        }

        public string IpAddress
        {
            get
            {
                wait.Until(d => driver.FindElement(ServerLabel));

                string text = driver.FindElement(ServerLabel).Text;
                int startIndex = text.IndexOf(':');
                int endIndex  = text.LastIndexOf(':');
                int length = (endIndex - startIndex) - 1;
                string ip = text.Substring(startIndex + 1,length).Trim();
                return ip;
            }
        }

        public int Port
        {
            get
            {
                string server = Server;
                int colonIndex = server.IndexOf(':');
                string port = server.Substring(colonIndex + 1,4).Trim();

                return int.Parse(port);
            }
        }

        public int PollingInterval
        {
            get
            {
                wait.Until(d => driver.FindElement(PollIntervalLabel));

                string text = driver.FindElement(PollIntervalLabel).Text;
                int res = int.Parse(Regex.Match(text, @"\d+").Value, NumberFormatInfo.InvariantInfo);

                return res;
            }
        }

        public string ConnectionStatus
        {
            get
            {
                wait.Until(d => driver.FindElement(ConnectionStatusLabel));

                string text = driver.FindElement(ConnectionStatusLabel).Text;
                int colonIndex = text.IndexOf(':');
                string status = text.Substring(colonIndex + 1).Trim();
                return status;
            }
        }

        public int MachineCount
        {
            get
            {
                wait.Until(d => driver.FindElement(MachCountLabel));

                string text = driver.FindElement(MachCountLabel).Text;
                int res = int.Parse(Regex.Match(text, @"\d+").Value, NumberFormatInfo.InvariantInfo);

                return res;
            }
        }

        public void SetAllOnline()
        {
            wait.Until(d => d.FindElement(SetAllOnlineButton));

            if (!IsReadOnly(SetAllOnlineButton))
            {
                driver.FindElement(SetAllOnlineButton).Click();
                ChangeStatusAlert.Confirm();
            }
        }

        public void SetAllOffline()
        {
            wait.Until(d => d.FindElement(SetAllOfflineButton));

            if (!IsReadOnly(SetAllOfflineButton))
            {
                driver.FindElement(SetAllOfflineButton).Click();
                ChangeStatusAlert.Confirm();
            }
        }

        public void TurnPromoTicketsOn()
        {
            WindowsElement btn = wait.Until(d => d.FindElement(TogglePromoButton));

            if(btn.Text == "Turn Promo Ticket On")
            {
                btn.Click();
                Alert.Confirm();

                Thread.Sleep(1000);
            }
        }

        public void TurnPromoTicketsOff()
        {
            WindowsElement btn = wait.Until(d => d.FindElement(TogglePromoButton));

            if (btn.Text == "Turn Promo Ticket Off")
            {
                btn.Click();
                Alert.Confirm();

                Thread.Sleep(1000);
            }
        }

        public void ClickTogglePromoButton()
        {
            WindowsElement btn = wait.Until(d => d.FindElement(TogglePromoButton));

            btn.Click();
        }

        public bool PromoToggleButtonIsVisible()
        {
            try
            {
                wait.Until(d => d.FindElement(TogglePromoButton));

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool PromoTicketsAreEnabled()
        {
            WindowsElement btn = wait.Until(d => d.FindElement(TogglePromoButton));

            if (btn.Text == "Turn Promo Ticket Off")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string PromoToggleButtonText()
        {
            try
            {
                WindowsElement btn = wait.Until(d => d.FindElement(TogglePromoButton));

                return btn.Text;
            }
            catch (Exception ex)
            {
                return String.Empty;
            }
        }

        public Machine GetMachineByMachNo(string machNo)
        {
            wait.Until(d => driver.FindElements(By.ClassName("DataGridRow")).Count > 0);
            var rows = driver.FindElements(By.ClassName("DataGridRow"));

            var machine = new Machine();

            foreach (var row in rows)
            {
                var currMachNo = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[2]")).Text;

                if(currMachNo == machNo)
                {
                    machine.MachNo = currMachNo;

                    var ipAddress = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[3]")).Text;
                    machine.IpAddress = ipAddress;

                    var dateString = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[4]")).Text;
                    var date = DateTime.ParseExact(dateString, "MM/dd/yyyy hh:mm tt",CultureInfo.InvariantCulture);
                    machine.LastPlayed = date;

                    var tType = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[5]")).Text;
                    if (!string.IsNullOrEmpty(tType))
                    {
                        machine.TransType = tType[0];
                    }

                    var desc = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[6]")).Text;
                    machine.Description = desc;

                    var balance = decimal.Parse(row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[7]")).Text, NumberStyles.Currency);
                    machine.Balance = balance;

                    //check if set offline/online button is on the row. If it is then set status to whatever the button text is.
                    //If not then machine is offline
                    try
                    {
                        WindowsElement statusBtn = (WindowsElement)row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[8]/Button"));

                        string statusText = statusBtn.Text;
                        machine.Status = statusText == "Set Offline" ? true : false;
                        
                    }
                    catch(Exception ex)
                    {
                        machine.Status = false;
                    }
                }
            }

            return machine;
        }

        public void SetMachineOffline(string machNo)
        {
            wait.Until(d => driver.FindElements(By.ClassName("DataGridRow")).Count > 0);
            var rows = driver.FindElements(By.ClassName("DataGridRow"));

            var machine = new Machine();

            foreach (var row in rows)
            {
                var currMachNo = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[2]")).Text;

                if (currMachNo == machNo)
                {
                    
                    //check if set offline/online button is on the row. If it is then set status to whatever the button text is.
                    //If not then machine is offline
                    try
                    {
                        WindowsElement statusBtn = (WindowsElement)row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[8]/Button"));

                        string statusText = statusBtn.Text;

                        if(statusText == "Set Offline")
                        {
                            statusBtn.Click();
                            ChangeStatusAlert.Confirm();
                        }
                        

                    }
                    catch (Exception ex)
                    {
                        
                    }
                }
            }
        }

        public void SetMachineOnline(string machNo)
        {
            wait.Until(d => driver.FindElements(By.ClassName("DataGridRow")).Count > 0);
            var rows = driver.FindElements(By.ClassName("DataGridRow"));

            var machine = new Machine();

            foreach (var row in rows)
            {
                var currMachNo = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[2]")).Text;

                if (currMachNo == machNo)
                {

                    //check if set offline/online button is on the row. If it is then set status to whatever the button text is.
                    //If not then machine is offline
                    try
                    {
                        WindowsElement statusBtn = (WindowsElement)row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[8]/Button"));

                        string statusText = statusBtn.Text;

                        if (statusText == "Set Online")
                        {
                            statusBtn.Click();
                            ChangeStatusAlert.Confirm();
                        }


                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        public bool MachineStatus(string machNo)
        {
            wait.Until(d => driver.FindElements(By.ClassName("DataGridRow")).Count > 0);
            var rows = driver.FindElements(By.ClassName("DataGridRow"));

            var machine = new Machine();

            foreach (var row in rows)
            {
                var currMachNo = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[2]")).Text;

                if (currMachNo == machNo)
                {

                    //check if set offline/online button is on the row. If it is then set status to whatever the button text is.
                    //If not then machine is offline
                    try
                    {
                        WindowsElement statusBtn = (WindowsElement)row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[8]/Button"));

                        string statusText = statusBtn.Text;

                        if (statusText == "Set Online")
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }

                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        public bool MachineStatus(int index)
        {
            wait.Until(d => driver.FindElements(By.ClassName("DataGridRow")).Count > 0);
            var rows = driver.FindElements(By.ClassName("DataGridRow"));

            var machine = new Machine();

            int rowCount = 0;
            foreach (var row in rows)
            {
                var currMachNo = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[2]")).Text;

                if (rowCount == index)
                {

                    //check if set offline/online button is on the row. If it is then set status to whatever the button text is.
                    //If not then machine is offline
                    try
                    {
                        WindowsElement statusBtn = (WindowsElement)row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[8]/Button"));

                        string statusText = statusBtn.Text;

                        if (statusText == "Set Online")
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }

                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }

                rowCount++;
            }

            return false;
        }

        public string MachineStatusButtonText(string machNo)
        {
            wait.Until(d => driver.FindElements(By.ClassName("DataGridRow")).Count > 0);
            var rows = driver.FindElements(By.ClassName("DataGridRow"));

            var machine = new Machine();
            string text = string.Empty;

            foreach (var row in rows)
            {
                var currMachNo = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[2]")).Text;

                if (currMachNo == machNo)
                {

                    //check if set offline/online button is on the row. If it is then set status to whatever the button text is.
                    //If not then machine is offline
                    try
                    {
                        WindowsElement statusBtn = (WindowsElement)row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[8]/Button"));

                        string statusText = statusBtn.Text;

                        text = statusText;

                    }
                    catch (Exception ex)
                    {
                        
                    }
                }
            }

            return text;
        }

        public void ClickMachineActionButton(string machNo)
        {
            wait.Until(d => driver.FindElements(By.ClassName("DataGridRow")).Count > 0);
            var rows = driver.FindElements(By.ClassName("DataGridRow"));

            var machine = new Machine();
            string text = string.Empty;

            foreach (var row in rows)
            {
                var currMachNo = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[2]")).Text;

                if (currMachNo == machNo)
                {

                    //check if set offline/online button is on the row. If it is then set status to whatever the button text is.
                    //If not then machine is offline
                    try
                    {
                        WindowsElement statusBtn = (WindowsElement)row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[8]/Button"));

                        statusBtn.Click();
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        public string MachineStatusButtonText(int index)
        {
            wait.Until(d => driver.FindElements(By.ClassName("DataGridRow")).Count > 0);
            var rows = driver.FindElements(By.ClassName("DataGridRow"));

            var machine = new Machine();
            string text = string.Empty;

            int rowCount = 0;
            foreach (var row in rows)
            {
                var currMachNo = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[2]")).Text;

                if (rowCount == index)
                {

                    //check if set offline/online button is on the row. If it is then set status to whatever the button text is.
                    //If not then machine is offline
                    try
                    {
                        WindowsElement statusBtn = (WindowsElement)row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[8]/Button"));

                        string statusText = statusBtn.Text;

                        text = statusText;

                    }
                    catch (Exception ex)
                    {

                    }
                }

                rowCount++;
            }

            return text;
        }

        public bool ReconnectBtnIsVisible()
        {
            try
            {
                wait.Until(d => d.FindElement(ReconnectButton));

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
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
                //var res = driver.ExecuteScript("var items = {}; for (index = 0; index < arguments[0].attributes.length; ++index) { items[arguments[0].attributes[index].name] = arguments[0].attributes[index].value }; return items;", icon);
                
                //Console.WriteLine(icon.GetProperty("Foreground"));
                var machNo = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[2]")).Text;
                Console.WriteLine(machNo);
            }
           
        }
    }
}
