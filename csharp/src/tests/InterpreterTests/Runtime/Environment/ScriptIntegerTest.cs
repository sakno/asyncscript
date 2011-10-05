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
            long sum = Run("return 2+10;");
            Assert.AreEqual(12, sum);
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
            long div = Run("return 10/2.5;");
            Assert.AreEqual(4L, div);
        }
    }
}
