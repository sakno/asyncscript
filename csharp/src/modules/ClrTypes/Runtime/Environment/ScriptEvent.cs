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
                foreach (var other in ei.GetOtherMethods(false))
                    if (other != null)
                        AddConstant(other.Name, new ScriptMethod(other, @this));
                var method = ei.GetAddMethod(false);
                if (method != null)
                    AddConstant("subscribe", new ScriptMethod(method, @this));
                method = ei.GetRemoveMethod(false);
                if (method != null)
                    AddConstant("unsubscribe", new ScriptMethod(method, @this));
                method = ei.GetRaiseMethod(false);
                if (method != null)
                    AddConstant("raise", new ScriptMethod(method, @this));
            }
        }
        #endregion

        public ScriptEvent(EventInfo ei, INativeObject @this = null)
            : base(new Slots(ei, @this))
        {
        }
    }
}
