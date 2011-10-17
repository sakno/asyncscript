using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [SemanticTest]
    [TestClass(typeof(ScriptFinSetContract))]
    sealed class ScriptFinSetContractTest: SemanticTestBase
    {
        [Test(Description = "FINSET contract hosting using C#/DLR.")]
        public void DlrInterop()
        {
            var contract = Run("return finset;");
            Assert.AreSame(contract, ScriptFinSetContract.Instance);
        }
    }
}
