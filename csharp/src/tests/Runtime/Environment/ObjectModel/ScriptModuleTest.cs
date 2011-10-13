using System;
using NUnit.Framework;
using DynamicScript.Testing;
using System.Linq;

namespace DynamicScript.Runtime.Environment.ObjectModel
{
    [TestClass(typeof(ScriptModule))]
    sealed class ScriptModuleTest:SemanticTestBase
    {
        [Test(Description = "Test for ENUM action.")]
        public void EnumActionTest()
        {
            var set1 = Run("return enum([1, 2]);");
            var set2 = Run("return {a=1, b=2} to type;");
            Assert.AreEqual(set1, set2);
        }

        [Test(Description = "Test of SPLIT action.")]
        public void SplitActionTest()
        {
            IScriptObject slots = Run("return split({a=1, b = 2});");
            Assert.IsInstanceOf<ScriptCompositeObject>(slots);
            var objects = ScriptIterator.AsEnumerable(slots, InterpreterState.Current);
            Assert.AreEqual(2L, objects.LongCount());
        }

        [Test(Description="Regular expression test.")]
        public void RegexTest()
        {
            bool result = Run(@"var r = regex(); r.ignoreCase = true; return r.ismatch('http://www.contoso.com:8080/letters/readme.html', '^(?<proto>\w+)://[^/]+?(?<port>:\d+)?/');");
            Assert.IsTrue(result);
            Array match = Run(@"var r = regex(); r.ignoreCase = true; const m = r.match('http://www.contoso.com:8080/letters/readme.html', '^(?<proto>\w+)://[^/]+?(?<port>:\d+)?/'); return [m['proto'].value, m['port'].value];");
            Assert.AreEqual(2L, match.GetLength(0));
            Assert.AreEqual(new ScriptString("http"), match.GetValue(0));
            Assert.AreEqual(new ScriptString(":8080"), match.GetValue(1));
        }
    }
}
