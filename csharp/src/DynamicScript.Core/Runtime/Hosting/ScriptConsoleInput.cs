using System;

namespace DynamicScript.Runtime.Hosting
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptObject = Environment.ScriptObject;

    [ComVisible(false)]
    sealed class ScriptConsoleInput: DynamicScriptInput
    {
        public override void ReadLine(out IScriptObject obj)
        {
            obj = ScriptObject.Convert(ReadLine());
        }

        public override int Peek()
        {
            return Console.In.Peek();
        }

        public override int Read()
        {
            return Console.In.Read();
        }

        public override int Read(char[] buffer, int index, int count)
        {
            return Console.In.Read(buffer, index, count);
        }

        public override int ReadBlock(char[] buffer, int index, int count)
        {
            return Console.In.ReadBlock(buffer, index, count);
        }

        public override string ReadLine()
        {
            return Console.In.ReadLine();
        }

        public override string ReadToEnd()
        {
            return Console.In.ReadToEnd();
        }
    }
}
