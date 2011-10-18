using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [TestClass(typeof(ScriptContract))]
    sealed class ContractOperations : SemanticTestBase
    {
        [Test(Description = "Contract union test.")]
        public void UnionContact()
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
    }
}
