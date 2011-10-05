using System;
using System.Collections.Generic;
using System.IO;

namespace DynamicScript.Runtime.Hosting
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Resources = Properties.Resources;

    [ComVisible(false)]
    sealed class ShowHelpCommand: ICommand
    {
        public int Execute(TextWriter output, TextReader input)
        {
            output.WriteLine(Resources.Usage);
            return InvalidCommand.Success;
        }
    }
}
