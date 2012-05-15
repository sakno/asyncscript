using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment.ObjectModel
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using NativeRegex = System.Text.RegularExpressions.Regex;
    using RegexOptions = System.Text.RegularExpressions.RegexOptions;
    using RegexMatch = System.Text.RegularExpressions.Match;
    using RegexCapture = System.Text.RegularExpressions.Capture;
    using RegexGroup = System.Text.RegularExpressions.Group;
    using SystemConverter=System.Convert;

    /// <summary>
    /// Represents DynamicScript regular expression.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class Regex: ScriptCompositeObject
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class RegexProxy
        {
            private RegexOptions m_options = RegexOptions.None;

            public bool IgnoreCase
            {
                get { return m_options.HasFlag(RegexOptions.IgnoreCase); }
                set
                {
                    switch (value)
                    {
                        case true: m_options |= RegexOptions.IgnoreCase; return;
                        default: m_options &= ~RegexOptions.IgnoreCase; return;
                    }
                }
            }

            public bool Multiline
            {
                get { return m_options.HasFlag(RegexOptions.Multiline); }
                set
                {
                    switch (value)
                    {
                        case true: m_options |= RegexOptions.Multiline; return;
                        default: m_options &= ~RegexOptions.Multiline; return;
                    }
                }
            }

            public bool Singleline
            {
                get { return m_options.HasFlag(RegexOptions.Singleline); }
                set
                {
                    switch (value)
                    {
                        case true: m_options |= RegexOptions.Singleline; return;
                        default: m_options &= ~RegexOptions.Singleline; return;
                    }
                }
            }

            public bool IgnorePatternWhitespace
            {
                get { return m_options.HasFlag(RegexOptions.IgnorePatternWhitespace); }
                set
                {
                    switch (value)
                    {
                        case true: m_options |= RegexOptions.IgnorePatternWhitespace; return;
                        default: m_options &= ~RegexOptions.IgnorePatternWhitespace; return;
                    }
                }
            }

            public bool RightToLeft
            {
                get { return m_options.HasFlag(RegexOptions.RightToLeft); }
                set
                {
                    switch (value)
                    {
                        case true: m_options |= RegexOptions.RightToLeft; return;
                        default: m_options &= ~RegexOptions.RightToLeft; return;
                    }
                }
            }

            public bool CultureInvariant
            {
                get { return m_options.HasFlag(RegexOptions.CultureInvariant); }
                set
                {
                    switch (value)
                    {
                        case true: m_options |= RegexOptions.CultureInvariant; return;
                        default: m_options &= ~RegexOptions.CultureInvariant; return;
                    }
                }
            }

            public bool IsMatch(string input, string pattern)
            {
                return NativeRegex.IsMatch(input, pattern, m_options);
            }

            public string Replace(string input, string pattern, string replacement)
            {
                return NativeRegex.Replace(input, pattern, replacement, m_options);
            }

            public string[] Split(string input, string pattern)
            {
                return NativeRegex.Split(input, pattern, m_options);
            }

            public RegexMatch Match(string input, string pattern)
            {
                return NativeRegex.Match(input, pattern, m_options);
            }
        }

        [ComVisible(false)]
        private sealed class SplitFunction : ScriptFunc<ScriptString, ScriptString>
        {
            public const string Name = "split";
            private const string FirstParamName="input";
            private const string SecondParamName="pattern";

            private readonly RegexProxy m_proxy;

            public SplitFunction(RegexProxy proxy)
                : base(FirstParamName, ScriptStringContract.Instance, SecondParamName, ScriptStringContract.Instance, ScriptStringContract.Instance)
            {
                m_proxy = proxy;
            }

            protected override IScriptObject Invoke(ScriptString a, ScriptString b, InterpreterState state)
            {
                return ScriptArray.Create(m_proxy.Split(a, b));
            }
        }

        [ComVisible(false)]
        private sealed class ReplaceFunction : ScriptFunc<ScriptString, ScriptString, ScriptString>
        {
            public const string Name = "replace";
            private const string FirstParamName = "input";
            private const string SecondParamName = "pattern";
            private const string ThirdParamName = "replacement";

            private readonly RegexProxy m_proxy;

            public ReplaceFunction(RegexProxy proxy)
                : base(FirstParamName, ScriptStringContract.Instance, SecondParamName, ScriptStringContract.Instance, ThirdParamName, ScriptStringContract.Instance, ScriptStringContract.Instance)
            {
                m_proxy = proxy;
            }

            public override IScriptObject Invoke(ScriptString input, ScriptString pattern, ScriptString replacement, InterpreterState state)
            {
                return (ScriptString)m_proxy.Replace(input, pattern, replacement);
            }
        }

        [ComVisible(false)]
        private sealed class IsMatchFunction : ScriptFunc<ScriptString, ScriptString>
        {
            public const string Name = "ismatch";
            private const string FirstParamName="input";
            private const string SecondParamName="pattern";

            private readonly RegexProxy m_proxy;

            public IsMatchFunction(RegexProxy proxy)
                : base(FirstParamName, ScriptStringContract.Instance, SecondParamName, ScriptStringContract.Instance, ScriptBooleanContract.Instance)
            {
                m_proxy = proxy;
            }

            protected override IScriptObject Invoke(ScriptString input, ScriptString pattern, InterpreterState state)
            {
                return (ScriptBoolean)m_proxy.IsMatch(input, pattern);
            }
        }

        [ComVisible(false)]
        private abstract class RegexOptionSlot : RuntimeSlotBase, IStaticRuntimeSlot
        {
            protected readonly RegexProxy Proxy;

            protected RegexOptionSlot(RegexProxy proxy)
            {
                Proxy = proxy;
            }

            public abstract ScriptBoolean Value
            {
                get;
                set;
            }

            public sealed override IScriptObject GetValue(InterpreterState state)
            {
                return Value;
            }

            public sealed override IScriptObject SetValue(IScriptObject value, InterpreterState state)
            {
                if (ScriptBooleanContract.TryConvert(ref value)) Value = (ScriptBoolean)value;
                return value;
            }

            public IScriptContract ContractBinding
            {
                get { return ScriptBooleanContract.Instance; }
            }

            public sealed override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.None; }
            }

            public sealed override bool DeleteValue()
            {
                return false;
            }
        }

        [ComVisible(false)]
        private sealed class IgnoreCaseSlot : RegexOptionSlot
        {
            public const string Name = "IgnoreCase";

            public IgnoreCaseSlot(RegexProxy proxy)
                :base(proxy)
            {
            }

            public override ScriptBoolean Value
            {
                get
                {
                    return Proxy.IgnoreCase;
                }
                set
                {
                    Proxy.IgnoreCase = value;
                }
            }
        }

        [ComVisible(false)]
        private sealed class MultilineSlot : RegexOptionSlot
        {
            public const string Name = "Multiline";

            public MultilineSlot(RegexProxy proxy)
                : base(proxy)
            {
            }

            public override ScriptBoolean Value
            {
                get
                {
                    return Proxy.Multiline;
                }
                set
                {
                    Proxy.Multiline = value;
                }
            }
        }

        [ComVisible(false)]
        private sealed class SinglelineSlot : RegexOptionSlot
        {
            public const string Name = "Singleline";

            public SinglelineSlot(RegexProxy proxy)
                : base(proxy)
            {
            }

            public override ScriptBoolean Value
            {
                get
                {
                    return Proxy.Singleline;
                }
                set
                {
                    Proxy.Singleline = value;
                }
            }
        }

        [ComVisible(false)]
        private sealed class IgnorePatternWhitespaceSlot : RegexOptionSlot
        {
            public const string Name = "IgnorePatternWhitespace";

            public IgnorePatternWhitespaceSlot(RegexProxy proxy)
                : base(proxy)
            {
            }

            public override ScriptBoolean Value
            {
                get
                {
                    return Proxy.IgnorePatternWhitespace;
                }
                set
                {
                    Proxy.IgnorePatternWhitespace = value;
                }
            }
        }

        [ComVisible(false)]
        private sealed class RightToLeftSlot : RegexOptionSlot
        {
            public const string Name = "RightToLeft";

            public RightToLeftSlot(RegexProxy proxy)
                : base(proxy)
            {
            }

            public override ScriptBoolean Value
            {
                get
                {
                    return Proxy.RightToLeft;
                }
                set
                {
                    Proxy.RightToLeft = value;
                }
            }
        }

        [ComVisible(false)]
        private sealed class CultureInvariantSlot : RegexOptionSlot
        {
            public const string Name = "CultureInvariant";

            public CultureInvariantSlot(RegexProxy proxy)
                : base(proxy)
            {
            }

            public override ScriptBoolean Value
            {
                get
                {
                    return Proxy.CultureInvariant;
                }
                set
                {
                    Proxy.CultureInvariant = value;
                }
            }
        }

        [ComVisible(false)]
        private new sealed class Slots : ObjectSlotCollection
        {
            public Slots()
            {
                var proxy = new RegexProxy();
                AddConstant(IsMatchFunction.Name, new IsMatchFunction(proxy));
                Add(MultilineSlot.Name, new MultilineSlot(proxy));
                Add(IgnoreCaseSlot.Name, new IgnoreCaseSlot(proxy));
                Add(SinglelineSlot.Name, new SinglelineSlot(proxy));
                Add(IgnorePatternWhitespaceSlot.Name, new IgnorePatternWhitespaceSlot(proxy));
                Add(RightToLeftSlot.Name, new RightToLeftSlot(proxy));
                Add(CultureInvariantSlot.Name, new CultureInvariantSlot(proxy));
                AddConstant(ReplaceFunction.Name, new ReplaceFunction(proxy));
                AddConstant(SplitFunction.Name, new SplitFunction(proxy));
                AddConstant(MatchAction.Name, new MatchAction(proxy));
            }
        }

        [ComVisible(false)]
        private class ScriptMatchCapture : ScriptCompositeObject
        {
            private const string IndexSlot = "index";
            private const string LengthSlot = "length";
            private const string ValueSlot = "value";

            [ComVisible(false)]
            protected new class Slots : ObjectSlotCollection
            {
                public Slots(RegexCapture capture)
                {
                    if(capture==null)throw new ArgumentNullException("capture");
                    AddConstant(IndexSlot, new ScriptInteger(capture.Index));
                    AddConstant(LengthSlot, new ScriptInteger(capture.Length));
                    AddConstant(ValueSlot, new ScriptString(capture.Value));
                }
            }

            protected ScriptMatchCapture(Slots slots)
                : base(slots)
            {
            }

            public ScriptMatchCapture(RegexCapture capture)
                : this(new Slots(capture))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetCaptureAction : ScriptFunc<ScriptInteger>
        {
            public const string Name = "getcap";
            private const string FirstParamName = "idx";

            private readonly RegexGroup m_group;

            public GetCaptureAction(RegexGroup group)
                : base(FirstParamName, ScriptIntegerContract.Instance, ScriptSuperContract.Instance)
            {
                m_group = group;
            }

            private static IScriptObject Invoke(RegexGroup group, int index)
            {
                return index >= 0 && index < group.Captures.Count ? new ScriptMatchCapture(group.Captures[index]) : null;
            }

            protected override IScriptObject Invoke(ScriptInteger index, InterpreterState state)
            {
                return index.IsInt32 ? Invoke(m_group, SystemConverter.ToInt32(index)) : null;
            }
        }

        [ComVisible(false)]
        private class ScriptMatchGroup : ScriptMatchCapture
        {
            private const string SuccessSlot = "success";
            private const string CapturesSlot = "captures";

            [ComVisible(false)]
            protected new class Slots : ScriptMatchCapture.Slots
            {
                public Slots(RegexGroup group)
                    : base(group)
                {
                    AddConstant(SuccessSlot, (ScriptBoolean)group.Success);
                    AddConstant(CapturesSlot, new ScriptInteger(group.Captures.Count));
                    AddConstant(GetCaptureAction.Name, new GetCaptureAction(group));
                }
            }

            protected ScriptMatchGroup(Slots slots)
                : base(slots)
            {
            }

            public ScriptMatchGroup(RegexGroup group)
                : this(new Slots(group))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetGroupAction : ScriptFunc<ScriptString>
        {
            public const string Name = GetItemAction;
            private const string FirstParamName = "groupName";

            private readonly RegexMatch m_match;

            public GetGroupAction(RegexMatch match)
                : base(FirstParamName, ScriptStringContract.Instance, ScriptSuperContract.Instance)
            {
                m_match = match;
            }

            protected override IScriptObject Invoke(ScriptString groupName, InterpreterState state)
            {
                var group = m_match.Groups[groupName ?? ScriptString.Empty];
                return group != null ? new ScriptMatchGroup(group) : null;
            }
        }

        [ComVisible(false)]
        private sealed class ScriptRegexMatch : ScriptMatchGroup
        {
            [ComVisible(false)]
            private new sealed class Slots : ScriptMatchGroup.Slots
            {
                public Slots(RegexMatch match)
                    :base(match)
                {
                    AddConstant(GetGroupAction.Name, new GetGroupAction(match));
                    AddConstant(GetCaptureAction.Name, new GetCaptureAction(match));
                }
            }

            public ScriptRegexMatch(RegexMatch match)
                : base(new Slots(match))
            {
            }
        }

        [ComVisible(false)]
        private sealed class MatchAction : ScriptFunc<ScriptString, ScriptString>
        {
            public const string Name = "match";
            private const string FirstParamName = "input";
            private const string SecondParamName = "pattern";

            private readonly RegexProxy m_proxy;

            public MatchAction(RegexProxy proxy)
                : base(FirstParamName, ScriptStringContract.Instance, SecondParamName, ScriptStringContract.Instance, ScriptSuperContract.Instance)
            {
                m_proxy = proxy;
            }

            protected override IScriptObject Invoke(ScriptString input, ScriptString pattern, InterpreterState state)
            {
                return new ScriptRegexMatch(m_proxy.Match(input, pattern));
            }
        }
        #endregion

        /// <summary>
        /// Initializes a new regular expression.
        /// </summary>
        public Regex()
            : base(new Slots())
        {
        }
    }
}
