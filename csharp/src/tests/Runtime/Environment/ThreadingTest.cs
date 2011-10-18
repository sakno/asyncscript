using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [TestClass(typeof(ISynchronizable))]
    sealed class ThreadingTest: SemanticTestBase
    {
        [Test(Description="Fork/await test.")]
        public void ForkAwaitTest()
        {
            var r = Run("var a = fork {threading.sleep(1000); leave 2;}; return await a while threading.sleep(2000);");
            Assert.IsTrue(r);
        }
    }
}
