using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class ScriptMethod : ScriptActionBase, IScriptMethod
    {
        #region Nested Types

        /// <summary>
        /// Represents delegate converter.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        public sealed class DelegateConverter : RuntimeConverter<Delegate>
        {
            public override bool Convert(Delegate input, out IScriptObject result)
            {
                var @this = input != null ? new NativeObject(input.Target) : null;
                var methods = input.GetInvocationList();
                switch (methods.LongLength > 1L)
                {
                    case true:
                        result = new ScriptArray(Array.ConvertAll(methods, m => new ScriptMethod(m.Method, @this)));
                        break;
                    default: result = new ScriptMethod(methods[0].Method, @this); break;
                }
                return true;
            }
        }

        [ComVisible(false)]
        private sealed class ScriptDelegate
        {
            public readonly InterpreterState State;
            public readonly IScriptAction Implementation;
            private readonly Type DelegateType;

            public ScriptDelegate(Type delegateType, IScriptAction action, InterpreterState state)
            {
                if (delegateType == null) throw new ArgumentNullException("delegateType");
                if (action == null) throw new ArgumentNullException("action");
                if (state == null) throw new ArgumentNullException("state");
                Implementation = action;
                State = state;
                DelegateType = delegateType;
            }

            public object DynamicInvoke(object[] arguments)
            {
                var parameterTypes = default(List<Type>);
                var returnType = default(Type);
                if (GetSignature(DelegateType, out parameterTypes, out returnType))
                {
                    var scriptArguments = new IScriptObject[arguments.Length];
                    Parallel.For(0, arguments.Length, i => scriptArguments[i] = NativeObject.ConvertFrom(arguments[i], parameterTypes[i]));
                    var result = default(object);
                    if (NativeObject.TryConvert(Implementation.Invoke(scriptArguments, State), returnType, State, out result))
                        return result;
                }
                throw new NotSupportedException();
            }

            private static void Implements(ILGenerator dm, IList<Type> parameters)
            {
                var array = dm.DeclareLocal(typeof(object[]));
                dm.Emit(OpCodes.Ldc_I4, parameters.Count - 1);  
                dm.Emit(OpCodes.Newarr, typeof(object));
                dm.Emit(OpCodes.Stloc, array);       // array = new object[parameters.Count - 1];
                //Save all arguments to the array
                for (var i = 1; i < parameters.Count; i++)
                {
                    dm.Emit(OpCodes.Ldloc, array);   
                    dm.Emit(OpCodes.Ldc_I4, i - 1); 
                    dm.Emit(OpCodes.Ldarg, i);  
                    if (parameters[i].IsValueType) dm.Emit(OpCodes.Box, parameters[i]); //boxes value if it is necessary
                    dm.Emit(OpCodes.Stelem_Ref);    //result[i - 1] = arg.i;
                }
                dm.Emit(OpCodes.Ldarg_0);
                dm.Emit(OpCodes.Ldloc, array);
                //Load this reference
                dm.Emit(OpCodes.Call, typeof(ScriptDelegate).GetMethod("DynamicInvoke", new[] { typeof(object[]) }));
                //Return value
                dm.Emit(OpCodes.Ret);
            }

            private static DynamicMethod CreateMethod(string methodName, List<Type> parameters, Type returnType)
            {
                parameters.Insert(0, typeof(ScriptDelegate));
                var method = new DynamicMethod(methodName, returnType, parameters.ToArray(), typeof(ScriptDelegate));
                Implements(method.GetILGenerator(), parameters);
                return method;
            }

            private static bool GetSignature(Type dt, out List<Type> parameters, out Type returnType)
            {
                var invokeMethod = dt.GetMethod("Invoke");
                switch (invokeMethod != null)
                {
                    case true:
                        parameters = new List<Type>(from p in invokeMethod.GetParameters() select p.ParameterType);
                        returnType = invokeMethod.ReturnType;
                        return true;
                    default:
                        parameters = new List<Type>();
                        returnType = null;
                        return false;
                }
            }

            public Delegate CreateDelegate()
            {
                var parameters = default(List<Type>);
                var returnType = default(Type);
                var method = GetSignature(DelegateType, out parameters, out returnType) ?
                    CreateMethod(Implementation.ToString(), parameters, returnType) :
                    null;
                return method != null ? method.CreateDelegate(DelegateType, this) : null;
            }
        }
        #endregion

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

        private static ScriptMethod MakeGenericMethod(MethodInfo mi, IList<IScriptObject> args)
        {
            var genericTypes = from a in args
                               let type = ScriptClass.GetType(a as IScriptContract)
                               where type != null
                               select type;
            return new ScriptMethod(mi.MakeGenericMethod(Enumerable.ToArray(genericTypes)));
        }

        private static IScriptObject Invoke(MethodInfo mi, object @this, IList<IScriptObject> args, InterpreterState state, ref Type[] parameters)
        {
            switch (mi.IsGenericMethodDefinition)
            {
                case true:
                    return MakeGenericMethod(mi, args);
                default:
                    if (parameters == null) parameters = Array.ConvertAll(mi.GetParameters(), p => p.ParameterType);
                    var methodArguments = default(object[]);
                    switch (NativeObject.TryConvert(args, out methodArguments, parameters, state))
                    {
                        case true:
                            var result = mi.Invoke(@this, methodArguments);
                            return mi.ReturnType == null || Equals(mi.ReturnType, typeof(void)) ? Void : NativeObject.ConvertFrom(result, mi.ReturnType);
                        default:
                            throw new UnsupportedOperationException(state);
                    }
            }
        }

        public static IScriptObject Invoke(MethodInfo mi, object @this, IList<IScriptObject> args, InterpreterState state)
        {
            var parameters = default(Type[]);
            return Invoke(mi, @this, args, state, ref parameters);
        }

        public static IScriptObject Invoke(Delegate d, IList<IScriptObject> args, InterpreterState state)
        {
            return Invoke(d.Method, d.Target, args, state);
        }

        protected override IScriptObject InvokeCore(IList<IScriptObject> args, InterpreterState state)
        {
            return Invoke(Method, This, args, state, ref m_parameters);
        }

        MethodInfo IScriptMethod.Method
        {
            get { return Method; }
        }

        /// <summary>
        /// Returns a string representation of this method.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Method.ToString();
        }

        bool IScriptConvertible.TryConvertTo(Type conversionType, out object result)
        {
            if (typeof(MethodInfo).IsAssignableFrom(conversionType))
                result = Method;
            else if (typeof(Delegate).IsAssignableFrom(conversionType))
                result = Delegate.CreateDelegate(conversionType, This, Method, false);
            else result = null;
            return result != null;
        }

        bool IScriptConvertible.TryConvert(out object result)
        {
            result = Method;
            return true;
        }

        public static Delegate CreateDelegate(Type delegateType, IScriptAction implementation, InterpreterState state)
        {
            if (implementation is IScriptMethod)
            {
                var result = default(object);
                return ((IScriptMethod)implementation).TryConvertTo(delegateType, out result) ? result as Delegate : null;
            }
            else
            {
                var instance = new ScriptDelegate(delegateType, implementation, state);
                return instance.CreateDelegate();
            }
        }
    }
}
