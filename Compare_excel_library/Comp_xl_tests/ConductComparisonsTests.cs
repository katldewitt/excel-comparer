using Compare_excel_library.Compare_Methods;
using Compare_excel_library.Data_Structures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

namespace Comp_xl_tests
{
    [TestClass]
    public class ConductComparisonTests
    {
        #region Test Data

        static string testKey = "testKey";
        static readonly Datum numericInt = new Datum(testKey + "_int", 123);
        static readonly Datum numericDouble = new Datum(testKey + "_dbl", 1.23);
        static readonly Datum datumBool = new Datum(testKey + "_bool", true);
        static readonly Datum datumString = new Datum(testKey + "_string", "stringy");
        static readonly Datum datumStringChanged = new Datum(testKey + "_string", "strongy");
        static readonly Datum datumDateTimeToday = new Datum(testKey + "_dt", DateTime.Today);
        static readonly Datum datumDateTimeTomorrow = new Datum(testKey + "_dt", DateTime.Today.AddDays(1));

        static InDataStruct inDataStructureTest = new InDataStruct()
        {
            Key = "Key#1",
            Data = new Dictionary<string, Datum>()
            {
                {numericInt.ColKey, numericInt },
                {datumString.ColKey, datumString }, //Changed
                {datumDateTimeToday.ColKey, datumDateTimeToday }, //Changed
                {datumBool.ColKey, datumBool }, //Only in Test
            }
        };
        static InDataStruct inDataStructureCompTest = new InDataStruct()
        {
            Key = "Key#1",
            Data = new Dictionary<string, Datum>()
            {
                {numericInt.ColKey, numericInt },
                {datumStringChanged.ColKey, datumStringChanged }, //Changed
                {datumDateTimeTomorrow.ColKey, datumDateTimeTomorrow }, //Changed
                {numericDouble.ColKey, numericDouble }, //Only in Comp
            }
        };

        //leverage the set operations to verify expection of comparison at the row level
        int IN_BOTH_EXPECTED_ROW;
        int IN_SOURCE_ONLY_EXPECTED_ROW;
        int IN_COMP_ONLY_EXPECTED_ROW;
        int IN_SOURCE_EXPECTED_ROW;
        int IN_COMP_EXPECTED_ROW;

        //Leverage the set operations to verify expectation of comparison at column level
        int IN_BOTH_EXPECTED_COL = inDataStructureCompTest.Data.Select(x => x.Key).Intersect(inDataStructureTest.Data.Select(x => x.Key)).Count();
        int TOTAL_EXPECTED_COL = inDataStructureCompTest.Data.Select(x => x.Key).Union(inDataStructureTest.Data.Select(x => x.Key)).Count();
        int IN_COMP_ONLY_EXPECTED_COL = inDataStructureCompTest.Data.Select(x => x.Key).Except(inDataStructureTest.Data.Select(x => x.Key)).Count();
        int IN_ORIG_ONLY_EXPECTED_COL = inDataStructureTest.Data.Select(x => x.Key).Except(inDataStructureCompTest.Data.Select(x => x.Key)).Count();
        #endregion

        private ConductComparisons ConductComparisonTester()
        {
            List<InDataStruct> original = new List<InDataStruct>() { inDataStructureTest };
            List<InDataStruct> comparison = new List<InDataStruct>() { inDataStructureCompTest };

            IN_BOTH_EXPECTED_ROW = original.Select(x => x.Key).Intersect(comparison.Select(x => x.Key)).Count();
            IN_SOURCE_ONLY_EXPECTED_ROW = original.Select(x => x.Key).Except(comparison.Select(x => x.Key)).Count();
            IN_COMP_ONLY_EXPECTED_ROW = comparison.Select(x => x.Key).Except(original.Select(x => x.Key)).Count();
            IN_SOURCE_EXPECTED_ROW = original.Count();
            IN_COMP_EXPECTED_ROW = comparison.Count();

            return new ConductComparisons(original, comparison);
        }

