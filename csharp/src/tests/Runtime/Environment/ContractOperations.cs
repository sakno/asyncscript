using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [TestClass(typeof(ScriptContract))]
    sealed class ContractOperations : SemanticTestBase
    {
        [Test(Description = "Contract union test.")]
        public void SimpleUnionContact()
        {
            var r = Run("return boolean | object;");
            Assert.AreSame(ScriptSuperContract.Instance, r);
            r = Run("return string | object;");
            Assert.AreSame(ScriptSuperContract.Instance, r);
            r = Run("return finset | type;");
            Assert.AreSame(ScriptMetaContract.Instance, r);
            r = Run("return dimensional | integer[];");
            Assert.AreSame(ScriptDimensionalContract.Instance, r);
            r = Run("return integer | integer;");
            Assert.AreSame(ScriptIntegerContract.Instance, r);
            r = Run("return boolean | integer;");
            Assert.AreSame(ScriptIntegerContract.Instance, r);
            r = Run("return ${{a: integer}} | ${{a: integer, b: object}};");
            Assert.AreEqual(1L, ((ScriptCompositeContract)r).Slots.Count);
            r = Run("return (@a: integer -> void) | (@a -> integer);");
            Assert.IsTrue(Equals(Run("return @a: integer -> void;"), r));
        }

        [Test(Description="Assignment to the variable typed with union.")]
        public void UnionContractAssignment()
        {
            var r = Run(@"
var a: integer | ${{ }};
a = {{z = 42, g = 'a'}};
a = 42;
return a;
");
            Assert.AreEqual(new ScriptInteger(10), r);
        }

        [Test(Description="Complex set of operations under the contracts.")]
        public void IntersectionWithUnion()
        {
            var r = Run(@"
var a = boolean | string;
a |= boolean;
return a & boolean;
");
            Assert.AreSame(ScriptBooleanContract.Instance, r);
            r = Run("return integer & string;");
            Assert.IsTrue(ScriptObject.IsVoid(r));
        }

        [Test(Description="Tests for complementation operator.")]
        public void Complementation()
        {
            var r = Run("return !integer | string;");
            Assert.IsTrue(ScriptContract.IsComplementation(r));
            r = Run("return !void | integer;");
            Assert.AreSame(ScriptSuperContract.Instance, r);
            r = Run("return !object | string;");
            Assert.AreSame(ScriptStringContract.Instance, r);
        }
    }
}
