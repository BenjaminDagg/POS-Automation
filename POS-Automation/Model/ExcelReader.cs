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

        public List<DailyCashierActivityReportRecord> ParseCashierActivityReport()
        {
            var records = new List<DailyCashierActivityReportRecord>();

            string prevSession = string.Empty;
            string prevUser = string.Empty;
            int startRow = RowNum("Created By");
        
            for(int i = startRow + 1; i < xlRange.Rows.Count - 2; i++)
            {

                DailyCashierActivityReportRecord record = new DailyCashierActivityReportRecord();

                if (ReadCell(i, 1) != String.Empty)
                {
                    prevUser = ReadCell(i, 1);
                }
                record.CreatedBy = prevUser;

                if (ReadCell(i,2) != String.Empty)
                {
                    prevSession = ReadCell(i,2);
                }
                record.SessionId = prevSession;
                record.Station = ReadCell(i, 3);
                record.VoucherNumber = ReadCell(i, 4);
                record.PayoutAmount = decimal.Parse(ReadCell(i, 5));
                record.ReceiptNumber = int.Parse(ReadCell(i, 6));
                string dateString = ReadCell(i, 7);
                record.Date = DateTime.ParseExact(dateString,"M/d/yyyy h:mm:ss tt",CultureInfo.InvariantCulture);

                records.Add(record);
            }

            return records;
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
