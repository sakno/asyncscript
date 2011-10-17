using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [TestClass(typeof(ScriptReal))]
    sealed class ScriptRealTest: SemanticTestBase
    {
        [Test(Description = "Sum operator.")]
        public void SumTest()
        {
            IScriptObject sum = Run("return 2 + 10.0;");
            Assert.AreEqual(new ScriptReal(12), sum);
            Assert.AreSame(ScriptRealContract.Instance, sum.GetContractBinding());
            sum = Run("return 13.2 + 5;");
            Assert.AreEqual(new ScriptReal(18.2), sum);
            sum = Run("return 19.53 + 9.90;");
            Assert.AreEqual(new ScriptReal(29.43), sum);
        }

        [Test(Description = "Subtraction of two integers.")]
        public void SubtractionTest()
        {
            IScriptObject sum = Run("return 2 - 10.0;");
            Assert.AreEqual(new ScriptReal(-8.0), sum);
            Assert.AreSame(ScriptRealContract.Instance, sum.GetContractBinding());
        }

        [Test(Description = "Division of real on real.")]
        public void DivisionTest()
        {
            IScriptObject div = Run("return 10.2 / 2;");
            Assert.AreEqual(new ScriptReal(5.1), div);
            Assert.AreSame(ScriptRealContract.Instance, div.GetContractBinding());
        }

        [Test(Description = "Multiply two reals.")]
        public void MulTest()
        {
            IScriptObject sum = Run("return 3.2 * 10;");
            Assert.AreEqual(new ScriptReal(32), sum);
            Assert.AreSame(ScriptRealContract.Instance, sum.GetContractBinding());
        }

        [Test(Description = "Modulo operator.")]
        public void ModuloTest()
        {
            double mod = Run("return 23.2 % 10;");
            Assert.AreEqual(23.2 % 10, mod);
        }

        [Test(Description = "Unary minus operator.")]
        public void UnaryMinusTest()
        {
            double um = Run("const a = 395.2; return -a;");
            Assert.AreEqual(-395.2, um);
        }

        [Test(Description = "Unary plus operator.")]
        public void UnaryPlusTest()
        {
            double um = Run("const a = 395.5; return +a;");
            Assert.AreEqual(395.5, um);
        }
    }
}