        #region Step 0. Validate Data Assummptions
        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void ConductComparisons_EmptyLists()
        {
            ConductComparisons condComp = new ConductComparisons(new List<InDataStruct>(), new List<InDataStruct>());
        }
        [TestMethod]
        [ExpectedException(typeof(DataMisalignedException))]
        public void ConductComparisons_NonUniqueKeyOrig()
        {
            List<InDataStruct> nonUniqueKey = new List<InDataStruct>() { inDataStructureTest, inDataStructureTest };
            ConductComparisons condComp = new ConductComparisons(nonUniqueKey, new List<InDataStruct>());
        }
        [TestMethod]
        [ExpectedException(typeof(DataMisalignedException))]
        public void ConductComparisons_NonUniqueKeyComp()
        {
            List<InDataStruct> nonUniqueKey = new List<InDataStruct>() { inDataStructureTest, inDataStructureTest };
            ConductComparisons condComp = new ConductComparisons(new List<InDataStruct>(), nonUniqueKey);
        }
        #endregion

        #region Conduct Comparisons Constructor
        [TestMethod]
        public void ConstructorValidityTests()
        {
            ConductComparisons cd = ConductComparisonTester();

            //1. Check the expected counts at the obs/row level
            List<OutDataStruct> bothList = cd.InBoth();
            Assert.AreEqual(IN_BOTH_EXPECTED_ROW, bothList.Count);
            Assert.AreEqual(IN_SOURCE_EXPECTED_ROW, cd.InSource().Count);
            Assert.AreEqual(IN_COMP_EXPECTED_ROW, cd.InComparison().Count);
            Assert.AreEqual(IN_COMP_ONLY_EXPECTED_ROW, cd.InCompNotOrig().Count);
            Assert.AreEqual(IN_SOURCE_ONLY_EXPECTED_ROW, cd.InOrigNotComp().Count);
            Assert.IsTrue(cd.InComparison().SequenceEqual(cd.InSource()),
                "Since all rows are in both, we should see the same sequence InCompariso and InSource");
            Assert.IsTrue(cd.InBoth().SequenceEqual(cd.InSource()),
                "Since all rows are in both, we should see the same sequence InBoth and InSource");

            //2. Verify checks at the column level
            OutDataStruct columnLevelChecks = bothList.FirstOrDefault();
            Assert.IsNotNull(columnLevelChecks);
            
            //2.1 Check the counts
            Assert.AreEqual(TOTAL_EXPECTED_COL, columnLevelChecks.Data.Count);
            Assert.AreEqual(IN_ORIG_ONLY_EXPECTED_COL, columnLevelChecks.Data.Where(x => x.Value.Source == Source_Comparison.ORIG).Count());
            Assert.AreEqual(IN_BOTH_EXPECTED_COL, columnLevelChecks.Data.Where(x => x.Value.Source == Source_Comparison.BOTH).Count());
            Assert.AreEqual(IN_COMP_ONLY_EXPECTED_COL, columnLevelChecks.Data.Where(x => x.Value.Source == Source_Comparison.NEW).Count());

            //3. Verify changes at the data level
            List<OData> withChanges = columnLevelChecks.Data
                                        .Where(x => x.Value.Source == Source_Comparison.BOTH) //Changes only occur in records in both
                                        .Select(z => z.Value) //We no longer need the Dict structure
                                        .ToList();
            Assert.AreEqual(1, withChanges.Where(x => x.delta.DeltaValue == 0).Count());
            Assert.AreEqual(2, withChanges.Where(x => x.delta.DeltaValue != 0).Count(), "There should be 2 items");
            Assert.IsTrue(withChanges.Any(x => x.colKey == datumString.ColKey));
            Assert.IsTrue(withChanges.Any(x => x.colKey == datumDateTimeToday.ColKey));

        }
        #endregion

        #region Conduct Comparisons Public Methods
        [TestMethod]
        public void InBoth_ReturnsValue()
        {
            ConductComparisons cd = ConductComparisonTester();
            Assert.IsNotNull(cd.InBoth());
        }
        [TestMethod]
        public void InComp_ReturnsValue()
        {
            ConductComparisons cd = ConductComparisonTester();
            Assert.IsNotNull(cd.InCompNotOrig());

        }
        [TestMethod]
        public void InOrig_ReturnsValue()
        {
            ConductComparisons cd = ConductComparisonTester();
            Assert.IsNotNull(cd.InSource());

        }
        [TestMethod]
        public void InCompNotOrig_ReturnsValue()
        {
            ConductComparisons cd = ConductComparisonTester();
            Assert.IsNotNull(cd.InCompNotOrig());

        }
        [TestMethod]
        public void InOrigNotComp_ReturnsValue()
        {
            ConductComparisons cd = ConductComparisonTester();
            Assert.IsNotNull(cd.InOrigNotComp());

        }
        #endregion
    }
}