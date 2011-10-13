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
    }
}
