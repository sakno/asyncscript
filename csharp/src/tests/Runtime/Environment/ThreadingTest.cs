using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [TestClass(typeof(IScriptProxyObject))]
    sealed class ThreadingTest: SemanticTestBase
    {
        [Test(Description="Fork/await test.")]
        public void ForkAwaitTest()
        {
            var r = Run("var a = fork {threading.sleep(1000); leave 2;}; return threading.unwrap(a);");
            Assert.AreEqual(new ScriptInteger(2), r);
        }

        [Test(Description="Async lambda test.")]
        public void AsyncTest()
        {
            var r = Run("const a = @i, g, h -> async real: i + g + h; const r = a(1, 2.3, 4, void); return r(3000, void);");
            Assert.AreEqual(new ScriptReal(7.3), r);
        }

        [Test(Description="Lazy constant.")]
        public void LazyConstTest()
        {
            var r = Run("const g = 3; const a = fork g + 10; return threading.unwrap(a);");
            Assert.AreEqual(new ScriptInteger(13), r);
        }

        [Test(Description = "Future computation.")]
        public void FutureComputation()
        {
            var r = Run(@"
var s = fork {threading.sleep(4000); leave 2; };
var f = s + 10;
return threading.unwrap(f);
");
            Assert.AreEqual(new ScriptInteger(12), r);
        }
    }
}
