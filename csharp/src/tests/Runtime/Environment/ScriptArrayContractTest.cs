using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [SemanticTest]
    [TestClass(typeof(ScriptArrayContract))]
    sealed class ScriptArrayContractTest: SemanticTestBase
    {
        [Test(Description = "Array contract hosting using C#/DLR.")]
        public void DlrInterop()
        {
            object arrayContract = Run("return integer[];");
            Assert.AreEqual(new ScriptArrayContract(ScriptIntegerContract.Instance), arrayContract);
            arrayContract = Run("return integer[,,];");
            Assert.AreEqual(new ScriptArrayContract(ScriptIntegerContract.Instance, 3), arrayContract);
        }

        [Test(Description="Relationship between different array contracts.")]
        public void RelationshipTest()
        {
            bool result = Run("return integer[] < object[];");
            Assert.IsTrue(result);
            result = Run("return integer[] > object[];");
            Assert.IsFalse(result);
            result = Run("return dimensional > string[];");
            Assert.IsTrue(result);
        }

        [Test(Description = "Array instantiation.")]
        public void ApplicationOperatorTest()
        {
            IScriptArray array = Run("return integer[](4);");
            Assert.AreEqual(4L, array.GetLength(0));
            for (var i = 0L; i < 4L; i++)
                Assert.AreEqual(ScriptInteger.Zero, array[new[] { i }, InterpreterState.Initial]);
        }
    }
}
