using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSVBreakdownConsolidator;

namespace CSVBreakdownConsolidatorTests
{
    // Test assumes USD.
    [TestClass]
    public sealed class TryParseValueTests
    {
        // TODO these tests should be way more rigorous.
        [TestMethod]
        public void RunTests()
        {
            double parsed;

            // Acceptable value formats that should return true:
            Assert.IsTrue(CSVConsolidator.TryParseValue("", out parsed));
            Assert.AreEqual(parsed, 0.0);
            Assert.IsTrue(CSVConsolidator.TryParseValue("0", out parsed));
            Assert.AreEqual(parsed, 0.0);
            Assert.IsTrue(CSVConsolidator.TryParseValue("1", out parsed));
            Assert.AreEqual(parsed, 1.0);
            Assert.IsTrue(CSVConsolidator.TryParseValue("10", out parsed));
            Assert.AreEqual(parsed, 10.0);
            Assert.IsTrue(CSVConsolidator.TryParseValue("100", out parsed));
            Assert.AreEqual(parsed, 100.0);
            Assert.IsTrue(CSVConsolidator.TryParseValue("1000", out parsed));
            Assert.AreEqual(parsed, 1000.0);
            Assert.IsTrue(CSVConsolidator.TryParseValue("1,000", out parsed));
            Assert.AreEqual(parsed, 1000.0);

            // Unacceptable value formats that should return false:
            Assert.IsFalse(CSVConsolidator.TryParseValue("1,ooo", out parsed));
            Assert.IsFalse(CSVConsolidator.TryParseValue("1,2,99", out parsed));
            Assert.IsFalse(CSVConsolidator.TryParseValue("4,49,1", out parsed));
            Assert.IsFalse(CSVConsolidator.TryParseValue("28.01.29", out parsed));
            Assert.IsFalse(CSVConsolidator.TryParseValue(".14.01", out parsed));
        }
    }
}
