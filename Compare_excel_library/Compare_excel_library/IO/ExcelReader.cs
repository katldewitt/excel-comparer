using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compare_excel_library.Data_Structures;
using OfficeOpenXml;

namespace Compare_excel_library.IO
{
    public class ExcelReader
    {
        //TODO: Keep col key in excel sheet
        //TODO: add option to use row number as key instead

        public List<InDataStruct> ReadExcelData(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            List<InDataStruct> resultingReadin = new List<InDataStruct>();

            //TODO: Filepath dynamic?
            using (var eppackage = new ExcelPackage(new FileInfo(filePath)))
            {
                //TODO: handle multiple worksheets?
                ExcelWorksheet ws = eppackage.Workbook.Worksheets[0];
                if (ws != null)
                {
                    Dictionary<int, string> colKeyLookup = new Dictionary<int, string>();

                    //Step 1. Get colKeysLookup from row 1
                    //TODO: Is it always on row 1??
                    int row = 1;
                    for (int col = 1; col <= ws.Dimension.Columns; col++)
                    {
                        var colOfInterest = ws.Cells[row, col].Value;
                        if (colOfInterest != null)
                        {
                            colKeyLookup[col] = colOfInterest.ToString();
                        }
                        else
                        {
                            break;
                        }
                    }

                    //Step 2. Iterate through each row to make data starting on Row 2

                    for (row = 2; row <= ws.Dimension.Rows; row++)
                    {
                        //Base case: use row # to make comparisons
                        string rowKey = row.ToString();
                        if (true) //TODO: When do we want to use row # only??
                        {
                            //TODO: is key always just row 1?!
                            rowKey = ws.Cells[row, 1].Value.ToString();
                        }
                        InDataStruct inData = new InDataStruct() { Key = rowKey, Data = new Dictionary<string, Datum>()};


                        for (int col = 2; col <= ws.Dimension.Columns; col++)
                        {
                            var cellOfInterest = ws.Cells[row, col].Value;
                            colKeyLookup.TryGetValue(col, out string? colKey);
                            Datum dm = new Datum(colKey, cellOfInterest);

                            inData.Data.Add(colKey, dm);
                        }

                        resultingReadin.Add(inData);
                    }
                }
            }

            return resultingReadin;

        }
    }
}
