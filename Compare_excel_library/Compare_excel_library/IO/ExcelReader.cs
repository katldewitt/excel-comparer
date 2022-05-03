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
        public List<InDataStruct> ReadExcelData(string filePath)
        {
            return null;
        }
        //TODO: add option to use row number as key instead

        public ExcelSheetForComparison ReadExcelSheet(ExcelWorksheet ws)
        {
            ExcelSheetForComparison finalSheetDataStruct = new ExcelSheetForComparison();
            List< InDataStruct> resultingReadin = new List< InDataStruct>();

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
                        continue;
                    }
                }
                finalSheetDataStruct.ColKeyLookup = colKeyLookup; 

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
                    InDataStruct inData = new InDataStruct() { Key = rowKey, Data = new Dictionary<string, Datum>() };


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
            finalSheetDataStruct.RowsOfData = resultingReadin;
            return finalSheetDataStruct;

        }

        public Dictionary<string, ExcelSheetForComparison> ReadEntireExcel(string filePath)
        {

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            Dictionary<string, ExcelSheetForComparison> spreadsheetObject = new Dictionary<string, ExcelSheetForComparison>();

            using (var eppackage = new ExcelPackage(new FileInfo(filePath)))
            {
                foreach (ExcelWorksheet ws in eppackage.Workbook.Worksheets)
                {
                    ExcelSheetForComparison excelSheet = ReadExcelSheet(ws);
                    spreadsheetObject.Add(ws.Name, excelSheet);
                }
            }

            return spreadsheetObject;
        }

    }
}
