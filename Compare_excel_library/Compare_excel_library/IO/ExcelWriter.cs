using Compare_excel_library.Compare_Methods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if (ws != null)
            {
                //Step 1. Do for InBoth()
                int row = 1;
                foreach (OutDataStruct item in _cd.InBoth())
                {
                    int col = 1;

                    if (row == 1)
                    {
                        ws.Cells[row, col].Value = "KEY";
                        col++;
                        //TODO: Set up the headers
                        foreach (var dat in item.Data.Values)
                        {
                            ws.Cells[row, col].Value = dat.colKey;
                            col++;
                        }
                        ws.Cells[row, col].Value = "Source";
                        row++;
                    }

                    col = 1;
                    ws.Cells[row, col].Value = item.Key;
                    col++;
                    foreach (var dat in item.Data)
                    {
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

                        if (dat.Value.delta.DeltaType != DeltaType.UNCOMPARABLE && dat.Value.delta.DeltaValue != 0)
                        {
                            string commentText = $"The original value was {dat.Value.original.Value};" +
                                $" the newer value is {dat.Value.newer.Value};" +
                                $" The delta is {dat.Value.delta.DeltaValue} for {dat.Value.delta.DeltaType} type";
                            var comment = ws.Cells[row, col].AddComment(commentText, "KD");
                        }
                        col++;
                    }
                    ws.Cells[row, col].Value = "IN BOTH";
                    row++;


                }
            }

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
                    int col = 1;

                    if (row == 1)
                    {
                        ws.Cells[row, col].Value = "KEY";
                        col++;
                        //TODO: Set up the headers
                        foreach (var dat in item.Data.Values)
                        {
                            ws.Cells[row, col].Value = dat.colKey;
                            col++;
                        }
                        ws.Cells[row, col].Value = "Source";
                        row++;
                    }
                    col = 1;
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
                    int col = 1;

                    if (row == 1)
                    {
                        ws.Cells[row, col].Value = "KEY";
                        col++;
                        //TODO: Set up the headers across types (ordering changes since not all columns are present)
                        foreach (var dat in item.Data.Values)
                        {
                            ws.Cells[row, col].Value = dat.colKey;
                            col++;
                        }
                        ws.Cells[row, col].Value = "Source";
                        row++;
                    }
                    col = 1;
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
