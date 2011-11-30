using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    using Thread = System.Threading.Thread;

    [SemanticTest]
    [TestClass(typeof(ScriptStringContract))]
    sealed class ScriptStringContractTest: SemanticTestBase
    {
        [Test(Description = "EMPTY slot test.")]
        public void EmptySlotTest()
        {
            string result = Run("return string.empty;");
            Assert.IsTrue(string.Empty.Equals(result, StringComparison.Ordinal));
        }

        [Test(Description = "CONCAT function test.")]
        public void ConcatActionTest()
        {
            string result = Run("return string.concat([1, 'abc']);");
            Assert.IsTrue(result.Equals("1abc", StringComparison.Ordinal));
        }

        [Test(Description="LANGUAGE slot test.")]
        public void LanguageSlotTest()
        {
            string result = Run("return string.language;");
            Assert.IsTrue(Thread.CurrentThread.CurrentCulture.IetfLanguageTag.Equals(result, StringComparison.OrdinalIgnoreCase));
        }

        [Test(Description = "EQU function test.")]
        public void EqualityTest()
        {
            bool result = Run("return string.equ('a', 'a', void);");
            Assert.IsTrue(result);
        }

        [Test(Description="LENGTH function test.")]
        public void LengthTest()
        {
            long len = Run("return string.length('abc');");
            Assert.AreEqual(3L, len);
        }

        [Test(Description="INSERT function test.")]
        public void InsertTest()
        {
            string len = Run("return string.insert('abc', 0, '123');");
            Assert.AreEqual(len, "123abc");
        }
    }
}
