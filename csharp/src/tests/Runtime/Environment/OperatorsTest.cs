using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [SemanticTest]
    [TestFixture(Description="Miscellanious operators.")]
    sealed class OperatorsTest: SemanticTestBase
    {
        [Test(Description = "IS operator.")]
        public void IsOperatorTest()
        {
            var r = Run("return integer is type;");
            Assert.IsTrue(r);
            r = Run("return real is object;");
            Assert.IsTrue(r);
            r = Run("return 2 is object;");
            Assert.IsTrue(r);
            r = Run("return 'a' is string;");
            Assert.IsTrue(r);
            r = Run("return void is void;");
            Assert.IsTrue(r);
        }

        [Test(Description="IN operator.")]
        public void InOperatorTest()
        {
            var r = Run("return 1 in [1,2];");
            Assert.IsTrue(r);
        }
    }
}
