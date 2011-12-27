using System;
using NUnit.Framework;
using DynamicScript.Testing;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment
{
    [TestClass(typeof(ScriptFunctionContract))]
    [SemanticTest]
    sealed class ScriptActionContractTest: SemanticTestBase
    {
        [Test(Description="Action signature reflection.")]
        public void ReflectionTest()
        {
            var r = Run(@"
const a = @i: real -> real: i + 10;
return $a;
");
            Assert.IsTrue(Equals(new ScriptFunctionContract(new[] { new ScriptFunctionContract.Parameter("i", ScriptRealContract.Instance) }, ScriptRealContract.Instance), r));
        }

        [Test(Description = "Return type reflection.")]
        public void ReturnTypeReflectionTest()
        {
            var r = Run(@"
const a = @i -> real;
return a.ret;
");
            Assert.AreSame(ScriptRealContract.Instance, r);
        }

        [Test(Description="Parameter type reflection.")]
        public void ParameterTypeReflectionTest()
        {
            var r = Run(@"
const a = @i -> real;
return a['i'];
");
            Assert.AreSame(ScriptSuperContract.Instance, r);
        }

        [Test(Description="Remove parameter from the signature.")]
        public void ParameterRemovingTest()
        {
            var r = Run(@"
const a = @i: real -> real;
return a % 'i';
");
            Assert.IsTrue(Equals(new ScriptFunctionContract(new ScriptFunctionContract.Parameter[0], ScriptRealContract.Instance), r));
        }
    }
}
