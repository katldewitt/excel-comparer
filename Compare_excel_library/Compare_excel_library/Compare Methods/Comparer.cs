using Compare_excel_library.Data_Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compare_excel_library.Compare_Methods
{
    public class Comparer
    {

        private static Delta Uncomparable = new Delta()
        {
            DeltaType = DeltaType.UNCOMPARABLE,
            DeltaValue = int.MaxValue
        };

        /// <summary>
        /// Compare two Datums to identify deltas and comparison type. Can handle null values in one or both inputs
        /// </summary>
        /// <param name="orig">The original datum</param>
        /// <param name="comp">The datum to which we are comparing</param>
        /// <returns>an OData </returns>
        /// <exception cref="NotImplementedException"></exception>
        public static OData Compare(Datum? orig, Datum? comp)
        {

            OData result = new OData()
            {
                original = orig,
                newer = comp,
            };

            //If both are null, can't compare
            if (orig == null && comp == null)
            {
                result.delta = Uncomparable;
                return result;
            }

            //Edge case: Either are null, so can't compare
            if (orig == null)
            {
                result.Source = Source_Comparison.NEW;
                result.delta = Uncomparable;
                result.colKey = comp.ColKey;
            }
            else if (comp == null)
            {
                result.Source = Source_Comparison.ORIG;
                result.delta = Uncomparable;
                result.colKey = orig.ColKey;

            }
            else
            {
                result.colKey = orig.ColKey;
                result.Source = Source_Comparison.BOTH;

                //Edge case: types don't match, so can't compare
                if (orig.Type != comp.Type)
                {
                    result.delta = Uncomparable;
                }
                else
                {

                    //Assume types are the same given the above edge case handling
                    switch (orig.Type.ToString().ToLower().Replace("system.", ""))
                    {
                        case "string":
                            result.delta = CompareString(orig, comp);
                            break;
                        case "int32":
                        case "double":
                        case "single": //float
                        case "decimal": 
                            result.delta = CompareNumeric(orig, comp);
                            break;
                        case "boolean":
                            result.delta = CompareBool(orig, comp);
                            break;
                        case "datetime":
                            result.delta = CompareDate(orig, comp);
                            break;
                        default:
                            throw new NotImplementedException($"Comparison of type {orig.Type} is not yet implemented");
                            result.delta = Uncomparable;
                            break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// returns large value if different. 0 represents no change
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        private static Delta CompareString(Datum orig, Datum comp)
        {
            return new Delta()
            {
                DeltaType = DeltaType.STRING,
                DeltaValue = CalcLevenshteinDistance((string)orig.Value, (string)comp.Value)
            };
        }

        /// <summary>
        /// Compares two bool values. 0 represents no change
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        private static Delta CompareBool(Datum orig, Datum comp)
        {
            return new Delta()
            {
                DeltaType = DeltaType.BOOL,
                DeltaValue = (bool)orig.Value == (bool)comp.Value ? 0 : 1,
            };
        }

        /// <summary>
        /// Compares two DateTime values. 0 represents no change. Order is orig compared to comp
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        private static Delta CompareDate(Datum orig, Datum comp)
        {
            return new Delta()
            {
                DeltaType = DeltaType.DATE,
                DeltaValue = ((DateTime)orig.Value).CompareTo((DateTime)comp.Value)
            };
        }

        /// <summary>
        /// Returns the abs value of the difference of the two numbers. 0 represents no change
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        private static Delta CompareNumeric(Datum orig, Datum comp)
        {
            double deltaV = int.MaxValue;
            switch (orig.Type.ToString().ToLower().Replace("system.", ""))
            {
                case "single": //float
                    deltaV = Math.Abs((float)orig.Value - (float)comp.Value);
                    break;
                case "int32":
                    deltaV = Math.Abs((int)orig.Value - (int)comp.Value);
                    break;
                case "double":
                    deltaV = Math.Abs((double)orig.Value - (double)comp.Value);
                    break;
                case "decimal":
                    deltaV = (double)Math.Abs((decimal)orig.Value - (decimal)comp.Value);
                    break;
                default:
                    break;
            }
            return new Delta()
            {
                DeltaType = DeltaType.NUMERIC,
                DeltaValue = deltaV,
            };
        }


        /// <summary>
        /// Algorithm for comparing two strings to each other to get a representation of similarity. 0 represents no change
        /// SOURCE:https://stackoverflow.com/questions/9453731/how-to-calculate-distance-similarity-measure-of-given-2-strings
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>an int representing the distance between strings</returns>
        private static int CalcLevenshteinDistance(string a, string b)
        {
            //Two strings are the same if they are both ""
            if (String.IsNullOrEmpty(a) && String.IsNullOrEmpty(b))
            {
                return 0;
            }
            if (String.IsNullOrEmpty(a))
            {
                return b.Length;
            }
            if (String.IsNullOrEmpty(b))
            {
                return a.Length;
            }
            int lengthA = a.Length;
            int lengthB = b.Length;
            var distances = new int[lengthA + 1, lengthB + 1];
            for (int i = 0; i <= lengthA; distances[i, 0] = i++);
            for (int j = 0; j <= lengthB; distances[0, j] = j++);

            for (int i = 1; i <= lengthA; i++)
                for (int j = 1; j <= lengthB; j++)
                {
                    int cost = b[j - 1] == a[i - 1] ? 0 : 1;
                    distances[i, j] = Math.Min
                        (
                        Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                        distances[i - 1, j - 1] + cost
                        );
                }
            return distances[lengthA, lengthB];
        }
    }
}
