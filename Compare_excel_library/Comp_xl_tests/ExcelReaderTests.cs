using Compare_excel_library.Data_Structures;
using Compare_excel_library.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Comp_xl_tests
{
    [TestClass]
    public class ExcelReaderTests
    {
        #region Test Datum
        static string testKey = "testKey";
        static readonly Datum numericInt = new Datum(testKey + "_int", 123);
        static readonly Datum numericDouble = new Datum(testKey + "_dbl", 1.23);
        static readonly Datum datumBool = new Datum(testKey + "_bool", true);
        static readonly Datum datumString = new Datum(testKey + "_string", "stringy");
        static readonly Datum datumStringChanged = new Datum(testKey + "_string", "strongy");
        //TODO: Date writing into exel is not as expected.
        //static readonly Datum datumDateTimeToday = new Datum(testKey + "_dt", DateTime.Today);
        //static readonly Datum datumDateTimeTomorrow = new Datum(testKey + "_dt", DateTime.Today.AddDays(1));

        //File set ups
        static string filepath = @"C:\Projects\excel-comparer\Compare_excel_library\assets\test";
        static string sheetname = "Sheet1";
        #endregion

        private string GetFileNameOrigOrComp(bool comparisonSheet)
        {
            return comparisonSheet ? "comparison.xlsx" : "original.xlsx";
        }

        private void GenerateTestExcel(bool comparisonSheet)
        {
            Directory.CreateDirectory(filepath);
            string fileName = GetFileNameOrigOrComp(comparisonSheet);

            List<Datum> dataForTesting = new List<Datum>()
            {
                numericInt,
                numericDouble,
                datumBool
            };
            if (comparisonSheet)
            {
                dataForTesting.Add(datumString);
                //dataForTesting.Add(datumDateTimeToday);
            }
            else
            {
                dataForTesting.Add(datumStringChanged);
                //dataForTesting.Add(datumDateTimeTomorrow);
            }
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var eppackage = new ExcelPackage())
            {
                ExcelWorksheet ws = eppackage.Workbook.Worksheets.Add(sheetname);
                int r = 1;
                int c = 1;
                ws.Cells[r, c].Value = "Key";
                ws.Cells[r, c += 1].Value = "Test_data";
                foreach (Datum item in dataForTesting)
                {
                    r++;
                    c = 1;
                    ws.Cells[r, c].Value = item.ColKey;
                    ws.Cells[r, c += 1].Value = item.Value;
                }

                eppackage.SaveAs(Path.Combine(filepath, fileName));
            }
        }

        [TestMethod]
        public void VerifyReadin_ColA()
        {
            GenerateTestExcel(true);
            GenerateTestExcel(false);

            ExcelReader er = new ExcelReader();
            Dictionary<string, ExcelSheetForComparison> orig = er.ReadEntireExcel(Path.Combine(filepath, GetFileNameOrigOrComp(false)),
                ExcelReader.ColKeyOptions.COL_A_ONLY,
                null);
            Assert.AreEqual(1, orig.Count, "There should only be 1 sheet in datastruct.");

            //Check ExcelSheetForComparison struct
            ExcelSheetForComparison sheetToCheck = orig[sheetname];
            Assert.AreEqual(4, sheetToCheck.RowsOfData.Count, "There should be 4 rows of data in datastruct.");
            Assert.AreEqual(2, sheetToCheck.ColKeyLookup.Count, "There should be 2 cols of data in datastruct.");

            //Check the expected values of the keys
            List<string> colAOnlyKeys = new List<string>()
            {
                numericInt.ColKey,
                numericDouble.ColKey,
                datumBool.ColKey,
                datumString.ColKey,
               // datumDateTimeToday.ColKey
            };
            Assert.IsTrue(Enumerable.SequenceEqual(colAOnlyKeys,
                                            sheetToCheck.RowsOfData.Select(x => x.Key).ToList()),
                          "The keys of the dataset should be the values in ColumnA.");


        }
        [TestMethod]
        public void VerifyReadin_Concatenated()
        {
            GenerateTestExcel(true);
            GenerateTestExcel(false);


            ExcelReader er = new ExcelReader();
            Dictionary<string, ExcelSheetForComparison> orig = er.ReadEntireExcel(Path.Combine(filepath, GetFileNameOrigOrComp(false)),
                ExcelReader.ColKeyOptions.CONCATENATED_COLS,
                new List<int>() { 1, 2 });
            Assert.AreEqual(1, orig.Count, "There should only be 1 sheet in datastruct.");

            //Check ExcelSheetForComparison struct
            ExcelSheetForComparison sheetToCheck = orig[sheetname];
            Assert.AreEqual(4, sheetToCheck.RowsOfData.Count, "There should be 4 rows of data in datastruct.");
            Assert.AreEqual(2, sheetToCheck.ColKeyLookup.Count, "There should be 2 cols of data in datastruct.");

            //Check the expected values of the keys
            List<string> concatenatedColKeys = new List<string>()
            {
                "-" + numericInt.ColKey + "-" +  numericInt.Value.ToString(),
                "-" + numericDouble.ColKey  + "-" + numericDouble.Value.ToString(),
                "-" + datumBool.ColKey  + "-" + datumBool.Value.ToString(),
               "-" +  datumStringChanged.ColKey  + "-" + datumStringChanged.Value.ToString(),
               // "-" + datumDateTimeTomorrow.ColKey  + "-" + ((DateTime)datumDateTimeTomorrow.Value).Subtract(new DateTime(1900, 1, 1)).TotalDays.ToString()
            };
            Assert.IsTrue(Enumerable.SequenceEqual(concatenatedColKeys,
                                            sheetToCheck.RowsOfData.Select(x => x.Key).ToList()),
                          "The keys of the dataset should be the values in ColumnA combined with ColumnB.");
        }
        [TestMethod]
        public void VerifyReadin_RowNumber()
        {
            GenerateTestExcel(true);
            GenerateTestExcel(false);

            ExcelReader er = new ExcelReader();
            Dictionary<string, ExcelSheetForComparison> orig = er.ReadEntireExcel(Path.Combine(filepath, GetFileNameOrigOrComp(false)),
                ExcelReader.ColKeyOptions.ROW_NUMBER,
                null);
            Assert.AreEqual(1, orig.Count, "There should only be 1 sheet in datastruct.");

            //Check ExcelSheetForComparison struct
            ExcelSheetForComparison sheetToCheck = orig[sheetname];
            Assert.AreEqual(4, sheetToCheck.RowsOfData.Count, "There should be 4 rows of data in datastruct.");
            Assert.AreEqual(2, sheetToCheck.ColKeyLookup.Count, "There should be 2 cols of data in datastruct.");

            //Check the expected values of the keys
            //Important: EPPlus is 1 indexed and our header is row 1, so we start at row 2
            List<string> rowNumberKeys = Enumerable.Range(2, 4).Select(x => x.ToString()).ToList();
            Assert.IsTrue(Enumerable.SequenceEqual(rowNumberKeys,
                                            sheetToCheck.RowsOfData.Select(x => x.Key).ToList()),
                          "The keys of the dataset should be strings 2 - 5");
        }

    }
}