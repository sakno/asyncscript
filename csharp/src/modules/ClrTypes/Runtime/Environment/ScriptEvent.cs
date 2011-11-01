using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class ScriptEvent : ScriptCompositeObject
    {
        #region Nested Types
        [ComVisible(false)]
        private new sealed class Slots : ObjectSlotCollection
        {
            public Slots(EventInfo ei, INativeObject @this = null)
            {
                foreach (var other in Enumerable.Concat(new[] { ei.GetAddMethod(false), ei.GetRaiseMethod(false), ei.GetRemoveMethod(false) }, ei.GetOtherMethods(false)))
                    if (other != null)
                        AddConstant(other.Name, new ScriptMethod(other, @this));
            }
        }
        #endregion

        public ScriptEvent(EventInfo ei, INativeObject @this = null)
            : base(new Slots(ei))
        {
        }
    }
}
