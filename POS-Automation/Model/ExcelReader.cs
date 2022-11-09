using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POS_Automation.Model.Reports;
using Excel = Microsoft.Office.Interop.Excel;

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

            string prevSession = string.Empty;
            string prevUser = string.Empty;
            int startRow = RowNum("Created By");

            var title = ReadCell(Report<DailyCashierActivityReportRecord>.TitleCell.Row, Report<DailyCashierActivityReportRecord>.TitleCell.Col);
            report.Title = title;

            var date = ReadCell(Report<DailyCashierActivityReportRecord>.RuntimeCell.Row, Report<DailyCashierActivityReportRecord>.RuntimeCell.Col);
            date = date.Replace("\n", "");
            date = date.Replace("Run Date/Time", "");
            report.RunDate = DateTime.Parse(date);

            var period = ReadCell(Report<DailyCashierActivityReportRecord>.PeriodCell.Row, Report<DailyCashierActivityReportRecord>.PeriodCell.Col);
            report.ReportPeriod = period;

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

        public DailyCashierActivityReport<DailyCashierActivityReportRecord> ParseCashBankActivityReport()
        {
            var report = new DailyCashierActivityReport<DailyCashierActivityReportRecord>();
            var records = new List<DailyCashierActivityReportRecord>();

            string prevSession = string.Empty;
            string prevUser = string.Empty;
            int startRow = RowNum("Created By");



            var period = ReadCell(Report<DailyCashierActivityReportRecord>.PeriodCell.Row, Report<DailyCashierActivityReportRecord>.PeriodCell.Col);
            report.ReportPeriod = period;

            string fUser = string.Empty;
            string fSession = string.Empty;

            var allSessions = new List<CashBankActivitySession>();
            var users = new List<CashBankActivityReportUser>();

            for (int i = startRow + 1; i < xlRange.Rows.Count - 1; i++)
            {

                var s = ReadCell(i,4);
                Console.WriteLine(s);
                if(s != fSession && s != String.Empty)
                {
                    //Console.WriteLine("Starting new session: " + s);
                    fSession = s;
                    var newSession = new CashBankActivitySession();
                    newSession.SessionId = s;

                    int j = i;

                    while(ReadCell(j,5) != "Totals:")
                    {
                        var trans = new CashBankActivityTransaction();
                        trans.SessionId = fSession;
                        trans.TransType = ReadCell(j, 9);
                        //Console.WriteLine("Transaction: " + trans.TransType);

                        newSession.Transactions.Add(trans);
                     
                        j++;
                    }

                    var totalMoney = decimal.Parse(ReadCell(j, 11));
                    var totalPayout = decimal.Parse(ReadCell(j, 13));
                    //Console.WriteLine("Session totals: " + totalMoney + ": " + totalPayout);

                    allSessions.Add(newSession);

                    i = j;
                }

            }

            /*
            fUser = string.Empty;
            fSession = string.Empty;

            for (int i = startRow + 1; i < xlRange.Rows.Count; i++)
            {

                var user = ReadCell(i, 1);
                Console.WriteLine(user);
                if(user != fUser && user != String.Empty)
                {
                    Console.WriteLine("Starting new user");
                    fUser = user;

                    var userCB = new CashBankActivityReportUser();
                    userCB.CreatedBy = user;

                    int j = i;

                    while(ReadCell(j, 1) == String.Empty || ReadCell(j,1) == fUser)
                    {
                        var sessionId = ReadCell(j, 4);
                        if(sessionId != fSession && sessionId != String.Empty)
                        {
                            Console.WriteLine("Adding new session");
                            fSession = sessionId;

                            var  sessions = allSessions.Where(s => s.SessionId == sessionId).ToList();
                            userCB.Sessions.AddRange(sessions);
                        }

                        j++;
                    }

                    

                    users.Add(userCB);
                }
                else
                {
                    continue;
                }

            }
            */

            fUser = string.Empty;
            fSession = string.Empty;

            for (int i = startRow + 1; i < xlRange.Rows.Count; i++)
            {

                var user = ReadCell(i, 1);
                if (user != fUser && user != String.Empty)
                {
                    fUser = user;

                    users.Add(new CashBankActivityReportUser() { CreatedBy = user });
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

            Console.WriteLine("Users: " + users.Count);
            Console.WriteLine("user 1 1st session = " + users[0].Sessions[0].SessionId);
            Console.WriteLine("user 2 1st session = " + users[1].Sessions[0].SessionId);

            report.Data = records;
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
