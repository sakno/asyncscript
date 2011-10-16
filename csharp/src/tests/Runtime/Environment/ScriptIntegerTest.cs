using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [TestClass(typeof(ScriptInteger))]
    [SemanticTest]
    sealed class ScriptIntegerTest: SemanticTestBase
    {
        [Test(Description = "Sum of two integers.")]
        public void SumTest()
        {
            IScriptObject sum = Run("return 2 + 10;");
            Assert.AreEqual(new ScriptInteger(12), sum);
            Assert.AreSame(ScriptIntegerContract.Instance, sum.GetContractBinding());
        }

        [Test(Description = "Sum of two integers in checked context.")]
        [ExpectedException(typeof(OverflowException))]
        public void CheckedSumTest()
        {
            Run("return checked integer.max + 10;");
        }

        [Test(Description="Sum of two integers in unchecked context.")]
        public void UncheckedSumTest()
        {
            long sum = Run("return unchecked integer.max + 10;");
            Assert.AreEqual(-9223372036854775799L, sum);
        }

        [Test(Description = "Subtraction of two integers.")]
        public void SubtractionTest()
        {
            long sum = Run("return 2 - 10;");
            Assert.AreEqual(-8L, sum);
        }

        [Test(Description="Subtraction of two integer in unchecked context.")]
        public void UncheckedSubtractionTest()
        {
            long sum = Run("return unchecked integer.min - 10;");
            Assert.AreEqual(9223372036854775798L, sum);
        }

        [Test(Description = "Subtraction of two integer in checked context.")]
        [ExpectedException(typeof(OverflowException))]
        public void CheckedSubtractionTest()
        {
            Run("return checked integer.min - 10;");
        }

        [Test(Description="Bit access.")]
        public void BitAccessTest()
        {
            bool bit = Run("return 1[0];");
            Assert.IsTrue(bit);
            bit = Run("return 1[1];");
            Assert.IsFalse(bit);
        }

        [Test(Description = "Division of integer on real.")]
        public void DivisionTest()
        {
            IScriptObject div = Run("return 10 / 2.5;");
            Assert.AreEqual(new ScriptReal(4L), div);
            Assert.AreSame(ScriptRealContract.Instance, div.GetContractBinding());
        }
    }
}
