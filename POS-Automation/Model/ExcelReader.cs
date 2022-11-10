using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POS_Automation.Model.Reports;
using Excel = Microsoft.Office.Interop.Excel;
using POS_Automation.Model.Reports;

namespace POS_Automation.Model
{
    public class ExcelReader
    {
        private Excel.Application xlApp;
        private Excel.Workbook xlWorkbook;
        private Excel.Worksheet xlWorksheet;
        private Excel.Range xlRange;
        string filename;

        public ExcelReader()
        {
            xlApp = new Excel.Application();
            filename = string.Empty;
        }

        public int RowCount
        {
            get
            {
                int recordCount = xlWorksheet.Cells.Find(
                What: "*",
                SearchOrder: Excel.XlSearchOrder.xlByRows,
                SearchDirection: Excel.XlSearchDirection.xlPrevious,
                MatchCase: false
            ).Row;

                return recordCount;
            }
        }

        public void Open(string filepath)
        {
            filename = filepath;

            xlWorkbook = xlApp.Workbooks.Open(filepath);
            xlWorksheet = xlWorkbook.Sheets[1];
            xlRange = xlWorksheet.UsedRange;
        }

        public string ReadCell(int row,int col)
        {
            try
            {
                var val = xlRange.Cells[row,col].Value2.ToString();
                return val;
            }
            catch (Exception ex)
            {
                return String.Empty;
            }
        }

        public DailyCashierActivityReport<DailyCashierActivityReportRecord> ParseCashierActivityReport()
        {
            var report = new DailyCashierActivityReport<DailyCashierActivityReportRecord>();
            var records = new List<DailyCashierActivityReportRecord>();

            int startRow = RowNum("Created By");

            var title = ReadCell(Report<DailyCashierActivityReportRecord>.TitleCell.Row, Report<DailyCashierActivityReportRecord>.TitleCell.Col);
            report.Title = title;

            var date = ReadCell(Report<DailyCashierActivityReportRecord>.RuntimeCell.Row, Report<DailyCashierActivityReportRecord>.RuntimeCell.Col);
            

            var period = ReadCell(Report<DailyCashierActivityReportRecord>.PeriodCell.Row, Report<DailyCashierActivityReportRecord>.PeriodCell.Col);
            report.ReportPeriod = period;

            //no data found in report. Fill out only title and period then return report with no data
            if (startRow == -1)
            {
                report.RunDate = DateTime.Now;
                report.Data = records;
                return report;
            }

            date = date.Replace("\n", "");
            date = date.Replace("Run Date/Time", "");
            report.RunDate = DateTime.Parse(date);

            string totalVouchersString = ReadCell(xlRange.Rows.Count - 1, 4);
            totalVouchersString = totalVouchersString.Substring(totalVouchersString.IndexOf(':') + 1);
            report.TotalVouchers = int.Parse(totalVouchersString);

            decimal totalPayout = decimal.Parse(ReadCell(xlRange.Rows.Count - 1, 5));
            report.TotalAmount = totalPayout;

            string totalTransaction = ReadCell(xlRange.Rows.Count - 1, 6);
            totalTransaction = totalTransaction.Substring(totalTransaction.IndexOf(':') + 1);
            report.TotalTransactions = int.Parse(totalTransaction);

            
            for (int i = startRow + 1; i < xlRange.Rows.Count - 2; i++)
            {

                DailyCashierActivityReportRecord record = new DailyCashierActivityReportRecord();

                //Created by reached a new user
                var user = ReadCell(i, 1);
                record.CreatedBy = user;
                var sessionUser = ReadCell(i, 2);
                var activities = new List<CashierActivityRecord>();

                int j = i;

                //loop through rows until the 2nd row displays 'Total'
                //Thats how I know a new user it about to be reached
                while(ReadCell(j,2) != "Total")
                {
                    var activity = new CashierActivityRecord();

                    activity.CreatedBy = user;
                    if(ReadCell(j,2) == string.Empty)
                    {
                        activity.SessionId = sessionUser;
                    }
                    else
                    {
                        sessionUser = ReadCell(j,2);
                        activity.SessionId = sessionUser;
                    }
 
                    activity.Station = ReadCell(j,3);
                    activity.VoucherNumber = ReadCell(j,4);
                    activity.PayoutAmount = decimal.Parse(ReadCell(j,5));
                    activity.ReceiptNumber = int.Parse(ReadCell(j,6));
                    activity.Date = DateTime.Parse(ReadCell(j,7));

                    activities.Add(activity);

                    j++;
                }
                string totalVouchers = ReadCell(j, 4);
                
                totalVouchers = totalVouchers.Substring(totalVouchers.IndexOf(':') + 1);
                record.TotalVouchers = int.Parse(totalVouchers);

                record.TotalAmount = decimal.Parse(ReadCell(j, 5).Replace("\n", ""),NumberStyles.Currency);

                string totalTransactions = ReadCell(j, 6);
                totalTransactions = totalTransactions.Substring(totalTransactions.IndexOf(':') + 1);
                record.TotalTransactions = int.Parse(totalTransactions);

                record.Activities = activities;
                records.Add(record);
                i = j;
            }

            report.Data = records;
            return report;
        }

