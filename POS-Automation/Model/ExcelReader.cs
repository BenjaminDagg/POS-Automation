using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public void Open(string filepath)
        {
            filename = filepath;

            xlWorkbook = xlApp.Workbooks.Open(filepath);
            xlWorksheet = xlWorkbook.Sheets[1];
            xlRange = xlWorksheet.UsedRange;
        }

        public void Read()
        {
            string currentSession = string.Empty;

            for(int i = 6; i < xlRange.Rows.Count; i++)
            {
                if (i == 6) continue;

                for(int j = 2; j < xlRange.Columns.Count; j++)
                {
                    if (j == 2)
                        Console.Write("\r\n");

                    if (xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null && i > 6)
                        Console.Write(xlRange.Cells[i, j].Value2.ToString() + "\t");
                }
            }
        }

        public void FindTotal()
        {
            
            int rowEnd = xlRange.Rows.Count - 1;
            
            var value = xlRange.Cells[rowEnd,5].Value2.ToString();
            Console.WriteLine("Got total of " + value);
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
