using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [TestClass(typeof(ScriptRuntimeAction))]
    [SemanticTest]
    sealed class ScriptActionTest: SemanticTestBase
    {
        [Test(Description = "Test for intersection operator applied to the action.")]
        public void UnificationTest()
        {
            long result = Run("var a=@i: integer, b: integer, x: integer->integer: i+b+x; return(a & {i=4, x=5})(0);");
            Assert.AreEqual(9, result);
        }
    }
}
