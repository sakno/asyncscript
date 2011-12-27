using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;

namespace DynamicScript.Runtime.Environment
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents wrapper of the native .NET object.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class NativeObject: DynamicObject, INativeObject, IScriptIterable
    {
        private const BindingFlags MemberFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase;
        public readonly object Instance;
        public readonly ScriptClass ContractBinding;
        private static readonly MethodInfo DynamicCastConverter;

        static NativeObject()
        {
            DynamicCastConverter = new Func<dynamic, object>(DynamicCast<object>).Method;
            DynamicCastConverter = DynamicCastConverter.GetGenericMethodDefinition();
        }

        /// <summary>
        /// Initializes a new wrapper of the native .NET object.
        /// </summary>
        /// <param name="obj">An object to wrap.</param>
        public NativeObject(object obj, Type slice = null)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            Instance = obj;
            ContractBinding = (ScriptClass)(slice ?? obj.GetType());
        }

        object INativeObject.Instance
        {
            get { return Instance; }
        }

        /// <summary>
        /// Converts native .NET object to the script-compliant representation.
        /// </summary>
        /// <param name="obj">An object to convert.</param>
        /// <returns>Conversion result.</returns>
        public static IScriptObject ConvertFrom(object obj, Type destinationType = null)
        {
            var scriptRepresentation = default(IScriptObject);
            if (obj == null || Equals(typeof(void), destinationType))
                return ScriptObject.Void;
            if (ScriptObject.TryConvert(obj, out scriptRepresentation))
                return scriptRepresentation;
            else return new NativeObject(obj, destinationType);
        }

        /// <summary>
        /// Attempts to convert script object to the CLR-compliant object.
        /// </summary>
        /// <param name="obj">The script object to convert.</param>
        /// <param name="conversionType">Type of the conversion result.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <param name="result">Conversion result.</param>
        /// <returns><see langword="true"/> if conversion is possible; otherwise, <see langword="false"/>.</returns>
        public static bool TryConvert(IScriptObject obj, Type conversionType, InterpreterState state, out object result)
        {
            if (conversionType.IsByRef) conversionType = conversionType.GetElementType();
            if (ScriptObject.IsVoid(obj) || Equals(typeof(void), conversionType))
            {
                result = null;
                return true;
            }
            if (obj is INativeObject)
                result = ((INativeObject)obj).Instance;
            else if (obj is IScriptConvertible)
                switch (conversionType != null)
                {
                    case true:
                        ((IScriptConvertible)obj).TryConvertTo(conversionType, out result);
                        break;
                    default:
                        ((IScriptConvertible)obj).TryConvert(out result);
                        break;
                }
            else if (typeof(Delegate).IsAssignableFrom(conversionType) && obj is IScriptFunction)
                result = ScriptMethod.CreateDelegate(conversionType, (IScriptFunction)obj, state);
            else result = null;
            return result != null;
        }

        /// <summary>
        /// Attempts to convert script object to the CLR-compliant object.
        /// </summary>
        /// <param name="obj">The script object to convert.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <param name="result">Conversion result.</param>
        /// <returns><see langword="true"/> if conversion is possible; otherwise, <see langword="false"/>.</returns>
        public static bool TryConvert(IScriptObject obj, InterpreterState state, out object result)
        {
            return TryConvert(obj, null, state, out result);
        }

        public static bool TryConvert(IList<IScriptObject> objects, out object[] result, Type[] types, InterpreterState state)
        {
            if (types == null) types = new Type[objects.Count];
            result = new object[objects.Count];
            for (var i = 0; i < objects.Count; i++)
            {
                var element = default(object);
                if (TryConvert(objects[i], types[i], state, out element)) result[i] = element;
                else return false;
            }
            return true;
        }

        public static bool TryConvert(IList<IScriptObject> objects, out Array result, Type destinationType, InterpreterState state)
        {
            result = Array.CreateInstance(destinationType, objects.Count);
            for (var i = 0; i < objects.Count; i++)
            {
                var element = default(object);
                if (TryConvert(objects[i], destinationType, state, out element)) result.SetValue(element, i);
                else return false;
            }
            return true;
        }

        private static ScriptBoolean Contains(object left, IEnumerable right, InterpreterState state)
        {
            foreach (var element in right)
                if (Equals(left, element))
                    return  ScriptBoolean.True;
            return ScriptBoolean.False;
        }

        private static IScriptObject PartOf(object left, object right, InterpreterState state)
        {
            if (right is IScriptObject)
                return ConvertFrom(left).BinaryOperation(ScriptCodeBinaryOperatorType.PartOf, (IScriptObject)right, state);
            else if (right is IEnumerable)
                return Contains(left, (IEnumerable)right, state);
            else return ScriptBoolean.False;
        }

        private static object DynamicCast<T>(dynamic source)
        {
            return (T)source;
        }

        private static IScriptObject CastTo(object source, Type destinationType)
        {
            var result = DynamicCastConverter.MakeGenericMethod(destinationType).Invoke(null, new[] { source });
            return ConvertFrom(result, destinationType);
        }

        private static IScriptObject CastTo(object source, IScriptContract destinationType)
        {
            return CastTo(source, ScriptClass.GetType(destinationType));
        }

        private static IScriptObject BinaryOperation(dynamic left, ScriptCodeBinaryOperatorType @operator, dynamic right, InterpreterState state)
        {
            var result = default(object);
            switch (@operator)
            {
                case ScriptCodeBinaryOperatorType.Add:
                    result = left + right;
                    break;
                case ScriptCodeBinaryOperatorType.AdditiveAssign:
                    result = (left += right);
                    break;
                case ScriptCodeBinaryOperatorType.AndAlso:
                    result = left && right;
                    break;
                case ScriptCodeBinaryOperatorType.Assign:
                    result = (result = right);
                    break;
                case ScriptCodeBinaryOperatorType.Coalesce:
                    result = left ?? right;
                    break;
                case ScriptCodeBinaryOperatorType.Divide:
                    result = left / right;
                    break;
                case ScriptCodeBinaryOperatorType.DivideAssign:
                    result = (left /= right);
                    break;
                case ScriptCodeBinaryOperatorType.Exclusion:
                    result = left ^ right;
                    break;
                case ScriptCodeBinaryOperatorType.ExclusionAssign:
                    result = (left ^= right);
                    break;
                case ScriptCodeBinaryOperatorType.Expansion:
                    result = (left |= right);
                    break;
                case ScriptCodeBinaryOperatorType.GreaterThan:
                    result = left > right;
                    break;
                case ScriptCodeBinaryOperatorType.GreaterThanOrEqual:
                    result = left >= right;
                    break;
                case ScriptCodeBinaryOperatorType.Initializer:
                    result = left == null ? (left = right) : left;
                    break;
                case ScriptCodeBinaryOperatorType.Intersection:
                    result = left & right;
                    break;
                case ScriptCodeBinaryOperatorType.LessThan:
                    result = left < right;
                    break;
                case ScriptCodeBinaryOperatorType.LessThanOrEqual:
                    result = left <= right;
                    break;
                case ScriptCodeBinaryOperatorType.Modulo:
                    result = left % right;
                    break;
                case ScriptCodeBinaryOperatorType.ModuloAssign:
                    result = (left %= right);
                    break;
                case ScriptCodeBinaryOperatorType.MultiplicativeAssign:
                    result = (left *= right);
                    break;
                case ScriptCodeBinaryOperatorType.Multiply:
                    result = left * right;
                    break;
                case ScriptCodeBinaryOperatorType.OrElse:
                    result = left || right;
                    break;
                case ScriptCodeBinaryOperatorType.Reduction:
                    result = (left &= right);
                    break;
                case ScriptCodeBinaryOperatorType.ReferenceEquality:
                    return (ScriptBoolean)ReferenceEquals(left, right);
                case ScriptCodeBinaryOperatorType.ReferenceInequality:
                    return (ScriptBoolean)(!ReferenceEquals(left, right));
                case ScriptCodeBinaryOperatorType.Subtract:
                    result = left - right;
                    break;
                case ScriptCodeBinaryOperatorType.SubtractiveAssign:
                    result = (left -= right);
                    break;
                case ScriptCodeBinaryOperatorType.Union:
                    result = left | right;
                    break;
                case ScriptCodeBinaryOperatorType.ValueEquality:
                    result = left == right;
                    break;
                case ScriptCodeBinaryOperatorType.ValueInequality:
                    result = left != right;
                    break;
                case ScriptCodeBinaryOperatorType.PartOf:
                    return PartOf(left, right, state);
                case ScriptCodeBinaryOperatorType.TypeCast:
                    return CastTo(left, right as IScriptContract);
                default: return ScriptObject.Void;
            }
            return ConvertFrom(result);
        }

        IScriptObject IScriptObject.BinaryOperation(ScriptCodeBinaryOperatorType @operator, IScriptObject right, InterpreterState state)
        {
            var r = default(object);
            if (TryConvert(right, state, out r))
                return BinaryOperation(Instance, @operator, r, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state);
        }

        private static IScriptObject UnaryOperation(dynamic operand, ScriptCodeUnaryOperatorType @operator, IScriptClass objectType)
        {
            var result = default(object);
            switch (@operator)
            {
                case ScriptCodeUnaryOperatorType.DecrementPostfix:
                    result = operand--; break;
                case ScriptCodeUnaryOperatorType.DecrementPrefix:
                    result = --operand; break;
                case ScriptCodeUnaryOperatorType.IncrementPostfix:
                    result = operand++; break;
                case ScriptCodeUnaryOperatorType.IncrementPrefix:
                    result = ++operand; break;
                case ScriptCodeUnaryOperatorType.Intern: break;
                case ScriptCodeUnaryOperatorType.Minus:
                    result = -operand; break;
                case ScriptCodeUnaryOperatorType.Negate:
                    result = !operand; break;
                case ScriptCodeUnaryOperatorType.Plus:
                    result = +operand; break;
                case ScriptCodeUnaryOperatorType.SquarePrefix:
                case ScriptCodeUnaryOperatorType.SquarePostfix:
                    result = operand * operand; break;
                case ScriptCodeUnaryOperatorType.VoidCheck:
                    result = operand == null; break;
                case ScriptCodeUnaryOperatorType.TypeOf:
                    return objectType;
                default: return ScriptObject.Void;
            }
            return ConvertFrom(result);
        }

        IScriptObject IScriptObject.UnaryOperation(ScriptCodeUnaryOperatorType @operator, InterpreterState state)
        {
            return UnaryOperation(Instance, @operator, ContractBinding);
        }

        IScriptObject IScriptObject.Invoke(IList<IScriptObject> args, InterpreterState state)
        {
            if (Instance is Delegate)
                return ScriptMethod.Invoke((Delegate)Instance, args, state);
            else throw new UnsupportedOperationException(state);
        }

        public IScriptObject this[string slotName, InterpreterState state]
        {
            get { return ContractBinding[slotName, MemberFlags, this, state]; }
            set { ContractBinding[slotName, MemberFlags, this, state] = value; }
        }

        public IScriptObject this[IList<IScriptObject> indicies, InterpreterState state]
        {
            get { return ContractBinding[indicies, this, state]; }
            set { ContractBinding[indicies, this, state] = value; }
        }

        /// <summary>
        /// Gets collection of available members.
        /// </summary>
        public ICollection<string> Slots
        {
            get { return ReflectionEngine.GetMemberNames(ContractBinding.NativeType, MemberFlags); }
        }

        IScriptContract IScriptObject.GetContractBinding()
        {
            return ContractBinding;
        }

        public static IScriptObject New(IList<IScriptObject> args, Type target, InterpreterState state)
        {
            var constructorArguments = default(object[]);
            switch (TryConvert(args, out constructorArguments, null, state))
            {
                case true: return new NativeObject(Activator.CreateInstance(target, constructorArguments));
                default: return ScriptObject.Void;
            }
        }

        public override string ToString()
        {
            return Instance.ToString();
        }

        IEnumerator IScriptIterable.GetIterator(InterpreterState state)
        {
            if (Instance is IEnumerable)
                foreach (var obj in (IEnumerable)Instance)
                    yield return NativeObject.ConvertFrom(obj);
            else yield break;
        }
    }
}
