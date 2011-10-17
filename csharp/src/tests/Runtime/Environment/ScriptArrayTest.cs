using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [SemanticTest]
    [TestClass(typeof(ScriptArrayContract))]
    sealed class ScriptArrayTest: SemanticTestBase
    {
        [Test(Description = "IN operator test.")]
        public void ContainsTest()
        {
            bool result = Run("return 2 in [1, 2];");
            Assert.IsTrue(result);
        }

        [Test(Description = "Fill array elements")]
        public void ArrayFillTest()
        {
            IScriptArray r = Run("return [0, 1, 2];");
            Assert.AreEqual(3L, r.GetLength(0));
            Assert.AreSame(ScriptIntegerContract.Instance, r.GetContractBinding().ElementContract);
            for (var i = 0L; i < r.GetLength(0); i++)
                Assert.AreEqual(new ScriptInteger(i), r[new[] { i }, InterpreterState.Initial]);
        }

        [Test(Description="Array concatentation test.")]
        public void ConcatenationTest()
        {
            IScriptArray r = Run("return [0, 1, 2] + dimensional(3);");
            Assert.AreEqual(4L, r.GetLength(0));
            for (var i = 1L; i < r.GetLength(0); i++)
                Assert.AreEqual(new ScriptInteger(i), r[new[] { i }, InterpreterState.Initial]);
            r = Run("return [0, 1, 2] + [3, 4];");
            for (var i = 0L; i < r.GetLength(0); i++)
                Assert.AreEqual(new ScriptInteger(i), r[new[] { i }, InterpreterState.Initial]);
        }
    }
}
