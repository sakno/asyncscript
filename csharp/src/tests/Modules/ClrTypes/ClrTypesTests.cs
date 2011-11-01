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
var clr = use('clrtypes.dll');
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
    }
}
