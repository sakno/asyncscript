﻿using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CallSiteBinder = System.Runtime.CompilerServices.CallSiteBinder;
    using QCodeUnaryOperatorType = Compiler.Ast.ScriptCodeUnaryOperatorType;
    using ScriptCodeBinaryOperatorType = Compiler.Ast.ScriptCodeBinaryOperatorType;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using SystemConverter = System.Convert;
    using SystemEnvironment = System.Environment;

    /// <summary>
    /// Represents interpreter internal routines and helper methods.
    /// </summary>
    [ComVisible(false)]
    public static class RuntimeHelpers
    {
        #region IScriptObject Extensions

        /// <summary>
        /// Determines whether the specified object is one of the specified type.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool OneOf<T1, T2, T3, T4, T5>(this IScriptObject obj)
            where T1 : IScriptObject
            where T2 : IScriptObject
            where T3 : IScriptObject
            where T4 : IScriptObject
            where T5 : IScriptObject
        {
            return obj is T1 || obj is T2 || obj is T3 || obj is T4 || obj is T5;
        }

        /// <summary>
        /// Determines whether the specified object is one of the specified type.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool OneOf<T1, T2, T3, T4>(this IScriptObject obj)
            where T1 : IScriptObject
            where T2 : IScriptObject
            where T3 : IScriptObject
            where T4 : IScriptObject
        {
            return OneOf<T1, T2, T3, T4, T4>(obj);
        }

        /// <summary>
        /// Determines whether the specified object is one of the specified type.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool OneOf<T1, T2, T3>(this IScriptObject obj)
            where T1 : IScriptObject
            where T2 : IScriptObject
            where T3 : IScriptObject
        {
            return OneOf<T1, T2, T3, T3>(obj);
        }

        /// <summary>
        /// Determines whether the specified object is one of the specified type.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool OneOf<T1, T2>(this IScriptObject obj)
            where T1 : IScriptObject
            where T2 : IScriptObject
        {
            return OneOf<T1, T2, T2>(obj);
        }

        /// <summary>
        /// Determines whether the specified object is <see langword="true"/> operator.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        public static bool IsTrue(this IScriptObject value, InterpreterState state)
        {
            switch (SystemConverter.GetTypeCode(value))
            {
                case TypeCode.String: return true;
                case TypeCode.Int16:
                    return SystemConverter.ToInt16(value) != 0;
                case TypeCode.Int32:
                    return SystemConverter.ToInt32(value) != 0;
                case TypeCode.Int64:
                    return SystemConverter.ToInt64(value) != 0;
                case TypeCode.Boolean:
                    return SystemConverter.ToBoolean(value);
                case TypeCode.Double:
                    return SystemConverter.ToDouble(value) != 0.0;
                case TypeCode.Single:
                    return SystemConverter.ToSingle(value) != 0.0F;
                default:
                    return value is IScriptProxyObject ?
                        IsTrue(((IScriptProxyObject)value).Unwrap(state), state) :
                        !ScriptObject.IsVoid(value);
            }
        }

        /// <summary>
        /// Determines whether the specified object is <see langword="true"/> operator.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns></returns>
        public static bool IsTrue(this IScriptObject value)
        {
            return IsTrue(value, InterpreterState.Current);
        }

        /// <summary>
        /// Determines whether the specified object is <see langword="false"/> operator.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        public static bool IsFalse(this IScriptObject value, InterpreterState state)
        {
            return !IsTrue(value, state);
        }

        /// <summary>
        /// Determines whether the specified object is <see langword="false"/> operator.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns></returns>
        public static bool IsFalse(this IScriptObject value)
        {
            return IsFalse(value, InterpreterState.Current);
        }

        /// <summary>
        /// Computes subtraction between the current object and the specified object.
        /// </summary>
        /// <param name="left">The left operand. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The subtrahend.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The subtraction result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject Subtract(this IScriptObject left, IScriptObject right, InterpreterState state)
        {
            if (left == null) throw new ArgumentNullException("left");
            return left.BinaryOperation(ScriptCodeBinaryOperatorType.Subtract, right, state);
        }

        /// <summary>
        /// Computes subtraction between the current object and the specified object.
        /// </summary>
        /// <param name="left">The leeft operand. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The subtrahend.</param>
        /// <returns>The subtraction result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject Subtract(this IScriptObject left, IScriptObject right)
        {
            return Subtract(left, right);
        }

        /// <summary>
        /// Converts the current object to the specified contract.
        /// </summary>
        /// <param name="value">The value to be converted. Cannot be <see langword="null"/>.</param>
        /// <param name="contract">The conversion destination. </param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The conversion result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public static IScriptObject Convert(this IScriptObject value, IScriptContract contract, InterpreterState state)
        {
            if (contract == null) throw new ArgumentNullException("contract");
            return value.BinaryOperation(ScriptCodeBinaryOperatorType.TypeCast, contract, state);
        }

        /// <summary>
        /// Converts the current object to the specified contract.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <param name="contract">The conversion destination. Cannot be <see langword="null"/>.</param>
        /// <returns>The conversion result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="contract"/> is <see langword="null"/>.</exception>
        public static IScriptObject Convert(this IScriptObject value, IScriptContract contract)
        {
            return Convert(value, contract, InterpreterState.Current);
        }

        /// <summary>
        /// Determines whether the current object is not equal to another.
        /// </summary>
        /// <param name="left">The first operand. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The second operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The comparison result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject NotEquals(this IScriptObject left, IScriptObject right, InterpreterState state)
        {
            if (left == null) throw new ArgumentNullException("left");
            return left.BinaryOperation(ScriptCodeBinaryOperatorType.ValueInequality, right, state);
        }

        /// <summary>
        /// Determines whether the current object is not equal to another.
        /// </summary>
        /// <param name="left">The first operand. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The second operand.</param>
        /// <returns>The comparison result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject NotEquals(this IScriptObject left, IScriptObject right)
        {
            return NotEquals(left, right, InterpreterState.Current);
        }

        /// <summary>
        /// Computes multiplication between the current object and the specified object.
        /// </summary>
        /// <param name="left">THe first operand. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The second operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The multiplication of the two objects.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject Multiply(this IScriptObject left, IScriptObject right, InterpreterState state)
        {
            if (left == null) throw new ArgumentNullException("left");
            return left.BinaryOperation(ScriptCodeBinaryOperatorType.Multiply, right, state);
        }

        /// <summary>
        /// Computes multiplication between the current object and the specified object.
        /// </summary>
        /// <param name="left">THe first operand. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The second operand.</param>
        /// <returns>The multiplication of the two objects.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject Multiply(this IScriptObject left, IScriptObject right)
        {
            return Multiply(left, right, InterpreterState.Current);
        }

        /// <summary>
        /// Computes the remainder after dividing the current object by the second.
        /// </summary>
        /// <param name="left">THe left operand. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The remainder.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject Modulo(this IScriptObject left, IScriptObject right, InterpreterState state)
        {
            if (left == null) throw new ArgumentNullException("left");
            return left.BinaryOperation(ScriptCodeBinaryOperatorType.Modulo, right, state);
        }

        /// <summary>
        /// Computes the remainder after dividing the current object by the second.
        /// </summary>
        /// <param name="left">THe left operand. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The remainder.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject Modulo(this IScriptObject left, IScriptObject right)
        {
            return Modulo(left, right, InterpreterState.Current);
        }

        /// <summary>
        /// Determines whether the current object is less than or equal to the specified object.
        /// </summary>
        /// <param name="left">The first object to compare. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is less than or equal to the specified object; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject LessThanOrEqual(this IScriptObject left, IScriptObject right, InterpreterState state)
        {
            if (left == null) throw new ArgumentNullException("left");
            return left.BinaryOperation(ScriptCodeBinaryOperatorType.LessThanOrEqual, right, state);
        }

        /// <summary>
        /// Determines whether the current object is less than or equal to the specified object.
        /// </summary>
        /// <param name="left">The first object to compare. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns><see langword="true"/>the current object is less than or equal to the specified object; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject LessThanOrEqual(this IScriptObject left, IScriptObject right)
        {
            return LessThanOrEqual(left, right, InterpreterState.Current);
        }

        /// <summary>
        /// Determines whether the current object is less than the specified object.
        /// </summary>
        /// <param name="left">The left object to compare. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is less than the specified object; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject LessThan(this IScriptObject left, IScriptObject right, InterpreterState state)
        {
            if (left == null) throw new ArgumentNullException("left");
            return left.BinaryOperation(ScriptCodeBinaryOperatorType.LessThan, right, state);
        }

        /// <summary>
        /// Determines whether the current object is less than the specified object.
        /// </summary>
        /// <param name="left">The left object to compare. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns><see langword="true"/>the current object is less than the specified object; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject LessThan(this IScriptObject left, IScriptObject right)
        {
            return LessThan(left, right, InterpreterState.Current);
        }

        /// <summary>
        /// Determines whether the current object is greater than or equal to the specified object.
        /// </summary>
        /// <param name="left">The first object to compare. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is greater than or equal to the specified object; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject GreaterThanOrEqual(this IScriptObject left, IScriptObject right, InterpreterState state)
        {
            if (left == null) throw new ArgumentNullException("left");
            return left.BinaryOperation(ScriptCodeBinaryOperatorType.GreaterThanOrEqual, right, state);
        }

        /// <summary>
        /// Determines whether the current object is greater than or equal to the specified object.
        /// </summary>
        /// <param name="left">The first object to compare. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns><see langword="true"/>the current object is greater than or equal to the specified object; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject GreaterThanOrEqual(this IScriptObject left, IScriptObject right)
        {
            return GreaterThanOrEqual(left, right, InterpreterState.Current);
        }

        /// <summary>
        /// Determines whether the current object is greater than the specified object.
        /// </summary>
        /// <param name="left">The first object to compare. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is greater than the specified object; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject GreaterThan(this IScriptObject left, IScriptObject right, InterpreterState state)
        {
            if (left == null) throw new ArgumentNullException("left");
            return left.BinaryOperation(ScriptCodeBinaryOperatorType.GreaterThan, right, state);
        }

        /// <summary>
        /// Determines whether the current object is greater than the specified object.
        /// </summary>
        /// <param name="left">The first object to compare. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns><see langword="true"/>the current object is greater than the specified object; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject GreaterThan(this IScriptObject left, IScriptObject right)
        {
            return GreaterThan(left, right, InterpreterState.Current);
        }

        /// <summary>
        /// Computes exclusive or, or difference between two objects.
        /// </summary>
        /// <param name="left">The first operand. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The second operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The computation result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject ExclusiveOr(this IScriptObject left, IScriptObject right, InterpreterState state)
        {
            if (left == null) throw new ArgumentNullException("left");
            return left.BinaryOperation(ScriptCodeBinaryOperatorType.Exclusion, right, state);
        }

        /// <summary>
        /// Computes exclusive or, or difference between two objects.
        /// </summary>
        /// <param name="left">The first operand. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The second operand.</param>
        /// <returns>The computation result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject ExclusiveOr(this IScriptObject left, IScriptObject right)
        {
            return ExclusiveOr(left, right, InterpreterState.Current);
        }

        /// <summary>
        /// Determines whether the the current object is equal to another.
        /// </summary>
        /// <param name="left">The first operand. Cannot be <see langword="null"/>.</param>
        /// <param name="right">Other object to be compared.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The comparison result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject Equals(this IScriptObject left, IScriptObject right, InterpreterState state)
        {
            if (left == null) throw new ArgumentNullException("left");
            return left.BinaryOperation(ScriptCodeBinaryOperatorType.ValueEquality, right, state);
        }

        /// <summary>
        /// Divides the current object using the specified.
        /// </summary>
        /// <param name="left">The first operand. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The right operand of the division operator.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The division result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject Divide(this IScriptObject left, IScriptObject right, InterpreterState state)
        {
            if (left == null) throw new ArgumentNullException("left");
            return left.BinaryOperation(ScriptCodeBinaryOperatorType.Divide, right, state);
        }

        /// <summary>
        /// Divides the current object using the specified.
        /// </summary>
        /// <param name="left">The first operand. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The right operand of the division operator.</param>
        /// <returns>The division result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject Divide(this IScriptObject left, IScriptObject right)
        {
            return Divide(left, right, InterpreterState.Current);
        }

        /// <summary>
        /// Returns coalesce result.
        /// </summary>
        /// <param name="left">The left operand. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The right operand of coalescing operation.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The operation result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject Coalesce(this IScriptObject left, IScriptObject right, InterpreterState state)
        {
            if (left == null) throw new ArgumentNullException("left");
            return left.BinaryOperation(ScriptCodeBinaryOperatorType.Coalesce, right, state);
        }

        /// <summary>
        /// Returns coalesce result.
        /// </summary>
        /// <param name="left">The left operand. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The right operand of coalescing operation.</param>
        /// <returns>The operation result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject Coalesce(this IScriptObject left, IScriptObject right)
        {
            return Coalesce(left, right, InterpreterState.Current);
        }

        

        /// <summary>
        /// Computes logical or, bitwise or, or union.
        /// </summary>
        /// <param name="left">The left operand. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The result of the binary operation.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject Or(this IScriptObject left, IScriptObject right, InterpreterState state)
        {
            if (left == null) throw new ArgumentNullException("left");
            return left.BinaryOperation(ScriptCodeBinaryOperatorType.Union, right, state);
        }

        /// <summary>
        /// Computes logical or, bitwise or, or union.
        /// </summary>
        /// <param name="left">The left operand. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the binary operation.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject Or(this IScriptObject left, IScriptObject right)
        {
            return Or(left, right, InterpreterState.Current);
        }

        /// <summary>
        /// Computies logical and, bitwise and, or intersection.
        /// </summary>
        /// <param name="left">The left operand. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The result of the binary operation.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject And(this IScriptObject left, IScriptObject right, InterpreterState state)
        {
            if (left == null) throw new ArgumentNullException("left");
            return left.BinaryOperation(ScriptCodeBinaryOperatorType.Intersection, right, state);
        }

        /// <summary>
        /// Computies logical and, bitwise and, or intersection.
        /// </summary>
        /// <param name="left">The left operand. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the binary operation.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject And(this IScriptObject left, IScriptObject right)
        {
            return And(left, right, InterpreterState.Current);
        }

        /// <summary>
        /// Computes the sum, or union of the current object with the specified.
        /// </summary>
        /// <param name="left">The first operand. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The second operand of the addition operation.</param>
        /// <param name="state">Internal interpretation state.</param>
        /// <returns>The result of the binary operation interpretation.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject Add(this IScriptObject left, IScriptObject right, InterpreterState state)
        {
            if (left == null) throw new ArgumentNullException("left");
            return left.BinaryOperation(ScriptCodeBinaryOperatorType.Add, right, state);
        }

        /// <summary>
        /// Computes the sum, or union of the current object with the specified.
        /// </summary>
        /// <param name="left">The first operand. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The second operand of the addition operation.</param>
        /// <returns>The result of the binary operation interpretation.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject Add(this IScriptObject left, IScriptObject right)
        {
            return Add(left, right, InterpreterState.Current);
        }

        /// <summary>
        /// Performs application operator to the current object.
        /// </summary>
        /// <param name="target">The target object to be invoked. Cannot be <see langword="null"/>.</param>
        /// <param name="args">An array of application arguments.</param>
        /// <returns>Application result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="target"/> is <see langword="null"/>.</exception>
        public static IScriptObject Invoke(this IScriptObject target, params ScriptObject[] args)
        {
            if (target == null) throw new ArgumentNullException("target");
            return target.Invoke(args, InterpreterState.Current);
        }

        /// <summary>
        /// Determines whether the current object is a part of the specified object.
        /// </summary>
        /// <param name="left">The left operand of 'in' operator. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The right operand of 'in' operator.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The operation result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject PartOf(this IScriptObject left, IScriptObject right, InterpreterState state)
        {
            if (left == null) throw new ArgumentNullException("left");
            return left.BinaryOperation(ScriptCodeBinaryOperatorType.PartOf, right, state);
        }

        /// <summary>
        /// Determines whether the current object is a part of the specified object.
        /// </summary>
        /// <param name="left">The left operand of 'in' operator. Cannot be <see langword="null"/>.</param>
        /// <param name="right">The right operand of 'in' operator.</param>
        /// <returns>The operation result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="left"/> is <see langword="null"/>.</exception>
        public static IScriptObject PartOf(this IScriptObject left, IScriptObject right)
        {
            return PartOf(left, right, InterpreterState.Current);
        }

        /// <summary>
        /// Determines whether the current object is void.
        /// </summary>
        /// <param name="operand">The operand of the unary operation. Cannot be <see langword="null"/>.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The result of the void check operation.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="operand"/> is <see langword="null"/>.</exception>
        public static IScriptObject IsVoid(this IScriptObject operand, InterpreterState state)
        {
            if (operand == null) throw new ArgumentNullException("operand");
            return operand.UnaryOperation(QCodeUnaryOperatorType.VoidCheck, state);
        }

        /// <summary>
        /// Determines whether the current object is void.
        /// </summary>
        /// <param name="operand">The operand of the unary operation.</param>
        /// <returns>The result of the void check operation.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="operand"/> is <see langword="null"/>.</exception>
        public static IScriptObject IsVoid(this IScriptObject operand)
        {
            return IsVoid(operand, InterpreterState.Current);
        }

        /// <summary>
        /// Applies postfixed ** operator to the current object.
        /// </summary>
        /// <param name="operand">The operand of the unary operation. Cannot be <see langword="null"/>.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The operation result</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="operand"/> is <see langword="null"/>.</exception>
        public static IScriptObject PostSquareAssign(this IScriptObject operand, InterpreterState state)
        {
            if (operand == null) throw new ArgumentNullException("operand");
            return operand.UnaryOperation(QCodeUnaryOperatorType.SquarePostfix, state);
        }

        /// <summary>
        /// Applies postfixed ** operator to the current object.
        /// </summary>
        /// <param name="operand">The operand of the unary operation. Cannot be <see langword="null"/>.</param>
        /// <returns>The operation result</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="operand"/> is <see langword="null"/>.</exception>
        public static IScriptObject PostSquareAssign(this IScriptObject operand)
        {
            return PostSquareAssign(operand, InterpreterState.Current);
        }

        /// <summary>
        /// Applies prefixed ** operator to the current object.
        /// </summary>
        /// <param name="operand">The operand of the unary operation. Cannot be <see langword="null"/>.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The operation result</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="operand"/> is <see langword="null"/>.</exception>
        public static IScriptObject PreSquareAssign(this IScriptObject operand, InterpreterState state)
        {
            if (operand == null) throw new ArgumentNullException("operand");
            return operand.UnaryOperation(QCodeUnaryOperatorType.SquarePrefix, state);
        }

        /// <summary>
        /// Applies prefixed ** operator to the current object.
        /// </summary>
        /// <param name="operand">The operand of the unary operation. Cannot be <see langword="null"/>.</param>
        /// <returns>The operation result</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="operand"/> is <see langword="null"/>.</exception>
        public static IScriptObject PreSquareAssign(this IScriptObject operand)
        {
            return PreSquareAssign(operand, InterpreterState.Current);
        }

        /// <summary>
        /// Applies bitwise complement or logicat negation.
        /// </summary>
        /// <param name="operand">The operand of the unary operation. Cannot be <see langword="null"/>.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The operation result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="operand"/> is <see langword="null"/>.</exception>
        public static IScriptObject Not(this IScriptObject operand, InterpreterState state)
        {
            if (operand == null) throw new ArgumentNullException("operand");
            return operand.UnaryOperation(QCodeUnaryOperatorType.Negate, state);
        }

        /// <summary>
        /// Applies bitwise complement or logicat negation.
        /// </summary>
        /// <param name="operand">The operand of the unary operation. Cannot be <see langword="null"/>.</param>
        /// <returns>The operation result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="operand"/> is <see langword="null"/>.</exception>
        public static IScriptObject Not(this IScriptObject operand)
        {
            return Not(operand, InterpreterState.Current);
        }

        /// <summary>
        /// Applies negation to the object.
        /// </summary>
        /// <param name="operand">The operand of the unary operation. Cannot be <see langword="null"/>.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>Negation result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="operand"/> is <see langword="null"/>.</exception>
        public static IScriptObject UnaryMinus(this IScriptObject operand, InterpreterState state)
        {
            if (operand == null) throw new ArgumentNullException("operand");
            return operand.UnaryOperation(QCodeUnaryOperatorType.Minus, state);
        }

        /// <summary>
        /// Applies negation to the object.
        /// </summary>
        /// <param name="operand">The operand of the unary operation. Cannot be <see langword="null"/>.</param>
        /// <returns>Negation result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="operand"/> is <see langword="null"/>.</exception>
        public static IScriptObject UnaryMinus(this IScriptObject operand)
        {
            return UnaryMinus(operand, InterpreterState.Current);
        }

        /// <summary>
        /// Applies unary plus to the object.
        /// </summary>
        /// <param name="operand">The operand of the unary operation. Cannot be <see langword="null"/>.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The operation result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="operand"/> is <see langword="null"/>.</exception>
        public static IScriptObject UnaryPlus(this IScriptObject operand, InterpreterState state)
        {
            if (operand == null) throw new ArgumentNullException("operand");
            return operand.UnaryOperation(QCodeUnaryOperatorType.Plus, state);
        }

        /// <summary>
        /// Applies unary plus to the object.
        /// </summary>
        /// <param name="operand">The operand of the unary operation. Cannot be <see langword="null"/>.</param>
        /// <returns>The operation result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="operand"/> is <see langword="null"/>.</exception>
        public static IScriptObject UnaryPlus(this IScriptObject operand)
        {
            return UnaryPlus(operand, InterpreterState.Current);
        }

        /// <summary>
        /// Performs prefixed decrement on the object.
        /// </summary>
        /// <param name="operand">The operand of the unary operation. Cannot be <see langword="null"/>.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The decremented object.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="operand"/> is <see langword="null"/>.</exception>
        public static IScriptObject PreDecrementAssign(this IScriptObject operand, InterpreterState state)
        {
            if (operand == null) throw new ArgumentNullException("operand");
            return operand.UnaryOperation(QCodeUnaryOperatorType.DecrementPrefix, state);
        }

        /// <summary>
        /// Performs prefixed decrement on the object.
        /// </summary>
        /// <param name="operand">The operand of the unary operation. Cannot be <see langword="null"/>.</param>
        /// <returns>The decremented object.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="operand"/> is <see langword="null"/>.</exception>
        public static IScriptObject PreDecrementAssign(this IScriptObject operand)
        {
            return PreDecrementAssign(operand, InterpreterState.Current);
        }

        /// <summary>
        /// Performs postfixed decrement on the object.
        /// </summary>
        /// <param name="operand">The operand of the unary operation. Cannot be <see langword="null"/>.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The decremented object.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="operand"/> is <see langword="null"/>.</exception>
        public static IScriptObject PostDecrementAssign(this IScriptObject operand, InterpreterState state)
        {
            if (operand == null) throw new ArgumentNullException("operand");
            return operand.UnaryOperation(QCodeUnaryOperatorType.DecrementPostfix, state);
        }

        /// <summary>
        /// Performs postfixed decrement on the object.
        /// </summary>
        /// <param name="operand">The operand of the unary operation. Cannot be <see langword="null"/>.</param>
        /// <returns>The decremented object.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="operand"/> is <see langword="null"/>.</exception>
        public static IScriptObject PostDecrementAssign(this IScriptObject operand)
        {
            return PreDecrementAssign(operand, InterpreterState.Current);
        }

        /// <summary>
        /// Performs prefixed increment on the object.
        /// </summary>
        /// <param name="operand">The operand of the unary operation. Cannot be <see langword="null"/>.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The incremented object.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="operand"/> is <see langword="null"/>.</exception>
        public static IScriptObject PreIncrementAssign(this IScriptObject operand, InterpreterState state)
        {
            if (operand == null) throw new ArgumentNullException("operand");
            return operand.UnaryOperation(QCodeUnaryOperatorType.IncrementPrefix, state);
        }

        /// <summary>
        /// Performs prefixed increment on the object.
        /// </summary>
        /// <param name="operand">The operand of the unary operation. Cannot be <see langword="null"/>.</param>
        /// <returns>The incremented object.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="operand"/> is <see langword="null"/>.</exception>
        public static IScriptObject PreIncrementAssign(this IScriptObject operand)
        {
            return PreIncrementAssign(operand, InterpreterState.Current);
        }

        /// <summary>
        /// Performs postfixed increment on the object.
        /// </summary>
        /// <param name="operand">The operand of the unary operation. Cannot be <see langword="null"/>.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The incremented object.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="operand"/> is <see langword="null"/>.</exception>
        public static IScriptObject PostIncrementAssign(this IScriptObject operand, InterpreterState state)
        {
            if (operand == null) throw new ArgumentNullException("operand");
            return operand.UnaryOperation(QCodeUnaryOperatorType.IncrementPostfix, state);
        }

        /// <summary>
        /// Performs postfixed increment on the object.
        /// </summary>
        /// <param name="operand">The operand of the unary operation. Cannot be <see langword="null"/>.</param>
        /// <returns>The incremented object.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="operand"/> is <see langword="null"/>.</exception>
        public static IScriptObject PostIncrementAssign(this IScriptObject operand)
        {
            return PostIncrementAssign(operand, InterpreterState.Current);
        }

        private static bool IsAssignableForm(IScriptContract contract, IScriptContract source, out bool theSame)
        {
            switch (contract.GetRelationship(source))
            {
                case ContractRelationshipType.TheSame:
                    theSame = true;
                    return true;
                case ContractRelationshipType.Superset:
                    theSame = false;
                    return true;
                default:
                    theSame = false;
                    return false;
            }
        }

        /// <summary>
        /// Determines whether an object of the current contract can be assigned from an object of the specified contract. 
        /// </summary>
        /// <param name="contract">The contract to be compared.</param>
        /// <param name="source">The second contract to compare.</param>
        /// <returns><see langword="true"/> if an object of the current contract can be assigned from an object of the specified contract;
        /// otherwise, <see langword="false"/>.</returns>
        public static bool IsAssignableFrom(this IScriptContract contract, IScriptContract source)
        {
            if (contract == null) throw new ArgumentNullException("contract");
            if (source == null) throw new ArgumentNullException("source");
            var theSame = default(bool);
            return IsAssignableForm(contract, source, out theSame);
        }

        /// <summary>
        /// Determines whether the specified object is satisfied to the contract.
        /// </summary>
        /// <param name="contract">The target contract. Cannot be <see langword="null"/>.</param>
        /// <param name="obj">The object to check.</param>
        /// <returns><see langword="true"/> if the specified object is satisfied to the current contract.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="contract"/> is <see langword="null"/>.</exception>
        public static bool IsCompatible(this IScriptContract contract, IScriptObject obj)
        {
            if (contract == null) throw new ArgumentNullException("contract");
            if (obj == null) throw new ArgumentNullException("obj");
            return IsAssignableFrom(contract, obj.GetContractBinding());
        }

        internal static bool IsCompatible(this IScriptContract contract, IScriptObject obj, out bool theSame)
        {
            return IsAssignableForm(contract, obj.GetContractBinding(), out theSame);
        }

        internal static IScriptObject BinaryOperation(this IScriptObject left, ExpressionType @operator, IScriptObject right, InterpreterState state)
        {
            if (left == null) throw new ArgumentNullException("left");
            switch (@operator)
            {
                case ExpressionType.Add:
                    return left.BinaryOperation(ScriptCodeBinaryOperatorType.Add, right, state);
                case ExpressionType.AddChecked:
                    return left.BinaryOperation(ScriptCodeBinaryOperatorType.Add, right, state.Update(InterpretationContext.Checked));
                case ExpressionType.And:
                    return left.BinaryOperation(ScriptCodeBinaryOperatorType.Intersection, right, state);
                case ExpressionType.Or:
                    return left.BinaryOperation(ScriptCodeBinaryOperatorType.Union, right, state);
                case ExpressionType.Coalesce:
                    return left.BinaryOperation(ScriptCodeBinaryOperatorType.Coalesce, right, state);
                case ExpressionType.Divide:
                    return left.BinaryOperation(ScriptCodeBinaryOperatorType.Divide, right, state);
                case ExpressionType.Equal:
                    return left.BinaryOperation(ScriptCodeBinaryOperatorType.ValueEquality, right, state);
                case ExpressionType.ExclusiveOr:
                    return left.BinaryOperation(ScriptCodeBinaryOperatorType.Exclusion, right, state);
                case ExpressionType.GreaterThan:
                    return left.BinaryOperation(ScriptCodeBinaryOperatorType.GreaterThan, right, state);
                case ExpressionType.GreaterThanOrEqual:
                    return left.BinaryOperation(ScriptCodeBinaryOperatorType.GreaterThanOrEqual, right, state);
                case ExpressionType.LessThan:
                    return left.BinaryOperation(ScriptCodeBinaryOperatorType.LessThan, right, state);
                case ExpressionType.LessThanOrEqual:
                    return left.BinaryOperation(ScriptCodeBinaryOperatorType.LessThanOrEqual, right, state);
                case ExpressionType.Modulo:
                    return left.BinaryOperation(ScriptCodeBinaryOperatorType.Modulo, right, state);
                case ExpressionType.Multiply:
                    return left.BinaryOperation(ScriptCodeBinaryOperatorType.Multiply, right, state);
                case ExpressionType.MultiplyChecked:
                    return left.BinaryOperation(ScriptCodeBinaryOperatorType.Multiply, right, state.Update(InterpretationContext.Checked));
                case ExpressionType.NotEqual:
                    return left.BinaryOperation(ScriptCodeBinaryOperatorType.ValueInequality, right, state);
                case ExpressionType.Subtract:
                    return left.BinaryOperation(ScriptCodeBinaryOperatorType.Subtract, right, state);
                case ExpressionType.SubtractChecked:
                    return left.BinaryOperation(ScriptCodeBinaryOperatorType.Subtract, right, state.Update(InterpretationContext.Checked));
                case ExpressionType.TypeAs:
                    return left.BinaryOperation(ScriptCodeBinaryOperatorType.TypeCast, right, state);
                default:
                    if (state.Context == InterpretationContext.Unchecked)
                        return left;
                    else throw new UnsupportedOperationException(state);
            }
        }

        internal static IScriptObject UnaryOperation(this IScriptObject operand, ExpressionType @operator, InterpreterState state)
        {
            if (operand == null) throw new ArgumentNullException("operand");
            switch (@operator)
            {
                case ExpressionType.Increment:
                case ExpressionType.PreIncrementAssign:
                    return operand.UnaryOperation(QCodeUnaryOperatorType.IncrementPrefix, state);
                case ExpressionType.PostIncrementAssign:
                    return operand.UnaryOperation(QCodeUnaryOperatorType.IncrementPostfix, state);
                case ExpressionType.Decrement:
                case ExpressionType.PreDecrementAssign:
                    return operand.UnaryOperation(QCodeUnaryOperatorType.DecrementPrefix, state);
                case ExpressionType.PostDecrementAssign:
                    return operand.UnaryOperation(QCodeUnaryOperatorType.DecrementPostfix, state);
                case ExpressionType.UnaryPlus:
                    return operand.UnaryOperation(QCodeUnaryOperatorType.Plus, state);
                case ExpressionType.Negate:
                    return operand.UnaryOperation(QCodeUnaryOperatorType.Minus, state);
                case ExpressionType.NegateChecked:
                    return operand.UnaryOperation(QCodeUnaryOperatorType.Minus, state.Update(InterpretationContext.Checked));
                case ExpressionType.Not:
                    return operand.UnaryOperation(QCodeUnaryOperatorType.Negate, state);
                default:
                    throw new UnsupportedOperationException(state);
            }
        }
        #endregion

        #region IRuntimeSlot Extensions

        internal static IScriptObject BinaryOperation(this IRuntimeSlot slot, ExpressionType @operator, IScriptObject right, InterpreterState state)
        {
            if (slot == null) throw new ArgumentNullException("slot");
            switch (@operator)
            {
                case ExpressionType.Assign:
                    return slot.SetValue(right, state);
                case ExpressionType.AddAssign:
                    return slot.SetValue(slot.GetValue(state).BinaryOperation(ScriptCodeBinaryOperatorType.Add, right, state), state);
                case ExpressionType.AddAssignChecked:
                    return slot.SetValue(slot.GetValue(state).BinaryOperation(ScriptCodeBinaryOperatorType.Add, right, state.Update(InterpretationContext.Checked)), state);
                case ExpressionType.SubtractAssign:
                    return slot.SetValue(slot.GetValue(state).BinaryOperation(ScriptCodeBinaryOperatorType.Subtract, right, state), state);
                case ExpressionType.SubtractAssignChecked:
                    return slot.SetValue(slot.GetValue(state).BinaryOperation(ScriptCodeBinaryOperatorType.Subtract, right, state.Update(InterpretationContext.Checked)), state);
                case ExpressionType.OrAssign:
                    return slot.SetValue(slot.GetValue(state).BinaryOperation(ScriptCodeBinaryOperatorType.Union, right, state), state);
                case ExpressionType.AndAssign:
                    return slot.SetValue(slot.GetValue(state).BinaryOperation(ScriptCodeBinaryOperatorType.Intersection, right, state), state);
                case ExpressionType.ModuloAssign:
                    return slot.SetValue(slot.GetValue(state).BinaryOperation(ScriptCodeBinaryOperatorType.Modulo, right, state), state);
                case ExpressionType.DivideAssign:
                    return slot.SetValue(slot.GetValue(state).BinaryOperation(ScriptCodeBinaryOperatorType.Divide, right, state), state);
                case ExpressionType.MultiplyAssign:
                    return slot.SetValue(slot.GetValue(state).BinaryOperation(ScriptCodeBinaryOperatorType.Multiply, right, state), state);
                case ExpressionType.MultiplyAssignChecked:
                    return slot.SetValue(slot.GetValue(state).BinaryOperation(ScriptCodeBinaryOperatorType.Multiply, right, state.Update(InterpretationContext.Checked)), state);
                default:
                    return BinaryOperation(slot.GetValue(state), @operator, right, state);
            }
        }

        internal static IScriptObject UnaryOperation(this IRuntimeSlot slot, ExpressionType @operator, InterpreterState state)
        {
            if (slot == null) throw new ArgumentNullException("slot");
            switch (@operator)
            {
                case ExpressionType.PreDecrementAssign:
                case ExpressionType.Decrement:
                    return slot.SetValue(slot.GetValue(state).UnaryOperation(QCodeUnaryOperatorType.DecrementPrefix, state), state);
                case ExpressionType.PostDecrementAssign:
                    var value = slot.GetValue(state);
                    slot.SetValue(value.UnaryOperation(QCodeUnaryOperatorType.DecrementPrefix, state), state);
                    return value;
                case ExpressionType.PreIncrementAssign:
                case ExpressionType.Increment:
                    return slot.SetValue(slot.GetValue(state).UnaryOperation(QCodeUnaryOperatorType.IncrementPrefix, state), state);
                case ExpressionType.PostIncrementAssign:
                    value = slot.GetValue(state);
                    slot.SetValue(value.UnaryOperation(QCodeUnaryOperatorType.IncrementPostfix, state), state);
                    return value;
                default:
                    return UnaryOperation(slot.GetValue(state), @operator, state);
            }
        }

        /// <summary>
        /// Stores value to the slot if it is possible.
        /// </summary>
        /// <param name="slot">The runtime slot. Cannot be <see langword="null"/>.</param>
        /// <param name="value">The value to store to the slot.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/> if <paramref name="value"/> is stored to the slot; otherwise, <see langword="false"/>.</returns>
        public static bool TrySetValue(this IStaticRuntimeSlot slot, IScriptObject value, InterpreterState state)
        {
            if (slot == null) throw new ArgumentNullException("slot");
            if (value == null) value = ScriptObject.Void;
            switch (slot.ContractBinding.IsAssignableFrom(value.GetContractBinding()))
            {
                case true:
                    slot.SetValue(value, state);
                    return true;
                default: return false;
            }
        }
        #endregion

        internal static MethodCallExpression BindIsTrue(Expression arg, ParameterExpression stateVar)
        {
            return LinqHelpers.Call<IScriptObject, InterpreterState, bool>((ob, state) => IsTrue(ob, state), null, arg, stateVar);
        }

        internal static MethodCallExpression BindIsFalse(Expression arg, ParameterExpression stateVar)
        {
            return LinqHelpers.Call<IScriptObject, InterpreterState, bool>((ob, state) => IsFalse(ob, state), null, arg, stateVar);
        }

        internal static InterpreterState GetState(this CallSiteBinder binder)
        {
            return binder is IScriptRuntimeBinder ? ((IScriptRuntimeBinder)binder).State : InterpreterState.Current;
        }

        /// <summary>
        /// Determines whether the specified parameter represents runtime variable.
        /// </summary>
        /// <param name="p">The parameteter to be checked.</param>
        /// <returns><see langword="true"/> if the specified parameter represents runtime variable; otherwise, <see langword="false"/>.</returns>
        public static bool IsRuntimeVariable(this Expression p)
        {
            return p != null && typeof(IStaticRuntimeSlot).IsAssignableFrom(p.Type);
        }

        /// <summary>
        /// Adds a new item to the collection if the specified condition is <see langword="true"/>.
        /// </summary>
        /// <typeparam name="T">Type of the items in the collection.</typeparam>
        /// <param name="collection">The collection to be modified. Cannot be <see langword="null"/>.</param>
        /// <param name="condition">The result of the condition evaluation.</param>
        /// <param name="item">An item to be added to the collection.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
        public static bool AddIf<T>(this ICollection<T> collection, bool condition, T item)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            switch (condition)
            {
                case true:
                    collection.Add(item);
                    return true;
                default:
                    return false;
            }
        }

        internal static Uri[] PrepareScriptFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return new Uri[0];
            var result = default(Uri);
            switch (Uri.TryCreate(path, UriKind.Absolute, out result))
            {
                case true: return new[] { result };
                default:
                    var entryAssembly = Assembly.GetEntryAssembly();
                    return entryAssembly != null ? new[]
                        {
                            new Uri(Path.Combine(Path.GetDirectoryName(entryAssembly.Location), path), UriKind.Absolute),
                            new Uri(Path.Combine(SystemEnvironment.CurrentDirectory, path), UriKind.Absolute)
                        } :
                        new[] { new Uri(Path.Combine(SystemEnvironment.CurrentDirectory, path), UriKind.Absolute) };
            }
        }

        internal static void RunClassConstructor<T>()
        {
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
        }

        internal static IScriptObject[] GetValues(this IRuntimeSlot[] slots, InterpreterState state)
        {
            var result = new IScriptObject[slots.LongLength];
            Parallel.For(0, result.LongLength, i => result[i] = slots[i].GetValue(state));
            return result;
        }

        internal static IScriptContract[] GetContractBindings(this IList<IScriptObject> values)
        {
            var result = new IScriptContract[values.Count];
            Parallel.For(0, values.Count, i => result[i] = values[i].GetContractBinding());
            return result;
        }
    }
}