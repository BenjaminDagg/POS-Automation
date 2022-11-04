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

namespace POS_Automation.Pages
{
    public class TransactionList : DataGridPage
    {

        private ByAccessibilityId TransactionDataGrid;
        private By VoucherCountLabel;
        private By TotalPayoutLabel;
        private By RemoveVoucherWindowSelector;
        public MultiChoiceAlertWindow RemoveVoucherPrompt;
        public LastReceiptWindow LastReceiptWindow;
        private ByAccessibilityId LastReceiptButton;

        public TransactionList(WindowsDriver<WindowsElement> _driver) : base(_driver)
        {
            driver = _driver;

            TransactionDataGrid = new ByAccessibilityId("VouchersGrid");
            VoucherCountLabel = By.XPath("//*[contains(@Name,'Voucher Count:')]");
            TotalPayoutLabel = By.XPath("//*[contains(@Name,'Total Payout:')]");

            RemoveVoucherWindowSelector = By.Name("Are You Sure?");
            RemoveVoucherPrompt = new MultiChoiceAlertWindow(driver,RemoveVoucherWindowSelector);
            LastReceiptWindow = new LastReceiptWindow(driver);
            LastReceiptButton = new ByAccessibilityId("ReprintReceipt");
        }

        public override By DataGrid
        {
            get
            {
                return TransactionDataGrid;
            }
        }

        public int VoucherCount
        {
            get
            {
                try
                {
                    wait.Until(d => driver.FindElement(VoucherCountLabel));

                    var text = driver.FindElement(VoucherCountLabel).Text;
                    int colonIndex = text.IndexOf(':');
                    string substr = text.Substring(colonIndex + 1);

                    return int.Parse(substr);

                }
                //list not loaded
                catch (Exception ex)
                {
                    return 0;
                }
            }
        }

        public decimal TotalPayout
        {
            get
            {
                try
                {
                    wait.Until(d => driver.FindElement(TotalPayoutLabel));

                    string text = driver.FindElement(TotalPayoutLabel).Text;
                    int colonIndex = text.IndexOf(':');
                    string amountString = text.Substring(colonIndex + 1).Trim();

                    return decimal.Parse(amountString, NumberStyles.Currency);
                }
                //list not loaded
                catch(Exception ex)
                {
                    return 0;
                }
            }
        }

        public void ReadTable()
        {
            
            var rows = driver.FindElement(DataGrid).FindElements(RowSelector);
            foreach (var row in rows)
            {
                var barcode = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[1]")).Text;
                var amount = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[2]")).Text;
                var id = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[3]")).Text;
                var date = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[4]")).Text;
                var loc = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[5]")).Text;
              

                Console.WriteLine("================");
                Console.WriteLine(barcode);
                Console.WriteLine(amount);
                Console.WriteLine(id);
                Console.WriteLine(date);
               
            }
        }

        public void RemoveVoucherByBarcode(string voucherBarcode)
        {
            var rows = driver.FindElement(DataGrid).FindElements(RowSelector);
            foreach (var row in rows)
            {
                var barcode = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[1]")).Text;

                if(barcode == voucherBarcode)
                {
                    try
                    {
                        var removeBtn = row.FindElement(By.XPath("(.//*[@ClassName='DataGridCell'])[7]/Button"));
                        removeBtn.Click();
                        Thread.Sleep(3000);
                        RemoveVoucherPrompt.Cancel();
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        public void ClickLastReceiptButton()
        {
            driver.FindElement(LastReceiptButton).Click();
        }
    }
}
