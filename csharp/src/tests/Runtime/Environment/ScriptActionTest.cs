using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [TestClass(typeof(ScriptRuntimeAction))]
    [SemanticTest]
    sealed class ScriptActionTest: SemanticTestBase
    {
        [Test(Description = "Unsupported operations for the lambdas.")]
        [ExpectedException(typeof(UnsupportedOperationException))]
        public void NotSupportedOperation()
        {
            Run(@"
const a = @i: integer -> integer: i + 10;
const b = @i: integer -> string: i to string;
var c = a % b;
");
        }

        [Test(Description="Action composition test.")]
        public void CompositionTest()
        {
            //Simple composition
            var r = Run(@"
const a = @i: integer -> integer: i + 10;
const b = @i: integer -> string: i to string;
var c = a * b;
return c(10);
");
            Assert.IsTrue(Equals(new ScriptString("20"), r));
            //Partial composition with two parameters in the first lambda
            r = Run(@"
const a = @x: integer, y: integer -> integer: x + y;
const b = @i: integer -> string: i to string;
var c = a * b;
return c(10, 15);
");
            Assert.IsTrue(Equals(new ScriptString("25"), r));
            //Partial composition with two paremeters in the second lambda
            r = Run(@"
const a = @x: integer, y: integer -> integer: x + y;
const b = @i: integer, s: string -> string: i to string + s;
var c = a * b;
return c(10, 15, 'ab');
");
            Assert.IsTrue(Equals(new ScriptString("25ab"), r));
        }

        [Test(Description="Unsupported composition test.")]
        public void UnsupportedCompositionTest()
        {
            var r = Run(@"
const a = @i: integer -> string: i to string;
const b = @i: integer -> string: i to string;
return unchecked a * b;
");
            Assert.AreSame(ScriptVoid.Instance, r);
        }

        [Test(Description = "Runtime overloading.")]
        public void CombinationTest()
        {
            var r = Run(@"
const a = @i: integer -> integer: i + 10;
const b = @s: string -> string: s + 'abc';
var c = a + b;
return c(10);
");
            Assert.AreEqual(new ScriptInteger(20), r);
            r = Run(@"
const a = @i: integer -> integer: i + 10;
const b = @s: string -> string: s + 'abc';
var c = a + b;
return c('abc');
");
            Assert.IsTrue(Equals(new ScriptString("abcabc"), r));   //do not use Assert.AreEqual because this method throws StackOverflow
        }

        [Test(Description="Action parameter reflection.")]
        public void GetParameterTypeTest()
        {
            var r = Run(@"
var a = @i: integer -> integer: i + 10;
return a :: i;
");
            Assert.AreSame(ScriptIntegerContract.Instance, r);
        }

        [Test(Description = "Obtaining the return type of the ")]
        public void AdjustAndThisReflectionTest()
        {
            var r = Run(@"
var a = @i -> integer: i + 10;
a = adjust(a, 2);
return a.owner;
");
            Assert.AreEqual(new ScriptInteger(2), r);
        }

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
        public void CurryingTest()
        {
            long result = Run(@"
const a = @i: integer, b: integer, x: integer->integer: i + b + x; 
return(a & {{i=4, x=5}})(0);
");
            Assert.AreEqual(9, result);
        }

        [Test(Description="Covariance test.")]
        public void CovarianceTest()
        {
            IScriptObject r = Run(@"
var a: @z: integer -> object;
a = @z -> integer: z to integer;
return a(10);
");
            Assert.IsFalse(ScriptObject.IsVoid(r));
        }

        [Test(Description = "Tag recursion.")]
        public void TagRecursionTest()
        {
            var r = Run(@"
const f = @i: integer -> integer: {
  var a;
  a ?= 0;
  a = a + 1;
  if i == 0 then {return a;};
  continue i - 1;
};
return f(2);
");
            Assert.AreEqual(new ScriptInteger(3), r);
            r = Run(@"
const f = @i: integer -> integer: {
  var a = 0;
  a = a + 1;
  if i == 0 then {return a;};
  continue i - 1;
};
return f(2);");
            Assert.AreEqual(new ScriptInteger(1), r);
        }

        [Test(Description="Fast signature description test.")]
        public void FastSignature()
        {
            long r = Run("const func = @2 -> integer: !!0 + !!1; return func(1, 2);");
            Assert.AreEqual(3L, r);
        }

        [Test(Description="Overloading test.")]
        public void OverloadingTest()
        {
            long r = Run("const c = (@0 ->void: void) + (@1 -> void: void) + (@2 -> void: void); var i = 0; for var g in c do i++; return i;");
            Assert.AreEqual(3L, r);
        }
    }
}
