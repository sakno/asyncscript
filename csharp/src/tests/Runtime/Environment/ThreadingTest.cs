﻿using System;
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
            var r = Run("var a = fork {threading.sleep(1000); leave 2;}; await a while threading.sleep(2000); return a;");
            Assert.AreEqual(new ScriptInteger(2), r);
            r = Run("var a = fork{threading.sleep(1000); leave 10.2;}; await a; return a;");
            Assert.AreEqual(new ScriptReal(10.2), r);
        }

        [Test(Description="Async lambda test.")]
        public void AsyncTest()
        {
            var r = Run("const a = @i, g, h -> async real: i + g + h; const r = a(1, 2.3, 4); await r; return r.result;");
            Assert.AreEqual(new ScriptReal(7.3), r);
        }
    }
}