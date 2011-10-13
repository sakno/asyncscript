using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [SemanticTest]
    [TestClass(typeof(ScriptString))]
    sealed class ScriptStringTest: SemanticTestBase
    {
        [Test(Description="Concatenation operator.")]
        public void ConcatenationTest()
        {
            string result = Run("return 'a'+'b'+'c';");
            Assert.IsTrue(result.Equals("abc", StringComparison.Ordinal));
        }

        [Test(Description="Substring replacement operator.")]
        public void ReplacementTest()
        {
            string result = Run("return 'abcab'-'ab';");
            Assert.IsTrue(result.Equals("c", StringComparison.Ordinal));
        }

        [Test(Description="String equality test.")]
        public void EqualityTest()
        {
            bool result = Run("return 'abc' == 'abc';");
            Assert.IsTrue(result);
        }

        [Test(Description = "Repeat operator test.")]
        public void RepeatTest()
        {
            string result = Run("return 'abc'*3;");
            Assert.IsTrue(result.Equals("abcabcabc", StringComparison.Ordinal));
        }

        [Test(Description = "Divide operator test.")]
        public void DivideTest()
        {
            Array result = Run("return 'abc' / 2;");
            Assert.AreEqual(2L, result.GetLength(0));
            Assert.AreEqual(new ScriptString("ab"), result.GetValue(0));
            Assert.AreEqual(new ScriptString("c"), result.GetValue(1));
        }

        [Test(Description = "String comparison test.")]
        public void ComparisonTest()
        {
            bool result = Run("return 'abc' > 'ab';");
            Assert.IsTrue(result);
            result = Run("return 'abc' < 'ab';");
            Assert.IsFalse(result);
        }

        [Test(Description = "IN operator test.")]
        public void ContainsTest()
        {
            bool result = Run("return 'ab' in 'abc';");
            Assert.IsTrue(result);
        }
    }
}
