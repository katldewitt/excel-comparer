// See https://aka.ms/new-console-template for more information
using Compare_excel_library.Compare_Methods;
using Compare_excel_library.Data_Structures;
using Compare_excel_library.IO;

Console.WriteLine("Hello, World!");

//TODO: Driver (this class)
//TODO: IO and paramaters
ExcelReader er = new ExcelReader();
//TODO: switch to relative filepath
Dictionary<string, ExcelSheetForComparison> orig = er.ReadEntireExcel(@"C:\Projects\excel-comparer\Compare_excel_library\assets\original.xlsx", ExcelReader.ColKeyOptions.ROW_NUMBER);
Dictionary<string, ExcelSheetForComparison> comp = er.ReadEntireExcel(@"C:\Projects\excel-comparer\Compare_excel_library\assets\comparison.xlsx", ExcelReader.ColKeyOptions.ROW_NUMBER);
//DONE-ish: Comparisons inner workings
ConductComparisons cd = new ConductComparisons(orig["Sheet1"], comp["Sheet1"]);
cd.PrintMergeStatistics();
cd.PrintKeysOnlyInComp();
cd.PrintKeysOnlyInOrig();
cd.PrintColsOnlyInComp();
cd.PrintColsOnlyInOrig();
//TODO: Output file
ExcelWriter ew = new ExcelWriter(cd);
ew.WriteReport(@"C:\Projects\excel-comparer\Compare_excel_library\report1.xlsx", true);
ew.WriteReport(@"C:\Projects\excel-comparer\Compare_excel_library\report2.xlsx", false);

//TODO: clean up exceptions?
//TODO: Library or blazor app?

