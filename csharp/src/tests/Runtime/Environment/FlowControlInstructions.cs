using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    using CodeAnalysisException = Compiler.CodeAnalysisException;

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
        [ExpectedException(typeof(CodeAnalysisException))]
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
        [ExpectedException(typeof(CodeAnalysisException))]
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
        public void CaseofTest()
        {
            var r = Run(@"
var i = 10;
return caseof i 
    if 1, 2 then 'one or two'
    if 10 then 'ten'
    else 'unknown';
");
            Assert.AreEqual("ten", (string)r);
            r = Run(@"
var s = '10';
return caseof s
    if '10', '11' then 10
    if '12' then 12
    else 0;
");
            Assert.AreEqual(new ScriptInteger(10), r);
            r = Run(@"
var s = {{a = 10}};
return caseof s
    if {{a = 10}} then 'ten'
    if '1' then 'one'
    else 'unknown';
");
            Assert.AreEqual("ten", (string)r);
        }

        [Test(Description="Caseof instruction with custom comparer.")]
        public void CaseofWithCustomComparerTest()
        {
            string r = Run(@"
var s = {{a = 10}};
return caseof s -> @src, value -> boolean: value == '1'
    if {{a = 10}} then 'ten'
    if '1' then 'one'
    else 'unknown';
");
            Assert.AreEqual("one", r);
        }

        [Test(Description = "Unattended return from FINALLY block.")]
        [ExpectedException(typeof(CodeAnalysisException))]
        public void ReturnFromFinally()
        {
            Run(@"
var a = @void -> integer: try 
{
    fault 2;
}
finally
{
    return 10;
};
return a();
");
        }

        [Test(Description="Try-else-finally test.")]
        public void TryElseFinallyTest()
        {
            var r = Run(@"
var a = @void -> integer: try 
{
    fault 2;
}
else()
{
    return 20;
}
finally
{
    leave 10;
};
return a();
");
            Assert.AreEqual(new ScriptInteger(20), r);
        }

        [Test(Description = "Try-else test.")]
        [ExpectedException(typeof(ScriptFault))]
        public void TryElseTestWithFailure()
        {
             Run(@"
return try
{
    fault 2;
}else(var s: string) s;
");
        }

        [Test(Description = "Try-else test.")]
        public void TryElseTest()
        {
            var r = Run(@"
return try
{
    fault 2;
} else(var s: string) s else(var i: integer) i;
");
            Assert.AreEqual(new ScriptInteger(2), r);
        }

        [Test(Description="Try-else-finally with continue in trap.")]
        public void TryElseFinallyWithContinue()
        {
            var r = Run(@"
var a = @i: integer -> integer: try if i > 0 then {fault 2;} else 3
else()
{
    continue 0;
}
finally
{
    leave 10;
};
return a(1);
");
            Assert.AreEqual(new ScriptInteger(3), r);
        }

        [Test(Description="Continue from FINALLY block.")]
        [ExpectedException(typeof(CodeAnalysisException))]
        public void ContinueFromFinally()
        {
            Run(@"
var a = @i: integer -> integer: try if i > 0 then {fault 2;} else 3
finally
{
    continue 10;
};
return a(1);
");
        }
    }
}
