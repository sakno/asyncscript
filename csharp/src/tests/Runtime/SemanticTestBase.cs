using System;
using System.Collections.Generic;
using DynamicScript.Testing;

namespace DynamicScript.Runtime
{
    using SemanticTestAttribute = DynamicScript.Testing.SemanticTestAttribute;

    [SemanticTest]
    abstract class SemanticTestBase
    {
        public static dynamic Run(IEnumerable<char> sourceCode, params string[] args)
        {
            return DynamicScriptInterpreter.Run(sourceCode, args);
        }
    }
}
