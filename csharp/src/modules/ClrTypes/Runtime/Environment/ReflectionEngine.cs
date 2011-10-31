using System;
using System.Collections.Generic;
using System.Reflection;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    static class ReflectionEngine
    {
        public static ICollection<string> GetMemberNames(Type t, BindingFlags flags)
        {
            return Array.ConvertAll(t.GetMembers(flags), m => m.Name);
        }
    }
}
