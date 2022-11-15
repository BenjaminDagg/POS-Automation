using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POS_Automation.Model.Reports;
using Excel = Microsoft.Office.Interop.Excel;
using POS_Automation.Model.Reports;
using System.Text.RegularExpressions;
using POS_Automation.Model.Reports.Cashier_Balance_Report;
using POS_Automation.Model.Reports.Drop_By_Date_Report;

//IMPORT THIS DLL AS REFERENCE: C:\Windows\assembly\GAC_MSIL\office\15.0.0.0__71e9bce111e9429c\OFFICE.DLL

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
            xlWorksheet = (Excel.Worksheet)xlWorkbook.Sheets[1];
            xlRange = xlWorksheet.UsedRange;
        }

        public string ReadCell(int row,int col)
        {
            try
            {
                var val = ((Excel.Range)xlRange.Cells[row,col]).Value2.ToString();
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

            //format voucher column in text format
            var r = (Excel.Range)xlWorksheet.Range["H:H"];
            r.NumberFormat = "@";
          
            var title = ReadCell(CashBankActivityReport<CashBankActivityReportRecord>.TitleCell.Row, CashBankActivityReport<CashBankActivityReportRecord>.TitleCell.Col);
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

                        //read voucher number if payout transtype
                        if(ReadCell(j,7) != String.Empty)
                        {
                            var voucher = Decimal.Parse(ReadCell(j, 7), NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint);

                            trans.VoucherNumber = voucher.ToString();
                        }
                        else
                        {
                            trans.VoucherNumber = String.Empty;
                        }

                        //read reference number
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

        public CashierBalanceReport<CashierSessionSummaryRecord> ParseCashierBalanceReport(bool includeVouchers)
        {
            var report = new CashierBalanceReport<CashierSessionSummaryRecord>();
            var records = new List<CashierSessionSummaryRecord>();
            var vouchers = new List<VoucherDetailRecord>();
            var drops = new List<CashRemovedRecord>();
            report.Data = records;
            report.UnpaidVouchers = vouchers;
            string title = string.Empty;
            DateTime runTime;
            string period = string.Empty;
            int cashierSessionSummaryStartRow = RowNum("Cashier Session Summary");
            int cashierSummaryTotalsRow = RowNum("Period Totals:");
            int voucherDetailsStartRow = -1;
            int DropRecordsStartingRow = RowNum("Dispenser Number");
            int titleColNum = ColNum("Cashier Balance Report");
            int runTimeCol = ColNum("Run Date/Time");
            int periodColNum = ColNum("Reporting Period");

            

            //get title
            title = ReadCell(3,titleColNum);
            report.Title = title;

            //get period
            period = ReadCell(6,periodColNum);
            period = Regex.Replace(period, @"\t|\n|\r", "");
            report.ReportPeriod = period;

            //get run time
            string runTimeString = ReadCell(2, runTimeCol);
            runTimeString = Regex.Replace(runTimeString, @"\t|\n|\r", "");
            runTimeString = runTimeString.Replace("Run Date/Time", "");
            runTime = DateTime.Parse(runTimeString);
            report.RunDate = runTime;

            //if cant find cashier summary then report is empty. Just return report with title, period, and empty values for data
            if (cashierSummaryTotalsRow == -1 && DropRecordsStartingRow == -1)
            {
                return report;
            }


            //loop over cashier summary and get data
            if (cashierSessionSummaryStartRow != -1)
            {
                
                int sessionIdCol = ColNum("Session ID");
                int startBalanceCol = ColNum("Starting Balance");
                int payoutCol = ColNum("Total Payout");
                int addedCol = ColNum("Total Amount Added");
                int removedCol = ColNum("Total Amount Removed");
                int endBalanceCol = ColNum("Ending Balance");
                int varianceCol = ColNum("Period Variance");

                for (int i = cashierSessionSummaryStartRow + 2; i < xlRange.Rows.Count && ReadCell(i, 1) != "Period Totals:" && cashierSessionSummaryStartRow != -1; i++)
                {

                    var session = new CashierSessionSummaryRecord();

                    //parse session id
                    string sessionIdText = ReadCell(i, sessionIdCol);
                    int firstNewLine = sessionIdText.IndexOf('\n');
                    string sessionId = sessionIdText.Substring(0, firstNewLine - 1);
                    session.SessionId = sessionId;

                    //parse start date
                    int startDateStartIndex = sessionIdText.IndexOf(':');
                    int startDateEndIndex = sessionIdText.IndexOf("End Date:");
                    int startDateLength = startDateEndIndex - startDateStartIndex - 1;
                    string startDate = sessionIdText.Substring(startDateStartIndex + 1, startDateLength).Replace("\n", "");
                    startDate = Regex.Replace(startDate, @"\t|\n|\r", "").Trim();
                    try
                    {
                        session.StartDate = DateTime.ParseExact(startDate, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                    }
                    catch (Exception ex)
                    {
                        session.StartDate = default(DateTime);
                    }

                    //parse end date
                    int endDateStartIndex = sessionIdText.LastIndexOf("e:");
                    int endDateEndIndex = sessionIdText.Length - 1;
                    int endDateLength = endDateEndIndex - endDateStartIndex - 1;
                    string endDate = sessionIdText.Substring(endDateStartIndex + 2, endDateLength).Trim();
                    if (endDate == "Session Not Ended")
                    {
                        session.EndDate = default(DateTime);
                    }
                    else
                    {
                        session.EndDate = DateTime.ParseExact(endDate, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                    }

                    string startBalance = ReadCell(i, startBalanceCol);
                    string totalPayouit = ReadCell(i, payoutCol);
                    string totalAdded = ReadCell(i, addedCol);
                    string totalRemoved = ReadCell(i, removedCol);
                    string endBalance = ReadCell(i, endBalanceCol);
                    string periodVariance = ReadCell(i, varianceCol);

                    session.SessionId = sessionId;
                    session.StartBalance = decimal.Parse(startBalance);
                    session.TotalPayoutAmount = decimal.Parse(totalPayouit);
                    session.TotalAmountAdded = decimal.Parse(totalAdded);
                    session.TotalAmountRemoved = decimal.Parse(totalRemoved);
                    session.EndBalance = decimal.Parse(endBalance);

                    report.Data.Add(session);
                }

                //get totals for session summary
                report.TotalStartingBalance = decimal.Parse(ReadCell(cashierSummaryTotalsRow, startBalanceCol));
                report.TotalPayoutAmount = decimal.Parse(ReadCell(cashierSummaryTotalsRow, payoutCol));
                report.TotalAmountAdded = decimal.Parse(ReadCell(cashierSummaryTotalsRow, addedCol));
                report.TotalAmountRemoved = decimal.Parse(ReadCell(cashierSummaryTotalsRow, removedCol));
                report.TotalEndBalance = decimal.Parse(ReadCell(cashierSummaryTotalsRow, endBalanceCol));
            }

            //Read drop data if exists
            if(DropRecordsStartingRow != -1)
            {
                int machNoCol = ColNum("Dispenser Number");
                int descriptionCol = ColNum("Dispenser Description");
                int cashRemovedCol = ColNum("Cash Removed",DropRecordsStartingRow);
                int timestampCol = ColNum("Date Time");

                int dropRowCount = DropRecordsStartingRow + 1;
                for(int i = DropRecordsStartingRow + 1; i < xlRange.Rows.Count && ReadCell(i,1) != "Period Total:"; i++)
                {
                    string machNo = ReadCell(i,machNoCol);
                    string description = ReadCell(i,descriptionCol);
                    string removed = ReadCell(i,cashRemovedCol);
                    string timeStamp = ReadCell(i,timestampCol);

                    var newDrop = new CashRemovedRecord();
                    newDrop.MachineNumber = machNo;
                    newDrop.MachineDescription = description;
                    newDrop.CashRemoved = decimal.Parse(removed, NumberStyles.Currency);
                    newDrop.TImestamp = DateTime.ParseExact(timeStamp,"MM/dd/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);

                    drops.Add(newDrop);

                    dropRowCount++;
                }

                //get drop totals
                string totalCashRemoved = ReadCell(dropRowCount, cashRemovedCol);
                report.TotalDropAmount = decimal.Parse(totalCashRemoved, NumberStyles.Currency);

                report.CashDrops = drops;
            }

            
            //switch sheets
            xlWorksheet = (Excel.Worksheet)xlWorkbook.Sheets[2];
            xlWorksheet.Select();
            xlRange = xlWorksheet.UsedRange;

            //loop over unpaid vouchers table
            voucherDetailsStartRow = RowNum("Unpaid Vouchers Detail") + 2;
            int rowCount = voucherDetailsStartRow;
            for (int i = voucherDetailsStartRow; i < xlRange.Rows.Count && ReadCell(i, 1) != "Total" && includeVouchers; i++)
            {
                var voucher = new VoucherDetailRecord();

                string barcode = ReadCell(i, 1);
                string amount = ReadCell(i, 2);
                string date = ReadCell(i, 4);

                voucher.VoucherNumber = barcode;
                voucher.Amount = decimal.Parse(amount);
                voucher.CreatedDate = DateTime.ParseExact(date, "M/d/yyyy", CultureInfo.InvariantCulture);
                report.UnpaidVouchers.Add(voucher);

                rowCount++;
            }

            //Get unpaid vouchers totals
            report.TotalUnpaidVoucherAmount = includeVouchers ? decimal.Parse(((Excel.Range)xlWorksheet.Cells[rowCount, 3]).Value2.ToString()) : 0;
            
            return report;
        }


        

        private CashierBalanceReport<CashierSessionSummaryRecord> ParseCashierBalanceReportWithDropData(bool includeVouchers)
        {
            var report = new CashierBalanceReport<CashierSessionSummaryRecord>();
            var records = new List<CashierSessionSummaryRecord>();
            var vouchers = new List<VoucherDetailRecord>();
            report.Data = records;
            report.UnpaidVouchers = vouchers;
            string title = string.Empty;
            DateTime runTime;
            string period = string.Empty;
            int cashierSessionSummaryStartRow = RowNum("Cashier Session Summary") + 2;
            int cashierSummaryTotalsRow = RowNum("Period Totals:");
            int voucherDetailsStartRow = -1;

            //if cant find cashier summary then report is empty. Just return report with title, period, and empty values for data
            if (cashierSummaryTotalsRow == -1)
            {
                report.Title = ((Excel.Range)xlWorksheet.Cells[3, 5]).Value.ToString();

                period = ((Excel.Range)xlWorksheet.Cells[6, 4]).Value.ToString();
                period = Regex.Replace(period, @"\t|\n|\r", "");
                report.ReportPeriod = period;

                string runTimeS = ((Excel.Range)xlWorksheet.Cells[2, 10]).Value.ToString();
                runTimeS = Regex.Replace(runTimeS, @"\t|\n|\r", "");
                runTimeS = runTimeS.Replace("Run Date/Time", "");
                runTime = DateTime.Parse(runTimeS);
                report.RunDate = runTime;

                return report;
            }

            //get title
            Excel.Range titleRange = xlWorksheet.Range["A3", "L3"];
            foreach (Microsoft.Office.Interop.Excel.Range cell in titleRange.Cells)
            {
                if (!string.IsNullOrEmpty(cell.Text.ToString()))
                {
                    title += cell.Value2.ToString();
                }
            }
            report.Title = title;

            //get period
            Excel.Range periodRange = xlWorksheet.Range["A6", "O6"];
            foreach (Microsoft.Office.Interop.Excel.Range cell in periodRange.Cells)
            {
                if (!string.IsNullOrEmpty(cell.Text.ToString()))
                {
                    period += cell.Value2.ToString();
                }
            }
 
            period = Regex.Replace(period, @"\t|\n|\r", "");
            report.ReportPeriod = period;

            //get run time
            Excel.Range runTimeRange = xlWorksheet.Range["M2", "R2"];
            var runTimeString = string.Empty;
            foreach (Microsoft.Office.Interop.Excel.Range cell in runTimeRange.Cells)
            {
                if (!string.IsNullOrEmpty(cell.Text.ToString()))
                {
                    runTimeString += cell.Value2.ToString();
                }
            }
            runTimeString = Regex.Replace(runTimeString, @"\t|\n|\r", "");
            
            runTimeString = runTimeString.Replace("Run Date/Time", "");
            runTime = DateTime.Parse(runTimeString);
            report.RunDate = runTime;

            
            //loop over cashier summary and get data
            for (int i = cashierSessionSummaryStartRow; i < xlRange.Rows.Count && ReadCell(i, 1) != "Period Totals:"; i++)
            {

                var session = new CashierSessionSummaryRecord();

                //parse session id
                string sessionIdText = ReadCell(i, 1);
                int firstNewLine = sessionIdText.IndexOf('\n');
                string sessionId = sessionIdText.Substring(0, firstNewLine - 1);
                session.SessionId = sessionId;

                //parse start date
                int startDateStartIndex = sessionIdText.IndexOf(':');
                int startDateEndIndex = sessionIdText.IndexOf("End Date:");
                int startDateLength = startDateEndIndex - startDateStartIndex - 1;
                string startDate = sessionIdText.Substring(startDateStartIndex + 1, startDateLength).Replace("\n", "");
                startDate = Regex.Replace(startDate, @"\t|\n|\r", "").Trim();
                try
                {
                    session.StartDate = DateTime.ParseExact(startDate, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    session.StartDate = default(DateTime);
                }

                //parse end date
                int endDateStartIndex = sessionIdText.LastIndexOf("e:");
                int endDateEndIndex = sessionIdText.Length - 1;
                int endDateLength = endDateEndIndex - endDateStartIndex - 1;
                string endDate = sessionIdText.Substring(endDateStartIndex + 2, endDateLength).Trim();
                if (endDate == "Session Not Ended")
                {
                    session.EndDate = default(DateTime);
                }
                else
                {
                    session.EndDate = DateTime.ParseExact(endDate, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                }

                string startBalance = ReadCell(i, 4);
                string totalPayouit = ReadCell(i, 6);
                string totalAdded = ReadCell(i, 8);
                string totalRemoved = ReadCell(i, 9);
                string endBalance = ReadCell(i, 12);
                string periodVariance = ReadCell(i, 15);

                session.SessionId = sessionId;
                session.StartBalance = decimal.Parse(startBalance);
                session.TotalPayoutAmount = decimal.Parse(totalPayouit);
                session.TotalAmountAdded = decimal.Parse(totalAdded);
                session.TotalAmountRemoved = decimal.Parse(totalRemoved);
                session.EndBalance = decimal.Parse(endBalance);

                report.Data.Add(session);
            }

            //get totals for session summary
            report.TotalStartingBalance = decimal.Parse(((Excel.Range)xlWorksheet.Cells[cashierSummaryTotalsRow, 5]).Value2.ToString());
            report.TotalPayoutAmount = decimal.Parse(((Excel.Range)xlWorksheet.Cells[cashierSummaryTotalsRow, 7]).Value2.ToString());
            report.TotalAmountAdded = decimal.Parse(((Excel.Range)xlWorksheet.Cells[cashierSummaryTotalsRow, 9]).Value2.ToString());
            report.TotalAmountRemoved = decimal.Parse(((Excel.Range)xlWorksheet.Cells[cashierSummaryTotalsRow, 10]).Value2.ToString());
            report.TotalEndBalance = decimal.Parse(((Excel.Range)xlWorksheet.Cells[cashierSummaryTotalsRow, 13]).Value2.ToString());

            //switch sheets
            xlWorksheet = (Excel.Worksheet)xlWorkbook.Sheets[2];
            xlWorksheet.Select();
            xlRange = xlWorksheet.UsedRange;

            //loop over unpaid vouchers table
            voucherDetailsStartRow = RowNum("Unpaid Vouchers Detail") + 2;
            int rowCount = voucherDetailsStartRow;
            for (int i = voucherDetailsStartRow; i < xlRange.Rows.Count && ReadCell(i, 1) != "Total" && includeVouchers; i++)
            {
                var voucher = new VoucherDetailRecord();

                string barcode = ReadCell(i, 1);
                string amount = ReadCell(i, 2);
                string date = ReadCell(i, 4);

                voucher.VoucherNumber = barcode;
                voucher.Amount = decimal.Parse(amount);
                voucher.CreatedDate = DateTime.ParseExact(date, "M/d/yyyy", CultureInfo.InvariantCulture);
                report.UnpaidVouchers.Add(voucher);

                rowCount++;
            }

            //Get unpaid vouchers totals
            report.TotalUnpaidVoucherAmount = includeVouchers ? decimal.Parse(((Excel.Range)xlWorksheet.Cells[rowCount, 3]).Value2.ToString()) : 0;
            
            return report;
        }


        public DropByDateReport<CashDropRecord> ParseDropByDateReport()
        {
            var report = new DropByDateReport<CashDropRecord>();
            var records = new List<CashDropRecord>();
            report.Data = records;

            string title = string.Empty;
            DateTime runTime;
            string period = string.Empty;
            int dataRowNumStart = RowNum("Terminal");

            //report is empty
            if(dataRowNumStart == -1)
            {
                //get title
                title = ((Excel.Range)xlWorksheet.Cells[6, 7]).Value.ToString();
                report.Title = title;

                //get period
                period = ((Excel.Range)xlWorksheet.Cells[8, 5]).Value.ToString();
                period = Regex.Replace(period, @"\t|\n|\r", "");
                report.ReportPeriod = period;

                //get run time
                string runTimeS = ((Excel.Range)xlWorksheet.Cells[5, 10]).Value.ToString();
                runTimeS = Regex.Replace(runTimeS, @"\t|\n|\r", "");
                runTimeS = runTimeS.Replace("Run Date/Time", "");
                runTime = DateTime.Parse(runTimeS);
                report.RunDate = runTime;

                //get location
                string locationS = ((Excel.Range)xlWorksheet.Cells[2, 2]).Value.ToString();
                report.Location = locationS;

                return report;
            }

            //get title
            title = ((Excel.Range)xlWorksheet.Cells[6,10]).Value.ToString();
            report.Title = title;

            //get period
            period = ((Excel.Range)xlWorksheet.Cells[8, 5]).Value.ToString();
            period = Regex.Replace(period, @"\t|\n|\r", "");
            report.ReportPeriod = period;

            //get run time
            string runTimeString = ((Excel.Range)xlWorksheet.Cells[5, 21]).Value.ToString();
            runTimeString = Regex.Replace(runTimeString, @"\t|\n|\r", "");
            runTimeString = runTimeString.Replace("Run Date/Time", "");
            runTime = DateTime.Parse(runTimeString);
            report.RunDate = runTime;

            //get location
            string locationString = ((Excel.Range)xlWorksheet.Cells[2, 2]).Value.ToString();
            report.Location = locationString;

           
            //loop through table and get drop data
            int rowCount = dataRowNumStart + 1;
            for(int i = dataRowNumStart + 1; i < xlRange.Rows.Count && ReadCell(i,3).Contains("Totals") == false; i++)
            {
                var drop = new CashDropRecord();

                string terminalNum = ReadCell(i,3).Replace(",","");
                string billCount1 = ReadCell(i, 5).Replace(",", "");
                string billCount5 = ReadCell(i,6).Replace(",", "");
                string billCount10 = ReadCell(i, 8).Replace(",", "");
                string billCount20 = ReadCell(i, 10).Replace(",", "");
                string billCount50 = ReadCell(i,11).Replace(",", "");
                string billCount100 = ReadCell(i, 13).Replace(",", "");
                string ticketAmountTotal = ReadCell(i, 14).Replace(",", "");
                string totalTickets = ReadCell(i, 15).Replace(",", "");
                string totalBills = ReadCell(i, 16).Replace(",", "");
                string totalDropAmount = ReadCell(i, 18).Replace(",", "");
                string dropDate = ReadCell(i, 19).Replace(",", "");
                string account = ReadCell(i, 22).Replace(",", "");

                drop.TerminalId = terminalNum;
                drop.Amount1Dollar = int.Parse(billCount1);
                drop.Amount5Dollar = int.Parse(billCount5);
                drop.Amount10Dollar = int.Parse(billCount10);
                drop.Amount20Dollar = int.Parse(billCount20);
                drop.Amount50Dollar = int.Parse(billCount50);
                drop.Amount100Dollar = int.Parse(billCount100);
                drop.TotalTicketAmount = decimal.Parse(ticketAmountTotal,NumberStyles.Currency);
                drop.TotalTickets = int.Parse(totalTickets);
                drop.TotalBills = int.Parse(totalBills);
                drop.TotalDropAmount = decimal.Parse(totalDropAmount, NumberStyles.Currency);
                drop.DropTime = DateTime.ParseExact(dropDate, "MM/dd/yyyy hh:mm:ss", CultureInfo.InvariantCulture);
                drop.Account = account;

                records.Add(drop);
                rowCount++;
            }

            //parse totals
            report.Total1Dollar = int.Parse(ReadCell(rowCount, 5).Replace(",", ""));
            report.Total5Dollar = int.Parse(ReadCell(rowCount, 6).Replace(",", ""));
            report.Total10Dollar = int.Parse(ReadCell(rowCount, 8).Replace(",", ""));
            report.Total20Dollar = int.Parse(ReadCell(rowCount, 10).Replace(",", ""));
            report.Total50Dollar = int.Parse(ReadCell(rowCount, 11).Replace(",", ""));
            report.Total100Dollar = int.Parse(ReadCell(rowCount, 13).Replace(",", ""));
            report.TotalTicketAmount = decimal.Parse(ReadCell(rowCount, 14).Replace(",", ""));
            report.TotalTicketCount = int.Parse(ReadCell(rowCount, 15).Replace(",", ""));
            report.TotalBills = int.Parse(ReadCell(rowCount, 16).Replace(",", ""));
            report.TotalDropAmount = decimal.Parse(ReadCell(rowCount, 18).Replace(",", ""));

            report.Data = records;

            return report;
        }


        public void FindTotal()
        {
            
            int rowEnd = xlRange.Rows.Count - 1;
            
            var value = ((Excel.Range)xlRange.Cells[rowEnd,5]).Value2.ToString();
            Console.WriteLine("Got total of " + value);
        }

        public int RowNum(string searchText)
        {
            //was 2
            for(int i = 2; i < xlRange.Rows.Count;i++)
            {
                for(int j = 1; j < xlRange.Columns.Count; j++)
                {
                    if(xlRange.Cells[i, j] != null && ((Excel.Range)xlRange.Cells[i, j]).Value2 != null)
                    {
                        if(((Excel.Range)xlRange.Cells[i,j]).Value2.ToString().ToLower() == searchText.ToLower())
                        {
                            return i;
                        }
                    }
                        
                }
            }

            return -1;
        }

        public int ColNum(string searchText)
        {
            for (int i = 1; i < xlRange.Rows.Count; i++)
            {
                for (int j = 1; j < xlRange.Columns.Count; j++)
                {
                    if (xlRange.Cells[i, j] != null && ((Excel.Range)xlRange.Cells[i, j]).Value2 != null)
                    {
                        if (((Excel.Range)xlRange.Cells[i, j]).Value2.ToString().ToLower().Contains(searchText.ToLower()))
                        {
                            return j;
                        }
                    }

                }
            }

            return -1;
        }

        public int ColNum(string searchText, int startRow)
        {
            for (int i = startRow; i < xlRange.Rows.Count; i++)
            {
                for (int j = 1; j < xlRange.Columns.Count; j++)
                {
                    if (xlRange.Cells[i, j] != null && ((Excel.Range)xlRange.Cells[i, j]).Value2 != null)
                    {
                        if (((Excel.Range)xlRange.Cells[i, j]).Value2.ToString().ToLower().Contains(searchText.ToLower()))
                        {
                            return j;
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
