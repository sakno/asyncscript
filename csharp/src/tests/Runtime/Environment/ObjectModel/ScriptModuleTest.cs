using System;
using NUnit.Framework;
using DynamicScript.Testing;
using System.Linq;

namespace DynamicScript.Runtime.Environment.ObjectModel
{
    using SystemEnvironment = System.Environment;

    [TestClass(typeof(ScriptModule))]
    sealed class ScriptModuleTest:SemanticTestBase
    {
        [Test(Description = "Test for ENUM action.")]
        public void EnumActionTest()
        {
            var set1 = Run("return enum([1, 2]);");
            var set2 = Run("return {{a = 1, b = 2}} to finset;");
            Assert.AreEqual(set1, set2);
        }

        [Test(Description = "Test for SPLIT action.")]
        public void SplitActionTest()
        {
            IScriptObject slots = Run("return split({{a = 1, b = 2}});");
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
            Assert.IsTrue(Equals(new ScriptString("http"), match.GetValue(0)));
            Assert.IsTrue(Equals(new ScriptString(":8080"), match.GetValue(1)));
        }

        [Test(Description="Test for WEAKREF action.")]
        public void WeakRefTest()
        {
            bool r = Run(@"
var a = 10;
var s = weakref(a);
a to void;
gc.collect();
gc.wait();
return s.isalive;
");
            Assert.IsFalse(r);
        }

        [Test(Description="Test for NEWOBJ action.")]
        public void NewObjTest()
        {
            bool r = Run(@"var obj = newobj('a', real);
return $obj == ${{a: real}};
");
            Assert.IsTrue(r);
        }

        [Test(Description="Test for ARGS slot.")]
        public void ArgsTest()
        {
            IScriptArray args = Run("return args;", "a", "b");
            Assert.AreEqual(2L, args.GetLength(0));
        }

        [Test(Description="Test for EVAL action.")]
        public void EvalTest()
        {
            long r = Run("return eval('return 15;', void);");
            Assert.AreEqual(15L, r);
        }

        [Test(Description="Test for PARSE action.")]
        public void ParseTest()
        {
            var r = Run("return parse('true', boolean, void);");
            Assert.IsTrue((bool)r);
            r = Run("return parse('12', integer, void);");
            Assert.AreEqual(12L, (long)r);
            r = Run("return parse('12.34', real, void);");
            Assert.AreEqual(12.34, (double)r);
            r = Run("return parse('hello, world', object, void);");
            Assert.AreEqual("hello, world", (string)r);
        }

        [Test(Description="Test for WDIR slot.")]
        public void WorkingDirectoryTest()
        {
            var r = Run("return wdir;");
            Assert.AreEqual(SystemEnvironment.CurrentDirectory, (string)r);
        }

        [Test(Description="Test for READONLY action.")]
        [ExpectedException(typeof(ConstantCannotBeChangedException))]
        public void ReadOnlyTest()
        {
            Run("var s = readonly({{a= 10}}); s.a = 10;");
        }

        [Test(Description="Test for GETDATA and SETDATA actions.")]
        public void GetDataSetDataTest()
        {
            bool r = Run("setdata('store', true); return getdata('store');");
            Assert.IsTrue(r);
        }

        [Test(Description="Test for IMPORT function.")]
        public void ImportActionTest()
        {
            IScriptCompositeObject r = Run("var r = {{a = 10}}; import({{a = 20, b = '123'}}, r); return r;");
            Assert.AreEqual(2, r.Slots.Count);
            Assert.AreEqual(new ScriptInteger(20), r["a"].GetValue(InterpreterState.Current));
        }
    }
}
