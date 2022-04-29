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
        /// Assumes that there is a KEY for each row. TODO: if doing aboslute comparison is the KEY just the row #?
        /// </summary>
        /// <param name="original"></param>
        /// <param name="comparison"></param>
        /// <returns></returns>
        public List<OutDataStruct> ConductComparisons(List<InDataStruct> original, List<InDataStruct> comparison)
        {
            //Step 1. Find rows in both or only in original:
            foreach (InDataStruct orig in original)
            {
                //1.1 Get those with the same key (if they exist) from comparison
                InDataStruct? comp = comparison.SingleOrDefault(x => x.Key == orig.Key);
                OutDataStruct resultComparsion = new OutDataStruct();
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
                if (inBoth.Any(x => x.Data.ContainsKey(comp.Key)))
                {
                    continue;
                }
                
                OutDataStruct resultComparsion = new OutDataStruct();

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

            comparisonResult = new List<OutDataStruct>();
            //TODO: append vs addrange?
            comparisonResult.AddRange(this.inBoth);
            comparisonResult.AddRange(this.inCompNotOrig);
                comparisonResult.AddRange(this.inOrigNotComp);
            return comparisonResult;
        }


        public List<OutDataStruct> InBoth()
        {
            if (comparisonResult.Count() == 0)
            {
                throw new InvalidOperationException("Cannot call methods about comparison before conducting comparison");
            }

            return inBoth;
        }
        public List<OutDataStruct> InCompNotOrig()
        {
            if (comparisonResult.Count() == 0)
            {
                throw new InvalidOperationException("Cannot call methods about comparison before conducting comparison");
            }

            return inCompNotOrig;
        }

        public List<OutDataStruct> InOrigNotComp()
        {
            if (comparisonResult.Count() == 0)
            {
                throw new InvalidOperationException("Cannot call methods about comparison before conducting comparison");
            }

            return inOrigNotComp;
        }

        public List<OutDataStruct> InSource()
        {
            if (true || comparisonResult.Count() == 0)
            {
                throw new InvalidOperationException("Cannot call methods about comparison before conducting comparison");
            }

            return null; //TODO: get both inBoth and inOrigNotComp inBoth.Concat(inOrigNotComp);
        }

        public void MergeStatistics()
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

            int inSourceOnly = inOrigNotComp.Count();
            int inCompOnly = inCompNotOrig.Count();
            int inBothCount = inBoth.Count();
            int sumComparisons = (inSourceOnly + inCompOnly + inBothCount);

            if (sumComparisons != comparisonResult.Count())
            {
                throw new DataMisalignedException("Fatal error. Merge should be exact");
            }

            //TODO: alignment of text based on numbers
            Console.WriteLine("|----------------------------------|");
            Console.WriteLine("| Location       | COUNT | CULM PERCENT |");
            Console.WriteLine("|----------------------------------|");
            Console.WriteLine($"| In Source Only | {inSourceOnly} | {inSourceOnly / sumComparisons} |");
            Console.WriteLine($"| In Comp Only   | {inCompOnly} | {inCompOnly / sumComparisons} |");
            Console.WriteLine($"| In Both        | {inBothCount} | {inBothCount / sumComparisons} |");
            Console.WriteLine("|----------------------------------|");

        }

        //TODO: merge results/analytics
    }
}
