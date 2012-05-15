using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment.ObjectModel
{
    using SystemMath = System.Math;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using InliningSourceAttribute = Compiler.Ast.Translation.LinqExpressions.InliningSourceAttribute;

    /// <summary>
    /// Represents mathematic DynamicScript library.
    /// </summary>
    [CLSCompliant(false)]
    [ComVisible(false)]
    public sealed class Math: ScriptCompositeObject
    {
        internal const string Name = "math";
        private const string PiSlotName = "pi";
        private const string ESlotName = "e";

        #region Nested Types
        [ComVisible(false)]
        private sealed class ShlAction : ScriptFunc<ScriptInteger, ScriptInteger>
        {
            public const string Name = "shl";
            private const string FirstParamName = "value";
            private const string SecondParamName = "shift";

            public ShlAction()
                : base(FirstParamName, ScriptIntegerContract.Instance, SecondParamName, ScriptIntegerContract.Instance, ScriptIntegerContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptInteger value, ScriptInteger shift, InterpreterState state)
            {
                return Shl(value, shift, state);
            }
        }

        [ComVisible(false)]
        private sealed class ShrAction : ScriptFunc<ScriptInteger, ScriptInteger>
        {
            public const string Name = "shr";
            private const string FirstParamName = "value";
            private const string SecondParamName = "shift";

            public ShrAction()
                : base(FirstParamName, ScriptIntegerContract.Instance, SecondParamName, ScriptIntegerContract.Instance, ScriptIntegerContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptInteger value, ScriptInteger shift, InterpreterState state)
            {
                return Shr(value, shift, state);
            }
        }

        [ComVisible(false)]
        private sealed class SinAction : ScriptFunc<ScriptReal>
        {
            public const string Name = "sin";
            private const string FirstParamName = "a";

            public SinAction()
                : base(FirstParamName, ScriptRealContract.Instance, ScriptRealContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptReal a, InterpreterState state)
            {
                return Sin(a, state);
            }
        }

        [ComVisible(false)]
        private sealed class SinhAction : ScriptFunc<ScriptReal>
        {
            public const string Name = "sinh";
            private const string FirstParamName = "a";

            public SinhAction()
                : base(FirstParamName, ScriptRealContract.Instance, ScriptRealContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptReal a, InterpreterState state)
            {
                return Sinh(a, state);
            }
        }

        [ComVisible(false)]
        private sealed class DegAction : ScriptFunc<ScriptReal>
        {
            public const string Name = "deg";
            private const string FirstParamName = "a";

            public DegAction()
                : base(FirstParamName, ScriptRealContract.Instance, ScriptRealContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptReal a, InterpreterState state)
            {
                return Deg(a, state);
            }
        }

        [ComVisible(false)]
        private sealed class RadAction : ScriptFunc<ScriptReal>
        {
            public const string Name = "rad";
            private const string FirstParamName = "a";

            public RadAction()
                : base(FirstParamName, ScriptRealContract.Instance, ScriptRealContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptReal a, InterpreterState state)
            {
                return Rad(a, state);
            }
        }

        [ComVisible(false)]
        private sealed class CosAction : ScriptFunc<ScriptReal>
        {
            public const string Name = "cos";
            private const string FirstParamName = "a";

            public CosAction()
                : base(FirstParamName, ScriptRealContract.Instance, ScriptRealContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptReal a, InterpreterState state)
            {
                return Cos(a, state);
            }
        }

        [ComVisible(false)]
        private sealed class CoshAction : ScriptFunc<ScriptReal>
        {
            public const string Name = "cosh";
            private const string FirstParamName = "a";

            public CoshAction()
                : base(FirstParamName, ScriptRealContract.Instance, ScriptRealContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptReal a, InterpreterState state)
            {
                return Cosh(a, state);
            }
        }

        [ComVisible(false)]
        private sealed class FloorAction : ScriptFunc<ScriptReal>
        {
            public const string Name = "floor";
            private const string FirstParamName = "a";

            public FloorAction()
                : base(FirstParamName, ScriptRealContract.Instance, ScriptIntegerContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptReal a, InterpreterState state)
            {
                return Floor(a, state);
            }
        }

        [ComVisible(false)]
        private sealed class CeilingAction : ScriptFunc<ScriptReal>
        {
            public const string Name = "ceiling";
            private const string FirstParamName = "a";

            public CeilingAction()
                : base(FirstParamName, ScriptRealContract.Instance, ScriptIntegerContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptReal a, InterpreterState state)
            {
                return Ceiling(a, state);
            }
        }

        [ComVisible(false)]
        private sealed class TanAction : ScriptFunc<ScriptReal>
        {
            public const string Name = "tan";
            private const string FirstParamName = "a";

            public TanAction()
                : base(FirstParamName, ScriptRealContract.Instance, ScriptRealContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptReal a, InterpreterState state)
            {
                return Tan(a, state);
            }
        }

        [ComVisible(false)]
        private sealed class TanhAction : ScriptFunc<ScriptReal>
        {
            public const string Name = "tanh";
            private const string FirstParamName = "a";

            public TanhAction()
                : base(FirstParamName, ScriptRealContract.Instance, ScriptRealContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptReal a, InterpreterState state)
            {
                return Tanh(a, state);
            }
        }

        [ComVisible(false)]
        private sealed class AtanAction : ScriptFunc<ScriptReal>
        {
            public const string Name = "atan";
            private const string FirstParamName = "a";

            public AtanAction()
                : base(FirstParamName, ScriptRealContract.Instance, ScriptRealContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptReal d, InterpreterState state)
            {
                return Atan(d, state);
            }
        }

        [ComVisible(false)]
        private sealed class AcosAction : ScriptFunc<ScriptReal>
        {
            public const string Name = "acos";
            private const string FirstParamName = "a";

            public AcosAction()
                : base(FirstParamName, ScriptRealContract.Instance, ScriptRealContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptReal d, InterpreterState state)
            {
                return Acos(d, state);
            }
        }

        [ComVisible(false)]
        private sealed class AsinAction : ScriptFunc<ScriptReal>
        {
            public const string Name = "asin";
            private const string FirstParamName = "a";

            public AsinAction()
                : base(FirstParamName, ScriptRealContract.Instance, ScriptRealContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptReal d, InterpreterState state)
            {
                return Asin(d, state);
            }
        }

        [ComVisible(false)]
        private sealed class TruncAction : ScriptFunc<ScriptReal>
        {
            public const string Name = "trunc";
            private const string FirstParamName = "d";

            public TruncAction()
                : base(FirstParamName, ScriptRealContract.Instance, ScriptIntegerContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptReal value, InterpreterState state)
            {
                return Trunc(value, state);
            }
        }
        #endregion

        private static new IEnumerable<KeyValuePair<string, IStaticRuntimeSlot>> Slots
        {
            get
            {
                yield return Constant(SinAction.Name, new SinAction());
                yield return Constant(SinhAction.Name, new SinhAction());
                yield return Constant(PiSlotName, Math.PI, ScriptRealContract.Instance);
                yield return Constant(ESlotName, Math.E, ScriptRealContract.Instance);
                yield return Constant(DegAction.Name, new DegAction());
                yield return Constant(RadAction.Name, new RadAction());
                yield return Constant(CoshAction.Name, new CoshAction());
                yield return Constant(CosAction.Name, new CosAction());
                yield return Constant(TanAction.Name, new TanAction());
                yield return Constant(TanhAction.Name, new TanhAction());
                yield return Constant(TruncAction.Name, new TruncAction());
                yield return Constant(FloorAction.Name, new FloorAction());
                yield return Constant(CeilingAction.Name, new CeilingAction());
                yield return Constant(AsinAction.Name, new AsinAction());
                yield return Constant(AcosAction.Name, new AcosAction());
                yield return Constant(AtanAction.Name, new AtanAction());
                yield return Constant(ShlAction.Name, new ShlAction());
                yield return Constant(ShrAction.Name, new ShrAction());
            }
        }

        /// <summary>
        /// Initializes a new DynamicScript Math library.
        /// </summary>
        private Math()
            : base(Slots)
        {
        }

        /// <summary>
        /// Represents singleton instance of this object.
        /// </summary>
        public static readonly Math Instance = new Math();

        /// <summary>
        /// Shifts the first value right by the number of bits specified by its value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="shift"></param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptInteger Shr(ScriptInteger value, ScriptInteger shift, InterpreterState state)
        {
            return value >> (int)shift;
        }

        /// <summary>
        /// Shifts the first value left by the number of bits specified by its value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="shift"></param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptInteger Shl(ScriptInteger value, ScriptInteger shift, InterpreterState state)
        {
            return value << (int)shift;
        }

        /// <summary>
        /// Returns the angle whose tangent is the specified number.
        /// </summary>
        /// <param name="d">A number representing a tangent.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptReal Atan(ScriptReal d, InterpreterState state)
        {
            return SystemMath.Atan(d);
        }

        /// <summary>
        /// Returns the angle whose sine is the specified number.
        /// </summary>
        /// <param name="d">A number representing a sine, where -1 ≤d≤ 1.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptReal Asin(ScriptReal d, InterpreterState state)
        {
            return SystemMath.Asin(d);
        }

        /// <summary>
        /// Returns the angle whose cosine is the specified number.
        /// </summary>
        /// <param name="d">A number representing a cosine, where -1 ≤d≤ 1.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptReal Acos(ScriptReal d, InterpreterState state)
        {
            return SystemMath.Acos(d);
        }

        /// <summary>
        /// Returns the smallest integral value that is greater than or equal to the
        /// specified double-precision floating-point number.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptInteger Ceiling(ScriptReal a, InterpreterState state)
        {
            return (long)SystemMath.Ceiling(a);
        }

        /// <summary>
        /// Returns the largest integer less than or equal to the specified decimal number.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptInteger Floor(ScriptReal a, InterpreterState state)
        {
            return (long)SystemMath.Floor(a);
        }

        /// <summary>
        /// Returns the sine of the specified angle.
        /// </summary>
        /// <param name="a">An angle, measured in radians.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The sine of the specified angle.</returns>
        [InliningSource]
        public static ScriptReal Sin(ScriptReal a, InterpreterState state)
        {
            return SystemMath.Sin(a);
        }

        /// <summary>
        /// Returns the hyperbolic sine of the specified angle.
        /// </summary>
        /// <param name="a">An angle, measured in radians.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The hyperbolic sine of the specified angle.</returns>
        [InliningSource]
        public static ScriptReal Sinh(ScriptReal a, InterpreterState state)
        {
            return SystemMath.Sinh(a);
        }

        /// <summary>
        /// Returns the cosine of the specified angle.
        /// </summary>
        /// <param name="a">An angle, measured in radians.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The cosine of the specified angle.</returns>
        [InliningSource]
        public static ScriptReal Cos(ScriptReal a, InterpreterState state)
        {
            return SystemMath.Cos(a);
        }

        /// <summary>
        /// Returns the hyperbolic cosine of the specified angle.
        /// </summary>
        /// <param name="a">An angle, measured in radians.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The hyperbolic cosine of the specified angle.</returns>
        [InliningSource]
        public static ScriptReal Cosh(ScriptReal a, InterpreterState state)
        {
            return SystemMath.Cosh(a);
        }

        /// <summary>
        /// Returns the tangent of the specified angle.
        /// </summary>
        /// <param name="a">An angle, measured in radians.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The tangent of the specified angle.</returns>
        [InliningSource]
        public static ScriptReal Tan(ScriptReal a, InterpreterState state)
        {
            return SystemMath.Tan(a);
        }

        /// <summary>
        /// Returns the hyperbolic tangent of the specified angle.
        /// </summary>
        /// <param name="a">An angle, measured in radians.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The hyperbolic tangent of the specified angle.</returns>
        [InliningSource]
        public static ScriptReal Tanh(ScriptReal a, InterpreterState state)
        {
            return SystemMath.Tanh(a);
        }

        /// <summary>
        /// Calculates integral part of a specified floating-point number.
        /// </summary>
        /// <param name="d">A number to truncate.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns> The integral part of <paramref name="d"/>; that is, the number that remains after any fractional
        ///     digits have been discarded.</returns>
        [InliningSource]
        public static ScriptInteger Trunc(ScriptReal d, InterpreterState state)
        {
            return (long)SystemMath.Truncate(d);
        }

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        /// <param name="a">An angle, measured in radians.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>An angle, measured in degrees.</returns>
        [InliningSource]
        public static ScriptReal Deg(ScriptReal a, InterpreterState state)
        {
            return a * 180 / SystemMath.PI;
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="a">An angle, measured in degrees.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>An angle, measured in radians.</returns>
        [InliningSource]
        public static ScriptReal Rad(ScriptReal a, InterpreterState state)
        {
            return a * SystemMath.PI / 180;
        }

        /// <summary>
        /// Represents the ratio of the circumference of a circle to its diameter, specified by the constant, π.
        /// </summary>
        public static readonly ScriptReal PI = new ScriptReal(SystemMath.PI);

        /// <summary>
        /// Represents the natural logarithmic base, specified by the constant, e.
        /// </summary>
        public static readonly ScriptReal E = new ScriptReal(SystemMath.E);
    }
}