        public CashBankActivityReport<CashBankActivityReportRecord> ParseCashBankActivityReport()
        {
            var report = new CashBankActivityReport<CashBankActivityReportRecord>();
            var records = new List<CashBankActivityReportRecord>();

            int startRow = RowNum("Created By");    //find row number that has "Created By" to start

           
            var title = ReadCell(Report<DailyCashierActivityReportRecord>.TitleCell.Row, Report<DailyCashierActivityReportRecord>.TitleCell.Col);
            report.Title = title;

            var period = ReadCell(6, 6);
            report.ReportPeriod = period;

            if (startRow == -1)
            {
                report.RunDate = DateTime.Now;
                report.Data = records;
                return report;
            }

            var date = ReadCell(2, 12);
            date = date.Replace("\n", "");
            date = date.Replace("Run Date/Time", "");
            report.RunDate = DateTime.Parse(date);

            //record header info and totals
            decimal totalReportAmount = decimal.Parse(ReadCell(xlRange.Rows.Count - 3, 11));
            report.TotalMoney = totalReportAmount;

            decimal totalReportPayout = decimal.Parse(ReadCell(xlRange.Rows.Count - 3, 13));
            report.TotalPayout = totalReportPayout;

            //stores last user and last session while iterating throgugh file
            string fUser = string.Empty;
            string fSession = string.Empty;

            var allSessions = new List<CashBankActivitySession>(); //parses all sessions in the list
            var users = new List<CashBankActivityReportRecord>(); //stores each unique cashier username and all of their sessions
            
            
            for (int i = startRow + 1; i < xlRange.Rows.Count - 1; i++)
            {

                var s = ReadCell(i,4);
                
                //new session started
                if(s != fSession && s != String.Empty)
                {
                    
                    fSession = s;
                    var newSession = new CashBankActivitySession();
                    newSession.SessionId = s;

                    int j = i;

                    //go down list until reach total to begin new session
                    while(ReadCell(j,5) != "Totals:")
                    {
                        //each row in a session is a new transaction add it to session transactions
                        var trans = new CashBankActivityTransaction();

                        trans.SessionId = fSession;
                        trans.TransType = ReadCell(j, 9);
                        trans.Station = ReadCell(j, 5);
                        trans.VoucherNumber = ReadCell(j, 7);
                        if(ReadCell(j, 8) != string.Empty)
                        {
                            trans.ReferenceNumber = int.Parse(ReadCell(j, 8));
                        }
                        else
                        {
                            trans.ReferenceNumber = 0;
                        }
                        trans.Money = decimal.Parse(ReadCell(j, 11));
                        trans.Payout = decimal.Parse(ReadCell(j, 13));
                        if(ReadCell(j, 17) != string.Empty)
                        {
                            trans.Date = DateTime.ParseExact(ReadCell(j, 17), "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            trans.Date = DateTime.Now;
                        }
                        
                        newSession.Transactions.Add(trans);
                     
                        j++;
                    }

                    //record this sessions totals
                    var totalMoney = decimal.Parse(ReadCell(j, 11));
                    var totalPayout = decimal.Parse(ReadCell(j, 13));
                    newSession.TotalMoney = totalMoney;
                    newSession.TotalPayout = totalPayout;

                    allSessions.Add(newSession);

                    //move down to start of next session;
                    i = j;
                }

            }

            //loop over spreadhseet again. Look for neq users then add the matching sessions to the user

            fUser = string.Empty;
            fSession = string.Empty;

            for (int i = startRow + 1; i < xlRange.Rows.Count; i++)
            {
                //If two rows in a row have total then a new user is about to begin so record the users totals
                if (ReadCell(i, 5) == "Totals:" && ReadCell(i + 1, 5) == "Totals:" && ReadCell(i + 2, 5) != String.Empty)
                {
                    decimal totalMoney = decimal.Parse(ReadCell(i + 1, 11));
                    decimal totalPayout = decimal.Parse(ReadCell(i + 1, 13));

                    foreach (var u in users)
                    {
                        if (u.CreatedBy == fUser)
                        {
                            u.TotalPayout = totalPayout;
                            u.TotalMoney = totalMoney;
                        }
                            
                    }
                }

                var user = ReadCell(i, 1);
                if (user != fUser && user != String.Empty)
                {
                    
                    fUser = user;

                    users.Add(new CashBankActivityReportRecord() { CreatedBy = user });
                }

               
                var sessionId = ReadCell(i, 4);
                if (sessionId != fSession && sessionId != String.Empty)
                {
                    fSession = sessionId;

                    var sessions = allSessions.Where(s => s.SessionId == sessionId).ToList();

                    foreach(var u in users)
                    {
                        if(u.CreatedBy == fUser)
                        {
                            u.Sessions.AddRange(sessions);
                        }
                    }
                }
            }

            report.Data = users;
            return report;
        }

        public void FindTotal()
        {
            
            int rowEnd = xlRange.Rows.Count - 1;
            
            var value = xlRange.Cells[rowEnd,5].Value2.ToString();
            Console.WriteLine("Got total of " + value);
        }

        public int RowNum(string searchText)
        {
            for(int i = 2; i < xlRange.Rows.Count;i++)
            {
                for(int j = 1; j < xlRange.Columns.Count; j++)
                {
                    if(xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                    {
                        if(xlRange.Cells[i,j].Value2.ToString().ToLower() == searchText.ToLower())
                        {
                            return i;
                        }
                    }
                        
                }
            }

            return -1;
        }



        public void Close()
        {
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            xlWorkbook.Close();
            xlApp.Quit();
        }
    }
}
