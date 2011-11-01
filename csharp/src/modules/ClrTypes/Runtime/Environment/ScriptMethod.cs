using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class ScriptMethod : ScriptActionBase, IScriptMethod
    {
        /// <summary>
        /// Represents method metadata.
        /// </summary>
        public readonly MethodInfo Method;
        private Type[] m_parameters;

        public ScriptMethod(MethodInfo mi, INativeObject @this = null)
            : base(GetContractBinding(mi), @this)
        {
            Method = mi;
            m_parameters = null;
        }

        /// <summary>
        /// Gets method owner.
        /// </summary>
        public new object This
        {
            get { return IsVoid(base.This) ? null : ((INativeObject)base.This).Instance; }
        }

        private static ScriptActionContract.Parameter CreateParameter(ParameterInfo pi)
        {
            return new ScriptActionContract.Parameter(pi.Name, ScriptClass.GetContractBinding(pi.ParameterType));
        }

        private static ScriptActionContract GetContractBinding(ParameterInfo[] parameters, Type returnType)
        {
            return new ScriptActionContract(Enumerable.Select(parameters, CreateParameter), ScriptClass.GetContractBinding(returnType));
        }

        private static ScriptActionContract.Parameter CreateParameter(Type genericParameter)
        {
            return new ScriptActionContract.Parameter(genericParameter.Name, new ScriptGeneric(genericParameter, null, false));
        }

        private static ScriptActionContract GetContractBinding(Type[] genericParameters)
        {
            return new ScriptActionContract(Enumerable.Select(genericParameters, CreateParameter), ScriptSuperContract.Instance);
        }

        public static ScriptActionContract GetContractBinding(MethodInfo mi)
        {
            return mi.IsGenericMethodDefinition ?
                GetContractBinding(mi.GetGenericArguments()) :
                GetContractBinding(mi.GetParameters(), mi.ReturnType);
        }

        public static IScriptObject Overload(MethodInfo[] mi, INativeObject @this, InterpreterState state)
        {
            var method = new ScriptMethod(mi[0], @this);
            return mi.LongLength > 2L ? method.Combine(new ScriptMethod(mi[1]), from m in Enumerable.Skip(mi, 2)
                                                                                select new ScriptMethod(m), state) :
                                                                        method.Combine(new ScriptMethod(mi[1], @this), null, state);
        }

        private ScriptMethod MakeGenericMethod(IList<IScriptObject> args)
        {
            var genericTypes = from a in args
                               let type = ScriptClass.GetType(a as IScriptContract)
                               where type != null
                               select type;
            return new ScriptMethod(Method.MakeGenericMethod(Enumerable.ToArray(genericTypes)));
        }

        protected override IScriptObject InvokeCore(IList<IScriptObject> args, InterpreterState state)
        {
            switch (Method.IsGenericMethodDefinition)
            {
                case true:
                    return MakeGenericMethod(args);
                default:
                    if (m_parameters == null) m_parameters = Array.ConvertAll(Method.GetParameters(), p => p.ParameterType);
                    var methodArguments = default(object[]);
                    switch (NativeObject.TryConvert(args, out methodArguments, m_parameters, state))
                    {
                        case true:
                            var result = Method.Invoke(This, methodArguments);
                            return Method.ReturnType == null || Equals(Method.ReturnType, typeof(void)) ? Void : NativeObject.ConvertFrom(result, Method.ReturnType);
                        default:
                            throw new UnsupportedOperationException(state);
                    }
            }
        }

        MethodInfo IScriptMethod.Method
        {
            get { return Method; }
        }

        Delegate IScriptMethod.CreateDelegate(Type delegateType, bool throwOnBindFailure)
        {
            return Delegate.CreateDelegate(delegateType, This, Method, throwOnBindFailure);
        }

        /// <summary>
        /// Returns a string representation of this method.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Method.ToString();
        }
    }
}
