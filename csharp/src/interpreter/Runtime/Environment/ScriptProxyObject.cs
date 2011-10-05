using System;
using System.Dynamic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Expression = System.Linq.Expressions.Expression;
    using BindingRestrictions = System.Dynamic.BindingRestrictions;

    /// <summary>
    /// Represents proxy object.
    /// </summary>
    
    [ComVisible(false)]
    [Serializable]
    public abstract class ScriptProxyObject: ScriptObject, IScriptProxyObject, ISerializable
    {
        private const string RealObjectHolder = "RealObject";
        private IScriptObject m_realObj;

        /// <summary>
        /// Deserializes proxy object.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ScriptProxyObject(SerializationInfo info, StreamingContext context)
        {
            m_realObj = (IScriptObject)info.GetValue(RealObjectHolder, typeof(IScriptObject));
        }

        /// <summary>
        /// Initializes a new proxy object.
        /// </summary>
        protected ScriptProxyObject()
        {
        }

        /// <summary>
        /// Applies an operation to the end of the unwrapper chain.
        /// </summary>
        /// <param name="operation">An operation to be applied.</param>
        /// <returns><see langword="true"/> if operation can be applied; otherwise, <see langword="false"/>.</returns>
        public abstract bool Apply(Func<IScriptObject, IScriptObject> operation);

        /// <summary>
        /// Unwraps object and applies operation chain constructed by calling <see cref="Apply"/> method.
        /// </summary>
        /// <returns>An unwrapped DynamicScript object.</returns>
        protected abstract IScriptObject UnwrapCore();

        /// <summary>
        /// Gets slot by its name.
        /// </summary>
        /// <param name="slotName">The name of the slot to get.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The runtime slot obtained by its name.</returns>
        public sealed override IRuntimeSlot this[string slotName, InterpreterState state]
        {
            get
            {
                var realObj = Unwrap();
                return realObj != null ? realObj[slotName, state] : null;
            }
        }

        /// <summary>
        /// Unwraps the real object.
        /// </summary>
        /// <returns>The real object.</returns>
        public IScriptObject Unwrap()
        {
            if (ShouldBeCached && m_realObj == null)
                m_realObj = UnwrapCore();
            return m_realObj ?? UnwrapCore();
        }

        /// <summary>
        /// Gets a value indicating that the real object should be cached.
        /// </summary>
        protected virtual bool ShouldBeCached
        {
            get { return true; }
        }

        /// <summary>
        /// Gets contract binding for the real object.
        /// </summary>
        /// <returns>The contract binding for the real object.</returns>
        public override IScriptContract GetContractBinding()
        {
            var value = UnwrapCore();
            return value.GetContractBinding();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(RealObjectHolder, Unwrap());
        }
    }
}
