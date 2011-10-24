using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [TestClass(typeof(ScriptIterator))]
    sealed class LoopTests: SemanticTestBase
    {
        [Test(Description="Loop's yield test.")]
        public void SelectionTest()
        {
            IScriptArray r = Run("return for var i in [1, 2, 3, 4, 5] do if i % 2 == 0 then i;");
            Assert.AreEqual(2L, r.GetLength(0));
            Assert.AreEqual(new ScriptInteger(2), r[new[] { 0L }, InterpreterState.Initial]);
            Assert.AreEqual(new ScriptInteger(4), r[new[] { 1L }, InterpreterState.Initial]);
        }
    }
}
