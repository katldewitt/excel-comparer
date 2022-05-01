using Compare_excel_library.Data_Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compare_excel_library.Compare_Methods
{
    public class ComparisonDriver
    {
        private List<OutDataStruct> comparisonResult = new List<OutDataStruct>();
        private List<OutDataStruct> inOrigNotComp = new List<OutDataStruct>();
        private List<OutDataStruct> inCompNotOrig = new List<OutDataStruct>();
        private List<OutDataStruct> inBoth = new List<OutDataStruct>();

        /// <summary>
        /// Compares two Lists of IndataStruct for differences. Assumes that there is a unique KEY for each row. 
        /// </summary>
        /// <param name="original"></param>
        /// <param name="comparison"></param>
        /// <returns>the entire comparisonResult (regardless of whether merged or not)</returns>
        public List<OutDataStruct> ConductComparisons(List<InDataStruct> original, List<InDataStruct> comparison)
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
                OutDataStruct resultComparsion = new OutDataStruct() { Key = orig.Key};
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
                    this.inOrigNotComp.Add(resultComparsion);

                }
            } //End foreach of orig

            //Step 2. Find rows in comparison:
            foreach (InDataStruct comp in comparison)
            {
                //2.1 Short circuit: don't need to deal to process cases that were in both (and therefore in the results)
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
                this.inCompNotOrig.Add(resultComparsion);

            } //End foreach of comp

            //Return the aggregate of the comparisons
            comparisonResult.AddRange(this.inBoth);
            comparisonResult.AddRange(this.inCompNotOrig);
            comparisonResult.AddRange(this.inOrigNotComp);
            return comparisonResult;
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

        /// <summary>
        /// Prints out a table of keys that were only in the comparison
        /// </summary>
        public void PrintKeysOnlyInComp()
        {
            CheckComparisonConductedFirst();
            PrintKeys("Comparison", inCompNotOrig);
        }

        /// <summary>
        ///  Prints out a table of keys that were only in the comparison
        /// </summary>
        public void PrintKeysOnlyInOrig()
        {
            CheckComparisonConductedFirst();
            PrintKeys("Original", inOrigNotComp);
        }

        private void PrintKeys(string grouping, List<OutDataStruct> listing)
        {

            ///
            /// |----------------------------------|
            /// | Keys only in {Grouping}          | 
            /// |                                  | 
            /// | Key1                             |  
            /// | Key112357465165768               |  
            /// | Key9999999999999999999999999     | 
            /// |----------------------------------|
            ///

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
                Console.WriteLine(PrintAlignedText(Header, maxLength));
                PrintDividingLine(maxLength, NUM_ITEMS, ADDITIONAL_CHARS_IN_ALIGNED_TXT);
                foreach (string item in keysToPrint)
                {
                    Console.WriteLine(PrintAlignedText(item, maxLength));
                }
                PrintDividingLine(maxLength, NUM_ITEMS, ADDITIONAL_CHARS_IN_ALIGNED_TXT);

            }
        }



        public void PrintMergeStatistics()
        {
            ///
            /// |----------------------------------|
            /// | Location       | COUNT | PERCENT | 
            /// |                                  | 
            /// | In Source Only | COUNT | PERCENT | 
            /// | In Comp Only   | COUNT | PERCENT | 
            /// | In Both        | COUNT | PERCENT |
            /// |----------------------------------|
            ///

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
            ////Define constants for formatting
            int NUM_ITEMS = 3;
            int ADDITIONAL_CHARS_IN_ALIGNED_TXT = 6;
            PrintDividingLine(maxLength, NUM_ITEMS, ADDITIONAL_CHARS_IN_ALIGNED_TXT);
            Console.WriteLine(PrintAlignedText("Location", "COUNT", "CULM PERCENT", maxLength));
            PrintDividingLine(maxLength, NUM_ITEMS, ADDITIONAL_CHARS_IN_ALIGNED_TXT);
            Console.WriteLine(PrintAlignedText("In Source Only", inSourceOnly.ToString(), String.Format("{0:0.##}", inSourceOnly / sumComparisons), maxLength));
            Console.WriteLine(PrintAlignedText("In Comp Only", inCompOnly.ToString(), String.Format("{0:0.##}", inCompOnly / sumComparisons), maxLength));
            Console.WriteLine(PrintAlignedText("In Both", inBothCount.ToString(), String.Format("{0:0.##}", inBothCount / sumComparisons), maxLength));
            PrintDividingLine(maxLength, NUM_ITEMS, ADDITIONAL_CHARS_IN_ALIGNED_TXT);

        }

        private string PrintAlignedText(string item1, int maxLength)
        {
            string toFormat = "|{0, []}|".Replace("[]", maxLength.ToString());
            return String.Format(toFormat, item1);

        }

        private string PrintAlignedText(string item1, string item2, string item3, int maxLength)
        {
            string toFormat = "|{0, []} | {1, []} | {2, []}|".Replace("[]", maxLength.ToString());
            return String.Format(toFormat, item1, item2, item3);

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
    }
}
