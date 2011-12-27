using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [TestClass(typeof(ScriptInteger))]
    [SemanticTest]
    sealed class ScriptIntegerTest : SemanticTestBase
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

        [Test(Description = "Sum of two integers in unchecked context.")]
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

        [Test(Description = "Subtraction of two integer in unchecked context.")]
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

        [Test(Description = "Bit access.")]
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
            Assert.AreEqual(new ScriptReal(4.0), div);
            Assert.AreSame(ScriptRealContract.Instance, div.GetContractBinding());
        }

        [Test(Description = "Division of integer on integer.")]
        public void IntegerDivisionTest()
        {
            IScriptObject div = Run("return 10 / 3;");
            Assert.AreEqual(new ScriptInteger(3), div);
            Assert.AreSame(ScriptIntegerContract.Instance, div.GetContractBinding());
        }

        [Test(Description = "Multiply two integers.")]
        public void MulTest()
        {
            IScriptObject sum = Run("return 3 * 10;");
            Assert.AreEqual(new ScriptInteger(30), sum);
            Assert.AreSame(ScriptIntegerContract.Instance, sum.GetContractBinding());
        }

        [Test(Description = "Multiply two integers in unchecked context.")]
        public void UncheckedMulTest()
        {
            IScriptObject sum = Run("return unchecked integer.max * 10;");
            Assert.AreEqual(new ScriptInteger(-10), sum);
            Assert.AreSame(ScriptIntegerContract.Instance, sum.GetContractBinding());
        }

        [Test(Description = "Multiply two integers in checked context.")]
        [ExpectedException(typeof(OverflowException))]
        public void CheckedMulTest()
        {
            Run("return checked integer.max * 10;");
        }

        [Test(Description = "Modulo operator.")]
        public void ModuloTest()
        {
            long mod = Run("return 23 % 10;");
            Assert.AreEqual(3L, mod);
        }

        [Test(Description = "AND operator.")]
        public void AndTest()
        {
            long and = Run("return 23 & 56;");
            Assert.AreEqual(16L, and);
        }

        [Test(Description = "OR operator.")]
        public void OrTest()
        {
            long and = Run("return 23 | 56;");
            Assert.AreEqual(63L, and);
        }

        [Test(Description = "NEG operator.")]
        public void NotTest()
        {
            long neg = Run("const a = 15; return !a;");
            Assert.AreEqual(-16L, neg);
        }

        [Test(Description = "Exclusive OR operator.")]
        public void ExclusiveOrTest()
        {
            long xor = Run("const a = 15; const b = 19; return a ^ b;");
            Assert.AreEqual(28L, xor);
        }

        [Test(Description = "Unary minus operator.")]
        public void UnaryMinusTest()
        {
            long um = Run("const a = 395; return -a;");
            Assert.AreEqual(-395L, um);
        }

        [Test(Description="Unary plus operator.")]
        public void UnaryPlusTest()
        {
            long um = Run("const a = 395; return +a;");
            Assert.AreEqual(395L, um);
        }

        [Test(Description = "Convert from integer to real.")]
        public void ConvertToRealTest()
        {
            IScriptObject d = Run("const a = 20; return a to real;");
            Assert.AreEqual(new ScriptReal(20), d);
            Assert.AreSame(ScriptRealContract.Instance, d.GetContractBinding());
        }

        [Test(Description="Squary operator.")]
        public void SquareTest()
        {
            IScriptObject s = Run("var a = 10; return **a;");
            Assert.AreEqual(new ScriptInteger(100), s);
            Assert.AreSame(ScriptIntegerContract.Instance, s.GetContractBinding());
        }
    }
}
