using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [TestClass(typeof(RuntimeSlot))]
    sealed class RuntimeSlotTest : SemanticTestBase
    {
        [Test(Description = "Erase variable value.")]
        [ExpectedException(typeof(UnassignedSlotReadingException))]
        public void VariableErasure()
        {
            Run("var a = 20.4342; a to void; return a;");
        }

        [Test(Description="Preventing constant erasure.")]
        public void PreventConstantErasure()
        {
            var r = Run("const a = 20.4342; a to void; return a;");
            Assert.AreEqual(new ScriptReal(20.4342), r);
        }
    }
}
