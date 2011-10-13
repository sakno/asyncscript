using System;

namespace DynamicScript.Testing
{
    using TestFixtureAttribute = NUnit.Framework.TestFixtureAttribute;
    using Resources = Properties.Resources;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    sealed class TestClassAttribute: TestFixtureAttribute
    {
        public TestClassAttribute(Type t)
            : base()
        {
            Description = string.Format(Resources.FmtTestClass, t.FullName);
        }
    }
}
