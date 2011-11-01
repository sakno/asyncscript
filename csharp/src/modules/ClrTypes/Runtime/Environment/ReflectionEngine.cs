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

        public static ContractRelationshipType GetRelationship(Type source, Type destination)
        {
            if (Equals(source, destination))
                return ContractRelationshipType.TheSame;
            else if (source.IsAssignableFrom(destination))
                return ContractRelationshipType.Superset;
            else if (source.IsAssignableFrom(destination))
                return ContractRelationshipType.Subset;
            else return ContractRelationshipType.None;
        }
    }
}
