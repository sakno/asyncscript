﻿using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptCodeBinaryOperatorType = Compiler.Ast.ScriptCodeBinaryOperatorType;

    [ComVisible(false)]
    interface IBinaryOperatorInvoker: IScriptFunction
    {
        ScriptCodeBinaryOperatorType Operator { get; }
    }
}