using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [SemanticTest]
    [TestClass(typeof(ScriptCompositeContract))]
    sealed class ScriptScriptCompositeContractTest: SemanticTestBase
    {
        [Test(Description="Two empty composite contract should be equal by value and by reference.")]
        public void EmptyContractEquality()
        {
            bool equal = Run("return ${} == ${};");
            Assert.IsTrue(equal);
            equal = Run("return ${} === ${};");
            Assert.IsTrue(equal);
        }
    }
}
