// See https://aka.ms/new-console-template for more information
using Compare_excel_library.Compare_Methods;
using Compare_excel_library.Data_Structures;
using Compare_excel_library.IO;

Console.WriteLine("Hello, World!");

//TODO: Driver (this class)
//TODO: IO and paramaters
ExcelReader er = new ExcelReader();
//TODO: switch to relative filepath
List<InDataStruct> orig = er.ReadExcelData(@"C:\Projects\excel-comparer\Compare_excel_library\original.xlsx");
List<InDataStruct> comp = er.ReadExcelData(@"C:\Projects\excel-comparer\Compare_excel_library\comparison.xlsx");
//DONE-ish: Comparisons inner workings
ConductComparisons cd = new ConductComparisons(orig, comp);
cd.PrintMergeStatistics();
cd.PrintKeysOnlyInComp();
cd.PrintKeysOnlyInOrig();
//TODO: Output file
ExcelWriter ew = new ExcelWriter(cd);
ew.WriteReport(@"C:\Projects\excel-comparer\Compare_excel_library\report1.xlsx", "TODO", true);
ew.WriteReport(@"C:\Projects\excel-comparer\Compare_excel_library\report2.xlsx", "TODO", false);

//TODO: clean up exceptions?
//TODO: Library or blazor app?

