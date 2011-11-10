using System;
using System.IO;

namespace DynamicScript.Runtime.Hosting
{
    using ScriptObject = Runtime.Environment.ScriptObject;

    sealed class EmptyTextReader: DynamicScriptInput
    {

        public override void ReadLine(out IScriptObject obj)
        {
            obj = ScriptObject.Void;
        }
    }
}
