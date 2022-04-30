using Compare_excel_library.Compare_Methods;
using Compare_excel_library.Data_Structures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Comp_xl_tests
{
    [TestClass]
    public class ComparerTests
    {
        #region Test Datum
        static string testKey = "testKey";
        Datum numericInt = new Datum(testKey, 123);
        Datum numericDouble = new Datum(testKey, 1.23);
        Datum datumBool = new Datum(testKey, true);
        Datum datumString = new Datum(testKey, "stringy");
        Datum datumStringChanged = new Datum(testKey, "strongy");
        Datum datumDateTimeToday = new Datum(testKey, DateTime.Today);
        Datum datumDateTimeTomorrow = new Datum(testKey, DateTime.Today.AddDays(1));
        Datum numericFloat = new Datum(testKey, 134.45E-2f);
        Datum numericDecimal = new Datum(testKey, 1.5E6m);

        #endregion
        
        private void CommonAssertionsForComparison(Datum? orig, Datum? comp, OData result)
        {
            Assert.AreEqual(orig, result.original);
            Assert.AreEqual(comp, result.newer);
            if (orig == null)
            {
                Assert.AreEqual(Source_Comparison.NEW, result.Source);
                Assert.AreEqual(comp.ColKey, result.colKey);
            }
            else if(comp == null)
            {
                Assert.AreEqual(Source_Comparison.ORIG, result.Source);
                Assert.AreEqual(orig.ColKey, result.colKey);
            }
            else
            {
                Assert.AreEqual(orig.ColKey, result.colKey);
                Assert.AreEqual(comp.ColKey, result.colKey);
                Assert.AreEqual(Source_Comparison.BOTH, result.Source);
            }
        }

        #region Compare To Self
        /// These comparisons are intended to demonstrate that an object compared to itself returns a delta of 0
        /// for the comparison type.
        
        [TestMethod]
        public void Compare_selfString()
        {
            OData result = Comparer.Compare(datumString, datumString);
            CommonAssertionsForComparison(datumString, datumString, result);
            Assert.AreEqual(DeltaType.STRING, result.delta.DeltaType);
            Assert.AreEqual(0, result.delta.DeltaValue);
        }
        [TestMethod]
        public void Compare_selfBool()
        {
            OData result = Comparer.Compare(datumBool, datumBool);
            CommonAssertionsForComparison(datumBool, datumBool, result);
            Assert.AreEqual(DeltaType.BOOL, result.delta.DeltaType);
            Assert.AreEqual(0, result.delta.DeltaValue);
        }

        [TestMethod]
        public void Compare_selfInt()
        {
            OData result = Comparer.Compare(numericInt, numericInt);
            CommonAssertionsForComparison(numericInt, numericInt, result);
            Assert.AreEqual(DeltaType.NUMERIC, result.delta.DeltaType);
            Assert.AreEqual(0, result.delta.DeltaValue);
        }

        [TestMethod]
        public void Compare_selfFloat()
        {
             OData result = Comparer.Compare(numericFloat, numericFloat);
            CommonAssertionsForComparison(numericFloat, numericFloat, result);
            Assert.AreEqual(DeltaType.NUMERIC, result.delta.DeltaType);
            Assert.AreEqual(0, result.delta.DeltaValue);
        }

        [TestMethod]
        public void Compare_selfDouble()
        {
            OData result = Comparer.Compare(numericDouble, numericDouble);
            CommonAssertionsForComparison(numericDouble, numericDouble, result);
            Assert.AreEqual(DeltaType.NUMERIC, result.delta.DeltaType);
            Assert.AreEqual(0, result.delta.DeltaValue);
        }

        [TestMethod]
        public void Compare_selfDate()
        {
            OData result = Comparer.Compare(datumDateTimeToday, datumDateTimeToday);
            CommonAssertionsForComparison(datumDateTimeToday, datumDateTimeToday, result);
            Assert.AreEqual(DeltaType.DATE, result.delta.DeltaType);
            Assert.AreEqual(0, result.delta.DeltaValue);
        }

        [TestMethod]
        public void Compare_selfDecimal()
        {
            OData result = Comparer.Compare(numericDecimal, numericDecimal);
            CommonAssertionsForComparison(numericDecimal, numericDecimal, result);
            Assert.AreEqual(DeltaType.NUMERIC, result.delta.DeltaType);
            Assert.AreEqual(0, result.delta.DeltaValue);
        }

        #endregion

        #region Compare Across Types
        /// These segments of tests demonstrate that it is not possible to compare across types

        [TestMethod]
        public void CompareDiffTypes_BoolString()
        {
            OData result = Comparer.Compare(datumBool, datumString);
            CommonAssertionsForComparison(datumBool, datumString, result);
            Assert.AreEqual(DeltaType.UNCOMPARABLE, result.delta.DeltaType);
        }
        #endregion

        #region Compare to Null
        [TestMethod]
        public void Compare_OrigNull()
        {
            OData result = Comparer.Compare(null, numericDouble);
            CommonAssertionsForComparison(null, numericDouble, result);
            Assert.AreEqual(DeltaType.UNCOMPARABLE, result.delta.DeltaType);
            Assert.AreEqual(int.MaxValue, result.delta.DeltaValue);
        }
        [TestMethod]
        public void Compare_CompNull()
        {
            OData result = Comparer.Compare(numericDouble, null);
            CommonAssertionsForComparison(numericDouble, null, result);
            Assert.AreEqual(DeltaType.UNCOMPARABLE, result.delta.DeltaType);
            Assert.AreEqual(int.MaxValue, result.delta.DeltaValue);
        }

        [TestMethod]
        public void Compare_BothNull()
        {
            //TODO: should this raise exception?
            OData result = Comparer.Compare(null, null);
            Assert.AreEqual(DeltaType.UNCOMPARABLE, result.delta.DeltaType);
            Assert.AreEqual(int.MaxValue, result.delta.DeltaValue);
        }
        #endregion

        #region Compare Values
        // / These segments of tests demonstrate the intended behavior of comparing 2 types of values.
        [TestMethod]
        public void Compare_TwoStrings()
        {
            OData result = Comparer.Compare(datumString, datumStringChanged);
            CommonAssertionsForComparison(datumString, datumStringChanged, result);
            Assert.AreEqual(DeltaType.STRING, result.delta.DeltaType);
            Assert.AreEqual(1, result.delta.DeltaValue);
        }

        [TestMethod]
        public void Compare_TwoDates()
        {
            OData result = Comparer.Compare(datumDateTimeToday, datumDateTimeTomorrow);
            CommonAssertionsForComparison(datumDateTimeToday, datumDateTimeTomorrow, result);
            Assert.AreEqual(DeltaType.DATE, result.delta.DeltaType);
            Assert.AreEqual(-1, result.delta.DeltaValue, "Failure. Today [orig] should be earlier than tomorrow [comp]");
        }
        #endregion

    }
}