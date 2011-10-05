using System;

namespace DynamicScript.Testing
{
    using CategoryAttribute = NUnit.Framework.CategoryAttribute;
    using Resources = Properties.Resources;

     [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    sealed class SyntaxTestAttribute: CategoryAttribute
    {
         public SyntaxTestAttribute()
             : base(Resources.SyntaxCategory)
         {
         }
    }
}
