using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSVBreakdownConsolidator;
using System.IO;
using System;

namespace CSVBreakdownConsolidatorTests
{
    [TestClass]
    public sealed class ConsolidateTests
    {
        [TestMethod]
        public void RunTests()
        {
            char slash = Path.DirectorySeparatorChar;

            // Retrieve the test csv content.
            string[] mockOffice1 = File.ReadAllLines(
                $"InputSets{slash}InputCSVs{slash}mock_office_1.csv");
            string[] mockOffice2 = File.ReadAllLines(
                $"InputSets{slash}InputCSVs{slash}mock_office_2.csv");
            string mockOutput1 = File.ReadAllText(
                $"InputSets{slash}expected_output.csv");

            string output = CSVConsolidator.Consolidate(
                new string[][] { mockOffice1, mockOffice2 });

            // Check to see if the consolidator came up with
            // what we expected it to.
            Assert.AreEqual(mockOutput1, output);


        }
    }
}
