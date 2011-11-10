using System;
using System.Collections.Generic;
using System.IO;

namespace DynamicScript.Runtime.Hosting
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Encoding = System.Text.Encoding;

    [ComVisible(false)]
    static class Program
    {
        private static int Execute(CommandLineParser cmd, string[] args)
        {
            return cmd.Run(args);
        }

        [LoaderOptimization(LoaderOptimization.SingleDomain)]
        private static int Main(string[] args)
        {
            using (var output = new FileStream("output.log", FileMode.Append, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(output, Encoding.UTF8))
            using (var reader = new EmptyTextReader())
                return Execute(new CommandLineParser(writer, reader), args);
        }
    }
}
