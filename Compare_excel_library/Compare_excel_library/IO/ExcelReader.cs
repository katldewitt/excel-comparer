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
        public enum ColKeyOptions
        {
            ROW_NUMBER,
            COL_A_ONLY,
            CONCATENATED_COLS
        }

        public ExcelSheetForComparison ReadExcelSheet(ExcelWorksheet ws, ColKeyOptions colKeyOptions)
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
                    string rowKey = "";
                    int startCol = 0;
                    switch (colKeyOptions)
                    {
                        default:
                        case ColKeyOptions.ROW_NUMBER:
                            rowKey = row.ToString();
                            startCol = 1;
                            break;
                        case ColKeyOptions.COL_A_ONLY:
                            rowKey = ws.Cells[row, 1].Value.ToString();
                            startCol = 2;
                            break;
                        case ColKeyOptions.CONCATENATED_COLS:
                            break;
                    }

                    InDataStruct inData = new InDataStruct() { Key = rowKey, Data = new Dictionary<string, Datum>() };

                    for (int col = startCol; col <= ws.Dimension.Columns; col++)
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

        public Dictionary<string, ExcelSheetForComparison> ReadEntireExcel(string filePath, ColKeyOptions colKeyOptions)
        {

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            Dictionary<string, ExcelSheetForComparison> spreadsheetObject = new Dictionary<string, ExcelSheetForComparison>();

            using (var eppackage = new ExcelPackage(new FileInfo(filePath)))
            {
                foreach (ExcelWorksheet ws in eppackage.Workbook.Worksheets)
                {
                    ExcelSheetForComparison excelSheet = ReadExcelSheet(ws, colKeyOptions);
                    spreadsheetObject.Add(ws.Name, excelSheet);
                }
            }

            return spreadsheetObject;
        }

    }
}
