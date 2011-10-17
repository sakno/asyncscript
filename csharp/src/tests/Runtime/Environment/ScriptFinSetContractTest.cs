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

        [Test(Description="Create finset.")]
        public void CreateFinsetTest()
        {
            var set = Run("return {{a = 10, b = 20}} to finset;");
            Assert.IsInstanceOf<IScriptSet>(set);
        }

        [Test(Description="Determines whether the any finite set is subset of FINSET contract.")]
        public void FinsetTypeTest()
        {
            var set = Run("const s = {{a = 10, b = 20}} to finset; return s is finset;");
            Assert.IsTrue(set);
        }

        [Test(Description = "Set member test.")]
        public void IsOperatorTest()
        {
            var set = Run("const s = {{a = 10, b = 20}} to finset; return 10 is s;");
            Assert.IsTrue(set);
        }
    }
}
