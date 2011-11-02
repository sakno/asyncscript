using System;
using NUnit.Framework;
using DynamicScript.Testing;
using DynamicScript.Runtime;
using DynamicScript.Runtime.Environment;

namespace DynamicScript.Modules.ClrTypes
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using SemanticTestBase = DynamicScript.Runtime.SemanticTestBase;

    [ComVisible(false)]
    [SemanticTest]
    [TestClass(typeof(Module))]
    sealed class ClrTypesTests : SemanticTestBase
    {
        [Test(Description="Load CLR-TYPES module.")]
        public void ModuleLoadingTest()
        {
            var r = Run("return use('clrtypes.dll');");
            Assert.IsFalse(ScriptObject.IsVoid(r));
        }

        [Test(Description="Make generic test.")]
        public void MakeGenericTest()
        {
            IScriptGeneric r = Run(@"
const clr = use('clrtypes.dll');
var Uri = clr.system.class('System.Uri');
var IConvertible = clr.mscorlib.class('System.IConvertible');
return clr.generic(Uri, [IConvertible], true);
");
            Assert.IsTrue(r.DefaultConstructor);
            Assert.AreEqual(typeof(Uri), r.BaseType.NativeType);
            Assert.AreEqual(1L, r.Interfaces.LongLength);
            Assert.AreEqual(typeof(IConvertible), r.Interfaces[0].NativeType);
        }

        [Test(Description = "Generating generic from test.")]
        public void AutoGenericTest()
        {
            IScriptGeneric r = Run("var clr = use('clrtypes.dll'); return $(clr.system.class('System.Uri'));");
            Assert.AreEqual(typeof(Uri), r.BaseType);
        }

        [Test(Description = "Simple method invocation.")]
        public void SimpleMethodInvocation()
        {
            string s = Run(@"
const clr = use('clrtypes.dll');
var uri = clr.system.class('System.Uri')('http://www.homepage.com');
return uri.ToString();
");
            Assert.AreEqual("http://www.homepage.com", s);
        }

        [Test(Description="Event subscribing.")]
        public void EventSubscribing()
        {
            bool r = Run(@"
const clr = use('clrtypes.dll');
var raised = false;
var component = clr.system.class('System.ComponentModel.Component')();
component.disposed.subscribe(@a, b -> void: raised = true);
component.dispose();
return raised;
");
            Assert.IsTrue(r);
        }
    }
}
