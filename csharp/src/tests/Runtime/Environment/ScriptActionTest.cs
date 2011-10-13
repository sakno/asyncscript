using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [TestClass(typeof(ScriptRuntimeAction))]
    [SemanticTest]
    sealed class ScriptActionTest: SemanticTestBase
    {
        [Test(Description = "Test for action invocation.")]
        public void InvocationTest()
        {
            var r = Run(@"
const a = @i -> integer: i + 10;
return a(5);
");
            Assert.AreEqual(15, r);
        }

        [Test(Description = "Test for intersection operator applied to the action.")]
        public void UnificationTest()
        {
            long result = Run(@"
const a = @i: integer, b: integer, x: integer->integer: i + b + x; 
return(a & {{i=4, x=5}})(0);
");
            Assert.AreEqual(9, result);
        }
    }
}
