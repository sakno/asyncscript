using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [TestFixture(Description = "Various tests for LEAVE/CONTINUE/RETURN statements.")]
    [SemanticTest]
    sealed class ControlFlowTests : SemanticTestBase
    {
        [Test(Description = "Test for valid loop breaking.")]
        public void LoopBreak()
        {
            IScriptArray r = Run("return for var i in [1, 2, 3] do if i == 2 then {leave;} else i;");
            Assert.AreEqual(1L, r.GetLength(0));
        }

        [Test(Description = "Leave root scope.")]
        public void BreakRootScope()
        {
            var r = Run("leave 10;");
            Assert.AreEqual(new ScriptInteger(10), r);
        }

        [Test(Description = "Return from root scope.")]
        public void ReturnFromRoot()
        {
            string r = Run("return 'hello';");
            Assert.AreEqual("hello", r);
        }

        [Test(Description = "Continue in root scope.")]
        public void ContinueInRootScope()
        {
            var s = Run(@"
var i;
i ?= 0;
i += 1;
if i == 2 then {return i;};
continue;
");
            Assert.AreEqual(new ScriptInteger(2), s);
        }

        [Test(Description="Continue in complex expression.")]
        public void ContinueInComplexExpression()
        {
            var s = Run(@"
var j = {
var i;
i ?= 0;
i += 1;
if i == 2 then {return i;};
continue;
};
");
            Assert.AreEqual(new ScriptInteger(2), s);
        }

        [Test(Description="Unattended return from const init expr.")]
        [ExpectedException(typeof(DynamicScriptException))]
        public void ReturnFromConstantExpr()
        {
             Run(@"
const a = 
{
  var a = 3;
  return 10;
};
leave a;
");

        }
    }
}
