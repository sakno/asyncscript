using System;

namespace DynamicScript.Runtime.Hosting
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Encoding = System.Text.Encoding;

    [ComVisible(false)]
    sealed class ScriptConsoleOutput: DynamicScriptOutput
    {
        public override void Write(IScriptObject value)
        {
            Console.Out.Write(value);
        }

        public override void WriteLine(IScriptObject value)
        {
            Console.Out.WriteLine(value);
        }

        public override Encoding Encoding
        {
            get
            {
                return Console.OutputEncoding;
            }
        }

        public override void Write(bool value)
        {
            Console.Out.Write(value);
        }

        public override void Write(char value)
        {
            Console.Out.Write(value);
        }

        public override void Write(char[] buffer)
        {
            Console.Out.Write(buffer);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            Console.Out.Write(buffer, index, count);
        }

        public override void Write(decimal value)
        {
            Console.Out.Write(value);
        }

        public override void Write(double value)
        {
            Console.Out.Write(value);
        }

        public override void Write(float value)
        {
            Console.Out.Write(value);
        }

        public override void Write(int value)
        {
            Console.Out.Write(value);
        }

        public override void Write(long value)
        {
            Console.Out.Write(value);
        }

        public override void Write(object value)
        {
            Console.Out.Write(value);
        }

        public override void Write(string format, object arg0)
        {
            Console.Out.Write(format, arg0);
        }

        public override void Write(string format, object arg0, object arg1)
        {
            Console.Out.Write(format, arg0, arg1);
        }

        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            Console.Out.Write(format, arg0, arg1, arg2);
        }

        public override void Write(string format, params object[] arg)
        {
            Console.Out.Write(format, arg);
        }

        public override void Write(string value)
        {
            Console.Out.Write(value);
        }

        public override void Write(uint value)
        {
            Console.Out.Write(value);
        }

        public override void Write(ulong value)
        {
            Console.Out.Write(value);
        }

        public override void WriteLine()
        {
            Console.Out.WriteLine();
        }

        public override void WriteLine(bool value)
        {
            Console.Out.WriteLine(value);
        }

        public override void WriteLine(char value)
        {
            Console.Out.WriteLine(value);
        }

        public override void WriteLine(char[] buffer)
        {
            Console.Out.WriteLine(buffer);
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            Console.Out.WriteLine(buffer, index, count);
        }

        public override void WriteLine(decimal value)
        {
            Console.Out.WriteLine(value);
        }

        public override void WriteLine(double value)
        {
            Console.Out.WriteLine(value);
        }

        public override void WriteLine(float value)
        {
            Console.Out.WriteLine(value);
        }

        public override void WriteLine(int value)
        {
            Console.Out.WriteLine(value);
        }

        public override void WriteLine(long value)
        {
            Console.Out.WriteLine(value);
        }

        public override void WriteLine(object value)
        {
            Console.Out.WriteLine(value);
        }

        public override void WriteLine(string format, object arg0)
        {
            Console.Out.WriteLine(format, arg0);
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            Console.Out.WriteLine(format, arg0, arg1);
        }

        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            Console.Out.WriteLine(format, arg0, arg1, arg2);
        }

        public override void WriteLine(string format, params object[] arg)
        {
            Console.Out.WriteLine(format, arg);
        }

        public override void WriteLine(string value)
        {
            Console.Out.WriteLine(value);
        }

        public override void WriteLine(uint value)
        {
            Console.Out.WriteLine(value);
        }

        public override void WriteLine(ulong value)
        {
            Console.Out.WriteLine(value);
        }

        public void Clear()
        {
            Console.Clear();
        }
    }
}
