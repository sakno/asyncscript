using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [TestFixture(Description = "Flow control instructions, such as IF, CONTINUE, CASEOF and etc.")]
    [SemanticTest]
    sealed class FlowControlInstructions : SemanticTestBase
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

        [Test(Description="Unattended return from slot initialization.")]
        [ExpectedException(typeof(DynamicScriptException))]
        public void ReturnInstructionInCompositeObject()
        {
            Run("var a = {{c = {var i; return i;} }};");
        }

        [Test(Description="Using complex expression to initialize object slot.")]
        public void LeaveScopeInCompositeObject()
        {
            var r = Run(@"var a = {{c = {var i = 0; leave i + 10;} }};
return a.c;
");
            Assert.AreEqual(new ScriptInteger(10), r);
        }

        [Test(Description = "Represents a simple CASEOF instruction usage.")]
        public void SimpleCaseof()
        {
            string r = Run(@"
var i = 10;
return caseof i 
    if 1, 2 then 'one or two'
    if 10 then 'ten'
    else 'unknown';
");
            Assert.AreEqual("ten", r);
        }
    }
}
