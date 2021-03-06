using Compare_excel_library.Compare_Methods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using OfficeOpenXml;
using Compare_excel_library.Data_Structures;

namespace Compare_excel_library.IO
{
    public class ExcelWriter
    {
        private readonly ConductComparisons _cd;
        private bool _prioritizeSource;

        public ExcelWriter(ConductComparisons cd)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            this._cd = cd;
        }

        public void WriteReport(string filePath, bool prioritizeSource)
        {
            this._prioritizeSource = prioritizeSource;

            //TODO: how to let users decide which comparison to print

            using (var eppackage = new ExcelPackage())
            {
                WriteInBothSheet(eppackage);
                WriteInSourceOnly(eppackage);
                WriteOnlyInComp(eppackage);
                WriteAllSheet(eppackage);
                WriteInSourceSheet(eppackage);
                WriteInComparisonSheet(eppackage);
                eppackage.SaveAs(new FileInfo(filePath));
            }
        }

        #region Sheet Creation and Helper Methods

        private void WriteInBothSheet(ExcelPackage eppackage)
        {
            WriteComparsions(eppackage, "In Both", _cd.InBoth());
        }

        private void WriteInSourceOnly(ExcelPackage eppackage)
        {
            WriteComparsions(eppackage, "Only in source", _cd.InOrigNotComp());
        }

        private void WriteOnlyInComp(ExcelPackage eppackage)
        {
            WriteComparsions(eppackage, "Only in Comp", _cd.InCompNotOrig());
        }

        private void WriteAllSheet(ExcelPackage eppackage)
        {
            WriteComparsions(eppackage, "ALL", _cd.InEntireResult());
        }

        private void WriteInSourceSheet(ExcelPackage eppackage)
        {
            WriteComparsions(eppackage, "In Source", _cd.InSource());
        }
        private void WriteInComparisonSheet(ExcelPackage eppackage)
        {
            WriteComparsions(eppackage, "In Comparison", _cd.InComparison());
        }

        #region Helper Methods
        /// <summary>
        /// The driver that creates a sheet in the excel using the resultsToPrint List
        /// </summary>
        /// <param name="eppackage"></param>
        /// <param name="sheetName"></param>
        /// <param name="ResultsToPrint"></param>
        private void WriteComparsions(ExcelPackage eppackage, string sheetName, List<OutDataStruct> ResultsToPrint)
        {
            ExcelWorksheet ws = eppackage.Workbook.Worksheets.Add(sheetName);

            Dictionary<string, int> ColKey = new Dictionary<string, int>();
            if (ws != null)
            {
                //Print for each result in the list
                int row = 1;
                foreach (OutDataStruct item in ResultsToPrint)
                {
                    if (row == 1)
                    {
                        ColKey = SetUpHeaders(ws);
                        row++;
                    }

                    int col = 1;
                    ws.Cells[row, col].Value = item.Key;
                    col++;

                    //This keeps track of which columns have or have NOT been populated
                    HashSet<string> columnsPopulated = new HashSet<string>();

                    foreach (var dat in item.Data)
                    {
                        col = ColKey[dat.Key];
                        columnsPopulated.Add(dat.Key);
                        object valueToWrite;

                        //Prioritize source dependent on bool
                        if ((dat.Value.Source == Source_Comparison.NEW && _prioritizeSource) ||
                            (dat.Value.Source == Source_Comparison.ORIG && !_prioritizeSource))
                        {
                            valueToWrite = "";
                        }
                        else
                        {
                            valueToWrite = _prioritizeSource ? dat.Value.original.Value ?? "" : dat.Value.newer.Value ?? "";
                        }
                        ws.Cells[row, col].Value = valueToWrite;

                        //Comments and hihglighting
                        string commentText = null;
                        System.Drawing.Color color = System.Drawing.Color.Transparent;
                        if (dat.Value.Source == Source_Comparison.NEW)
                        {
                            commentText = $"Warning this was only found in {dat.Value.Source};" +
                               $" The newer value is {dat.Value.newer.Value};";
                            color = System.Drawing.Color.LightYellow;
                        }
                        else if (dat.Value.Source == Source_Comparison.ORIG)
                        {
                            commentText = $"Warning this was only found in {dat.Value.Source};" +
                               $" The newer value is {dat.Value.original.Value};";
                            color = System.Drawing.Color.LightYellow;
                        }
                        else if (dat.Value.delta.DeltaType != DeltaType.UNCOMPARABLE && dat.Value.delta.DeltaValue != 0)
                        {
                            commentText = $"The original value was {dat.Value.original.Value};" +
                                $" the newer value is {dat.Value.newer.Value};" +
                                $" The delta is {dat.Value.delta.DeltaValue} for {dat.Value.delta.DeltaType} type";
                            color = System.Drawing.Color.LightSalmon;
                        }

                        if (!String.IsNullOrEmpty(commentText))
                        {
                            var comment = ws.Cells[row, col].AddComment(commentText, "KD");
                            ws.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(color);
                        }
                    }
                    col = ColKey["Source"];
                    ws.Cells[row, col].Value = item.RowSource;

                    //Identify columns that were not present in comparison
                    List<string> colsNotPresent = ColKey.Keys.Where(x => !columnsPopulated.Contains(x)).ToList();
                    foreach (string toImpute in colsNotPresent)
                    {
                        col = ColKey[toImpute];
                        if (ws.Cells[row, col].Value == null)
                        {

                            //Comments and hihglighting
                            System.Drawing.Color color = System.Drawing.Color.LightYellow;
                            string source = !_prioritizeSource ? "Comparison" : "Original";
                            string commentText = $"Warning column was only found in {source};" +
                                   $" This cell doesn't have a value due to the column not existing.";
                            try
                            {

                                var comment = ws.Cells[row, col].AddComment(commentText, "KD");
                                ws.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(color);
                            }
                            catch (Exception e)
                            {
                            }
                        }

                    }

                    row++;
                }
            }
        }

        /// <summary>
        /// This method creates Headers that combines the headers from both sheets to create a dictionary that will order the values
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        private Dictionary<string, int> SetUpHeaders(ExcelWorksheet ws)
        {
            int row = 1;
            int col = 1;

            Dictionary<int, string> compColKeyLookup = _cd.GetCompColKeyLookup();
            Dictionary<int, string> origColKeyLookup = _cd.GetOrigColKeyLookup();
            Dictionary<string, int> headersForSheet = new Dictionary<string, int>();

            headersForSheet.Add("KEY", col);
            ws.Cells[row, col].Value = "KEY";
            ws.Cells[row, col].Style.Font.Bold = true;

            col++;
            HashSet<string> items = new HashSet<string>();

            //This prioritize the ORIGINAL's headers in the ordering
            foreach (KeyValuePair<int, string> dat in origColKeyLookup)
            {
                headersForSheet.Add(dat.Value, col);
                ws.Cells[row, col].Value = dat.Value;
                ws.Cells[row, col].Style.Font.Bold = true;
                col++;
            }

            //We then add columns that were only in the comparison at the end of the worksheet
            List<string> remainingCols = compColKeyLookup.Select(x => x.Value).Except(headersForSheet.Keys).ToList();
            foreach (string colName in remainingCols)
            {
                headersForSheet.Add(colName, col);
                ws.Cells[row, col].Value = colName;
                ws.Cells[row, col].Style.Font.Bold = true;
                col++;
            }

            //Finally add the SOURCE 
            headersForSheet.Add("Source", col);
            ws.Cells[row, col].Value = "Source";
            ws.Cells[row, col].Style.Font.Bold = true;

            return headersForSheet;
        }
        #endregion
        #endregion

    }
}
