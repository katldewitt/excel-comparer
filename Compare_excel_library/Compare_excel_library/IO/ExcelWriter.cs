﻿using Compare_excel_library.Compare_Methods;
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

        public void WriteReport(string filePath, string type, bool prioritizeSource)
        {
            this._prioritizeSource = prioritizeSource;
            using (var eppackage = new ExcelPackage())
            {
                WriteInBothSheet(eppackage);
                WriteInSourceOnly(eppackage);
                WriteOnlyInComp(eppackage);

                eppackage.SaveAs(new FileInfo(filePath));
            }
        }

        private void WriteInBothSheet(ExcelPackage eppackage)
        {
            //TODO: Handle multiple worksheets?
            ExcelWorksheet ws = eppackage.Workbook.Worksheets.Add("In Both");

            Dictionary<string, int> ColKey = new Dictionary<string, int>();
            if (ws != null)
            {
                //Step 1. Do for InBoth()
                int row = 1;
                foreach (OutDataStruct item in _cd.InBoth())
                {
                    if (row == 1)
                    {
                        ColKey = SetUpHeaders(ws);
                        row++;
                    }

                    int col = 1; 
                    ws.Cells[row, col].Value = item.Key;
                    col++;
                    foreach (var dat in item.Data)
                    {
                        col = ColKey[dat.Key];
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

                        if (!String.IsNullOrEmpty(commentText)) {
                            var comment = ws.Cells[row, col].AddComment(commentText, "KD");
                            ws.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(color);
                        }
                    }
                    col = ColKey["Source"];
                    ws.Cells[row, col].Value = "IN BOTH";
                    row++;


                }
            }

        }

        /// <summary>
        /// This method returns headers based on the first OutDataStruct 
        /// (which is only useful if using OnlyInComp or OnlyInSource)
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="ods"></param>
        private void SetUpHeaders(ExcelWorksheet ws, OutDataStruct ods)
        {
            int row = 1;
            int col = 1;
            ws.Cells[row, col].Value = "KEY";
            col++;
            //TODO: Set up the headers
            foreach (var dat in ods.Data.Values)
            {
                ws.Cells[row, col].Value = dat.colKey;
                col++;
            }
            ws.Cells[row, col].Value = "Source";
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

            headersForSheet.Add( "KEY", col);
            HashSet<string> items   = new HashSet<string>();

            //This prioritize the ORIGINAL's headers in the ordering
            foreach (KeyValuePair<int, string> dat in origColKeyLookup)
            {
                headersForSheet.Add(dat.Value, col);
                ws.Cells[row, col].Value = dat.Value;
                col++;
            }
            
            //We then add columns that were only in the comparison at the end of the worksheet
            List<string> remainingCols = compColKeyLookup.Select(x => x.Value).Except(headersForSheet.Keys).ToList();
            foreach (string colName in remainingCols)
            {
                headersForSheet.Add(colName, col);
                ws.Cells[row, col].Value = colName;
                col++;
            }

            //Finally add the SOURCE 
            headersForSheet.Add("Source", col );
            ws.Cells[row, col].Value = "Source";

            return headersForSheet;
        }

        private void WriteInSourceOnly(ExcelPackage eppackage)
        {
            ExcelWorksheet ws = eppackage.Workbook.Worksheets.Add("Only In Source");
            if (ws != null)
            {
                //Step 1. Do for InOrigNotComp()
                int row = 1;
                foreach (OutDataStruct item in _cd.InOrigNotComp())
                {
                    if (row == 1)
                    {
                        SetUpHeaders(ws, item);
                        row++;
                    }

                    int col = 1;

                    ws.Cells[row, col].Value = item.Key;
                    col++;
                    foreach (var dat in item.Data)
                    {
                        //Prioritize source dependent on bool
                        ws.Cells[row, col].Value = dat.Value.original.Value ?? "";
                        col++;
                    }
                    ws.Cells[row, col].Value = "InOrigNotComp";
                    row++;

                }
            }
        }

        private void WriteOnlyInComp(ExcelPackage eppackage)
        {

            ExcelWorksheet ws = eppackage.Workbook.Worksheets.Add("Only In Comp");
            if (ws != null)
            {
                //Step 1. Do for InBoth()
                int row = 1;
                foreach (OutDataStruct item in _cd.InCompNotOrig())
                {
                    if (row == 1)
                    {
                        SetUpHeaders(ws, item);
                        row++;
                    }

                    int col = 1;
                    ws.Cells[row, col].Value = item.Key;
                    col++;
                    foreach (var dat in item.Data)
                    {
                        //Prioritize source dependent on bool
                        ws.Cells[row, col].Value = dat.Value.newer.Value ?? "";
                        col++;
                    }
                    ws.Cells[row, col].Value = "InCompNotOrig";
                    row++;

                }
            }
        }
    }
}
