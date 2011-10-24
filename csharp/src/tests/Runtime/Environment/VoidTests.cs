using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment
{
    [TestClass(typeof(ScriptVoid))]
    sealed class VoidTests: SemanticTestBase
    {
        [Test(Description = "Converts void to string.")]
        public void VoidToString()
        {
            string r = Run("return void to string;");
            Assert.IsEmpty(r);
        }

        [Test(Description="Converts void to integer.")]
        public void VoidToInteger()
        {
            long r = Run("return void to integer;");
            Assert.AreEqual(0L, r);
        }

        [Test(Description="Converts void to composite contract.")]
        public void VoidToComposite()
        {
            var r = Run("return void to ${{ }};");
            Assert.AreSame(ScriptVoid.Instance, r);
        }

        [Test(Description = "Converts void to boolean.")]
        public void VoidToBoolean()
        {
            bool r = Run("return void to boolean;");
            Assert.IsFalse(r);
        }

        [Test(Description="Converts boolean contract to void.")]
        [ExpectedException(typeof(UnsupportedOperationException))]
        public void BooleanContractToVoid()
        {
            Run("return boolean to void;");
        }

        [Test(Description="Converts boolean literal to void.")]
        public void BooleanToVoid()
        {
            bool r = Run("return true to void;");
            Assert.IsFalse(r);
        }

        [Test(Description="Converts integer to void.")]
        public void IntegerToVoid()
        {
            long r = Run("return 10 to void;");
            Assert.AreEqual(0L, r);
        }
    }
}
