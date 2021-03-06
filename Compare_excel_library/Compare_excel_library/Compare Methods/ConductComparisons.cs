using Compare_excel_library.Data_Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compare_excel_library.Compare_Methods
{
    public class ConductComparisons
    {
        private List<OutDataStruct> comparisonResult = new List<OutDataStruct>();
        private List<OutDataStruct> inOrigNotComp = new List<OutDataStruct>();
        private List<OutDataStruct> inCompNotOrig = new List<OutDataStruct>();
        private List<OutDataStruct> inBoth = new List<OutDataStruct>();

        private Dictionary<int, string> origColKey = new Dictionary<int, string>();
        private Dictionary<int, string> compColKey = new Dictionary<int, string>();


        public ConductComparisons(ExcelSheetForComparison original, ExcelSheetForComparison comparison)
        {
            this.origColKey = original.ColKeyLookup;
            this.compColKey = comparison.ColKeyLookup;
            ConductComparisonsLists(original.RowsOfData, comparison.RowsOfData);
        }


        /// <summary>
        /// Compares two Lists of IndataStruct for differences. Assumes that there is a unique KEY for each row. 
        /// </summary>
        /// <param name="original"></param>
        /// <param name="comparison"></param>
        /// <returns>the entire comparisonResult (regardless of whether merged or not)</returns>
        public void ConductComparisonsLists(List<InDataStruct> original, List<InDataStruct> comparison)
        {
            //Step 0. Validate data:
            //Verify at least 1 item to compare
            if (original.Count == 0 && comparison.Count == 0)
            {
                throw new IOException("Fatal Error. Both original and comparison are empty");
            }

            //Verify the unique key assumption
            if (original.Select(x => x.Key).Distinct().Count() != original.Count())
            {
                throw new DataMisalignedException("Fatal Error. The original input does not meet the unique key assumption");
            }
            if (comparison.Select(x => x.Key).Distinct().Count() != comparison.Count())
            {
                throw new DataMisalignedException("Fatal Error. The comparison input does not meet the unique key assumption");
            }

            //Step 1. Find rows in both or only in original:
            foreach (InDataStruct orig in original)
            {
                //1.1 Get those with the same key (if they exist) from comparison
                InDataStruct? comp = comparison.SingleOrDefault(x => x.Key == orig.Key);
                OutDataStruct resultComparsion = new OutDataStruct() { Key = orig.Key };
                if (comp != null)
                {
                    //1.1.1 Iterate through all the columns from orig
                    foreach (var item in orig.Data)
                    {
                        //1.1.2 Try to see if the column in orig also existed in comp
                        comp.Data.TryGetValue(item.Key, out var compDatum);

                        //1.1.3 call the comparer
                        OData compResult = Comparer.Compare(item.Value, compDatum);
                        //1.1.4: add back to final result
                        resultComparsion.Data[item.Key] = compResult;
                    }

                    //1.1.5 Iterate through the remaining columns from comp
                    foreach (var item in comp.Data)
                    {
                        //1.1.6 Short circuit: don't need to process columns that were in both (and therefore in the results already)
                        if (resultComparsion.Data.ContainsKey(item.Key))
                        {
                            continue;
                        }

                        //1.1.7 call the comparer
                        OData compResult = Comparer.Compare(null, item.Value);
                        //1.1.4: add back to final result
                        resultComparsion.Data[item.Key] = compResult;
                    }
                    resultComparsion.RowSource = Source_Comparison.BOTH;
                    this.inBoth.Add(resultComparsion);
                }
                else
                {
                    //1.2 if there is no row with the same values in comp, generate an empty comparison
                    foreach (var item in orig.Data)
                    {
                        //1.2.1 call the comparer
                        OData compResult = Comparer.Compare(item.Value, null);
                        //1.2.2: add back to final result
                        resultComparsion.Data[item.Key] = compResult;
                    }
                    resultComparsion.RowSource = Source_Comparison.ORIG;
                    this.inOrigNotComp.Add(resultComparsion);

                }
            } //End foreach of orig

            //Step 2. Find rows in comparison:
            foreach (InDataStruct comp in comparison)
            {
                //2.1 Short circuit: don't need to process rows that were in both (and therefore in the results)
                if (inBoth.Any(x => x.Key == comp.Key))
                {
                    continue;
                }

                OutDataStruct resultComparsion = new OutDataStruct() { Key = comp.Key };

                //2.2 if there is no row with the same values in orig, generate an empty comparison
                foreach (var item in comp.Data)
                {
                    //2.2.1 call the comparer
                    OData compResult = Comparer.Compare(null, item.Value);
                    //2.2.2: add back to final result
                    resultComparsion.Data[item.Key] = compResult;
                }
                resultComparsion.RowSource = Source_Comparison.NEW;
                this.inCompNotOrig.Add(resultComparsion);

            } //End foreach of comp

            //Return the aggregate of the comparisons
            comparisonResult.AddRange(this.inBoth);
            comparisonResult.AddRange(this.inCompNotOrig);
            comparisonResult.AddRange(this.inOrigNotComp);
        }


        public List<OutDataStruct> InBoth()
        {
            CheckComparisonConductedFirst();

            return inBoth;
        }
        public List<OutDataStruct> InCompNotOrig()
        {
            CheckComparisonConductedFirst();

            return inCompNotOrig;
        }

        public List<OutDataStruct> InOrigNotComp()
        {
            CheckComparisonConductedFirst();

            return inOrigNotComp;
        }

        public List<OutDataStruct> InSource()
        {
            CheckComparisonConductedFirst();

            //get both inBoth and inOrigNotComp since both of these were in the source
            List<OutDataStruct> result = new List<OutDataStruct>();
            result.AddRange(inBoth);
            result.AddRange(inOrigNotComp);
            return result;
        }

        public List<OutDataStruct> InComparison()
        {
            CheckComparisonConductedFirst();

            //get both inBoth and inCompNotOrig since both of these were in the comp
            List<OutDataStruct> result = new List<OutDataStruct>();
            result.AddRange(inBoth);
            result.AddRange(inCompNotOrig);
            return result;
        }

        public List<OutDataStruct> InEntireResult()
        {
            CheckComparisonConductedFirst();
            return this.comparisonResult;
        }


        /// <summary>
        /// Prints out a table of keys that were only in the comparison
        /// </summary>
        public void PrintKeysOnlyInComp()
        {
            CheckComparisonConductedFirst();
            PrintKeys("Comparison", inCompNotOrig);
        }

        /// <summary>
        ///  Prints out a table of keys that were only in the original
        /// </summary>
        public void PrintKeysOnlyInOrig()
        {
            CheckComparisonConductedFirst();
            PrintKeys("Original", inOrigNotComp);
        }

        public void PrintColsOnlyInOrig()
        {
            CheckComparisonConductedFirst();
            HashSet<string> keys = new HashSet<string>(this.origColKey.Values.Except(compColKey.Values));
            PrintCols("Original", keys);
        }

        public void PrintColsOnlyInComp()
        {
            CheckComparisonConductedFirst();
            HashSet<string> keys = new HashSet<string>(this.compColKey.Values.Except(origColKey.Values));
            PrintCols("Comparison", keys);
        }

        private void PrintCols(string grouping, HashSet<string> cols)
        {
            /// |----------------------------------|
            /// | Cols only in {Grouping}          | 
            /// |----------------------------------|
            /// | Column 1                         | 
            /// | Column 5                         | 
            /// | Column 18                        | 
            /// | Column 99                        | 
            /// |----------------------------------|

            //Define constants for printing
            int NUM_ITEMS = 1;
            int ADDITIONAL_CHARS_IN_ALIGNED_TXT = 0;

            if (cols.Count == 0)
            {
                string Header = $"There are no columns that were only in {grouping}";
                PrintDividingLine(Header.Length, NUM_ITEMS, ADDITIONAL_CHARS_IN_ALIGNED_TXT);
                Console.WriteLine(Header);
                PrintDividingLine(Header.Length, NUM_ITEMS, ADDITIONAL_CHARS_IN_ALIGNED_TXT);
            }
            else
            {
                string Header = $"Columns only in {grouping}";
                int maxLength = Math.Max(cols.Max(x => x.Length), Header.Length);

                PrintDividingLine(maxLength, NUM_ITEMS, ADDITIONAL_CHARS_IN_ALIGNED_TXT);
                Console.WriteLine(PrintAlignedText(new List<string>() { Header }, maxLength));
                PrintDividingLine(maxLength, NUM_ITEMS, ADDITIONAL_CHARS_IN_ALIGNED_TXT);
                foreach (string item in cols)
                {
                    Console.WriteLine(PrintAlignedText(new List<string>() { item }, maxLength));
                }
                PrintDividingLine(maxLength, NUM_ITEMS, ADDITIONAL_CHARS_IN_ALIGNED_TXT);
            }
        }

        private void PrintKeys(string grouping, List<OutDataStruct> listing)
        {
            /// |----------------------------------|
            /// | Keys only in {Grouping}          | 
            /// |----------------------------------|
            /// | Key1                             |  
            /// | Key112357465165768               |  
            /// | Key9999999999999999999999999     | 
            /// |----------------------------------|

            //Define constants for printing
            int NUM_ITEMS = 1;
            int ADDITIONAL_CHARS_IN_ALIGNED_TXT = 0;

            if (listing.Count == 0)
            {
                string Header = $"There are no items that were only in {grouping}";
                PrintDividingLine(Header.Length, NUM_ITEMS, ADDITIONAL_CHARS_IN_ALIGNED_TXT);
                Console.WriteLine(Header);
                PrintDividingLine(Header.Length, NUM_ITEMS, ADDITIONAL_CHARS_IN_ALIGNED_TXT);
            }
            else
            {
                string Header = $"Keys only in {grouping}";
                List<string> keysToPrint = listing.Select(x => x.Key).ToList();
                int maxLength = Math.Max(keysToPrint.Max(x => x.Length), Header.Length);

                PrintDividingLine(maxLength, NUM_ITEMS, ADDITIONAL_CHARS_IN_ALIGNED_TXT);
                Console.WriteLine(PrintAlignedText(new List<string>() { Header }, maxLength));
                PrintDividingLine(maxLength, NUM_ITEMS, ADDITIONAL_CHARS_IN_ALIGNED_TXT);
                foreach (string item in keysToPrint)
                {
                    Console.WriteLine(PrintAlignedText(new List<string>() { item }, maxLength));
                }
                PrintDividingLine(maxLength, NUM_ITEMS, ADDITIONAL_CHARS_IN_ALIGNED_TXT);
            }
        }

        /// <summary>
        /// Provide a summary of how many items merged, how many items were only in comparison, and how many were only in source.
        /// </summary>
        /// <exception cref="DataMisalignedException"></exception>
        public void PrintMergeStatistics()
        {
            /// |----------------------------------|
            /// | Location       | COUNT | PERCENT |            
            /// |----------------------------------|
            /// | In Source Only | COUNT | PERCENT | 
            /// | In Comp Only   | COUNT | PERCENT | 
            /// | In Both        | COUNT | PERCENT |
            /// |----------------------------------|

            double inSourceOnly = inOrigNotComp.Count();
            double inCompOnly = inCompNotOrig.Count();
            double inBothCount = inBoth.Count();
            double sumComparisons = (inSourceOnly + inCompOnly + inBothCount);

            if (sumComparisons != comparisonResult.Count())
            {
                throw new DataMisalignedException("Fatal error. Merge should be 1:1");
            }

            //Determine max length of counts to compare to longest string used in table
            int maxLength = Math.Max(sumComparisons.ToString().Length, "In Source Only".Length);

            //Define constants for formatting
            int NUM_ITEMS = 3;
            int ADDITIONAL_CHARS_IN_ALIGNED_TXT = 6;

            PrintDividingLine(maxLength, NUM_ITEMS, ADDITIONAL_CHARS_IN_ALIGNED_TXT);
            Console.WriteLine(PrintAlignedText(new List<string>() { "Location", "COUNT", "CULM PERCENT" }, maxLength));
            PrintDividingLine(maxLength, NUM_ITEMS, ADDITIONAL_CHARS_IN_ALIGNED_TXT);
            Console.WriteLine(PrintAlignedText(new List<string>() { "In Source Only", inSourceOnly.ToString(), String.Format("{0:0.##}", inSourceOnly / sumComparisons) }, maxLength));
            Console.WriteLine(PrintAlignedText(new List<string>() { "In Comp Only", inCompOnly.ToString(), String.Format("{0:0.##}", inCompOnly / sumComparisons) }, maxLength));
            Console.WriteLine(PrintAlignedText(new List<string>() { "In Both", inBothCount.ToString(), String.Format("{0:0.##}", inBothCount / sumComparisons) }, maxLength));
            PrintDividingLine(maxLength, NUM_ITEMS, ADDITIONAL_CHARS_IN_ALIGNED_TXT);

        }


        private string PrintAlignedText(List<string> itemsToFormat, int maxLength)
        {
            StringBuilder sb = new StringBuilder("|");
            int counter = 0;
            foreach (var item in itemsToFormat)
            {
                //Becomes |{0, maxLength} | {1, maxLength} | ... | {N, maxLength}|
                sb.Append("{" + counter + ", " + maxLength + "}");
                if (counter != itemsToFormat.Count - 1)
                {
                    sb.Append(" | ");
                }
                else
                {
                    sb.Append("|");
                }
                counter++;
            }

            //Becomes |     item 1 |    item 2 | ... |       item N|
            return String.Format(sb.ToString(), itemsToFormat.ToArray());

        }
        private void PrintDividingLine(int maxLength, int NUM_ITEMS, int ADDITIONAL_CHARS_IN_ALIGNED_TXT)
        {
            Console.WriteLine("|" + new string('-', maxLength * NUM_ITEMS + ADDITIONAL_CHARS_IN_ALIGNED_TXT) + "|");
        }

        private void CheckComparisonConductedFirst()
        {
            if (comparisonResult.Count() == 0)
            {
                throw new InvalidOperationException("Cannot call methods about comparison before conducting comparison");
            }
        }

        public Dictionary<int, string> GetOrigColKeyLookup()
        {
            return this.origColKey;
        }

        public Dictionary<int, string> GetCompColKeyLookup()
        {
            return this.compColKey;
        }
    }
}
