using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [SemanticTest]
    [TestClass(typeof(ScriptIntegerContract))]
    sealed class ScriptIntegerContractTest: SemanticTestBase
    {
        [Test(Description = "INTEGER contract hosting using C#/DLR.")]
        public void DlrInterop()
        {
            object integer = Run("return integer;");
            Assert.AreEqual(integer, ScriptIntegerContract.Instance); 
        }

        [Test(Description="Implicit/explicit conversion from boolean to integer.")]
        public void FromBooleanTest()
        {
            long integer = Run("var v: integer; v = false; return v;");
            Assert.AreEqual(integer, 0L);
            integer = Run("var v: integer; v = true; return v;");
            Assert.AreEqual(integer, 1L);
            integer = Run("return true to integer;");
            Assert.AreEqual(integer, 1L);
        }

        [Test(Description="Explicit conversion from real to integer.")]
        public void FromRealTest()
        {
            long integer = Run("return 1.9 to integer;");
            Assert.AreEqual(integer, 1L);
        }

        [Test(Description = "EVEN action test.")]
        public void EvenActionTest()
        {
            bool even = Run("return integer.even(2);");
            Assert.IsTrue(even);
            even = Run("return integer.even(1);");
            Assert.IsFalse(even);
        }

        [Test(Description = "ODD action test.")]
        public void OddActionTest()
        {
            bool odd = Run("return integer.odd(2);");
            Assert.IsFalse(odd);
            odd = Run("return integer.odd(1);");
            Assert.IsTrue(odd);
        }

        [Test(Description = "SIZE slot test.")]
        public void SizeSlotTest()
        {
            long size = Run("return integer.size;");
            Assert.AreEqual(sizeof(long), size);
        }

        [Test(Description = "MAX slot test.")]
        public void MaxSlotTest()
        {
            long max = Run("return integer.max;");
            Assert.AreEqual(long.MaxValue, max);
        }

        [Test(Description = "MIN slot test.")]
        public void MinSlotTest()
        {
            long min = Run("return integer.min;");
            Assert.AreEqual(long.MinValue, min);
        }

        [Test(Description = "ABS action test.")]
        public void AbsActionTest()
        {
            long abs = Run("return integer.abs(-10);");
            Assert.AreEqual(10L, abs);
        }

        [Test(Description = "SUM action test.")]
        public void SumActionTest()
        {
            long result = Run("return integer.sum([1, 3, 5]);");
            Assert.AreEqual(9L, result);
        }

        [Test(Description = "REM action test.")]
        public void RemActionTest()
        {
            long result = Run("return integer.rem([5, 3, 1]);");
            Assert.AreEqual(1L, result);
        }

        [Test(Description = "ISINTERNED action test.")]
        public void IsInternedTest()
        {
            bool result = Run("return integer.isInterned(^5);");
            Assert.IsTrue(result);
        }
    }
}
