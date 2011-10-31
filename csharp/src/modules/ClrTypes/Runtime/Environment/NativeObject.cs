using System;

namespace DynamicScript.Runtime.Environment
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents wrapper of the native .NET object.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class NativeObject: INativeObject
    {
        public readonly object Instance;

        /// <summary>
        /// Initializes a new wrapper of the native .NET object.
        /// </summary>
        /// <param name="obj">An object to wrap.</param>
        public NativeObject(object obj)
        {
            Instance = obj;
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
        public static IScriptObject ConvertFrom(object obj)
        {
            var scriptRepresentation = default(IScriptObject);
            if (obj == null)
                return ScriptObject.Void;
            if (ScriptObject.TryConvert(obj, out scriptRepresentation))
                return scriptRepresentation;
            else return new NativeObject(obj);
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

        private static IScriptObject BinaryOperation(dynamic left, ScriptCodeBinaryOperatorType @operator, dynamic right)
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
                default: return ScriptObject.Void;
            }
            return ConvertFrom(result);
        }

        IScriptObject IScriptObject.BinaryOperation(ScriptCodeBinaryOperatorType @operator, IScriptObject right, InterpreterState state)
        {
            var r = default(object);
            if (TryConvert(right, state, out r))
                return BinaryOperation(Instance, @operator, r);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state);
        }

        private static IScriptObject UnaryOperation(dynamic operand, ScriptCodeUnaryOperatorType @operator)
        {
            var result = default(object);
            switch (@operator)
            {
                case ScriptCodeUnaryOperatorType.DecrementPostfix:
                    result = operand--; break;
            }
            return ConvertFrom(result);
        }

        IScriptObject IScriptObject.UnaryOperation(ScriptCodeUnaryOperatorType @operator, InterpreterState state)
        {
            throw new NotImplementedException();
        }

        public IScriptObject Invoke(System.Collections.Generic.IList<IScriptObject> args, InterpreterState state)
        {
            throw new NotImplementedException();
        }

        public IRuntimeSlot this[string slotName, InterpreterState state]
        {
            get { throw new NotImplementedException(); }
        }

        public IScriptObject GetRuntimeDescriptor(string slotName, InterpreterState state)
        {
            throw new NotImplementedException();
        }

        public IRuntimeSlot this[IScriptObject[] args, InterpreterState state]
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.Generic.ICollection<string> Slots
        {
            get { throw new NotImplementedException(); }
        }

        public IScriptContract GetContractBinding()
        {
            throw new NotImplementedException();
        }

        public System.Dynamic.DynamicMetaObject GetMetaObject(System.Linq.Expressions.Expression parameter)
        {
            throw new NotImplementedException();
        }
    }
}
