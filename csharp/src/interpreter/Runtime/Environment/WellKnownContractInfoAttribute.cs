using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    sealed class WellKnownContractInfoAttribute : Attribute
    {
        public ScriptTypeCode TypeCode;

        public WellKnownContractInfoAttribute(ScriptTypeCode t)
        {
            TypeCode = t;
        }
    }
}
