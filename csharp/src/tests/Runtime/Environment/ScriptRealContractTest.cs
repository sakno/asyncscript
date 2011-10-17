using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [SemanticTest]
    [TestClass(typeof(ScriptRealContract))]
    sealed class ScriptRealContractTest: SemanticTestBase
    {
        [Test(Description = "REAL contract hosting using C#/DLR.")]
        public void DlrInterop()
        {
            var real = Run("return real;");
            Assert.AreEqual(ScriptRealContract.Instance, real);
        }

        [Test(Description="NAN slot access.")]
        public void NanSlotTest()
        {
            double nan = Run("return real.nan;");
            Assert.AreEqual(double.NaN, nan);
        }

        [Test(Description = "MAX slot access.")]
        public void MaxSlotTest()
        {
            double max = Run("return real.max;");
            Assert.AreEqual(double.MaxValue, max);
        }

        [Test(Description = "MIN slot access.")]
        public void MinSlotTest()
        {
            double min = Run("return real.min;");
            Assert.AreEqual(double.MinValue, min);
        }

        [Test(Description="EPSILON slot access.")]
        public void EpsilonSlotTest()
        {
            double epsilon = Run("return real.epsilon;");
            Assert.AreEqual(double.Epsilon, epsilon);
        }

        [Test(Description = "ABS action invocation.")]
        public void AbsActionTest()
        {
            double abs = Run("return real.abs(-2);");
            Assert.AreEqual(2.0, abs);
        }

        [Test(Description = "SUM action test.")]
        public void SumActionTest()
        {
            double result = Run("return real.sum([1.0, 3, 5]);");
            Assert.AreEqual(9.0, result);
        }

        [Test(Description = "REM action test.")]
        public void RemActionTest()
        {
            double result = Run("return real.rem([5.0, 3, 1]);");
            Assert.AreEqual(1.0, result);
        }

        [Test(Description="ISINTERNED action test.")]
        public void IsInternedTest()
        {
            bool result = Run("return real.isInterned(^5.0);");
            Assert.IsTrue(result);
        }

        [Test(Description="Positive infinity test.")]
        public void PositiveInfinityTest()
        {
            double pinf = Run("return real.pinf;");
            Assert.AreEqual(double.PositiveInfinity, pinf);
        }

        [Test(Description="Negative infinity test.")]
        public void NegativeInfinityTest()
        {
            double ninf = Run("return real.ninf;");
            Assert.AreEqual(double.NegativeInfinity, ninf);
        }
    }
}
