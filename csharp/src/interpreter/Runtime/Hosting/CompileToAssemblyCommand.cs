using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DynamicScript.Runtime.Hosting
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Resources = Properties.Resources;
    using Win32Exception = System.ComponentModel.Win32Exception;

    /// <summary>
    /// Represents command that is used to compile script.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class CompileToAssemblyCommand: ICommand
    {
        //Command identifiers.
        public const string CommandName = "/c";
        public const string CommandNameAlt = "-c";
        //name command
        private const string NameCommandName = "/name";
        private const string NameCommandNameAlt = "-name";
        //version command
        private const string VersionCommandName = "/ver";
        private const string VersionCommandNameAlt = "-ver";
        //debug command
        private const string DebugCommandName = "/debug";
        private const string DebugCommandNameAlt = "-debug";
        //exe command
        private const string ExecutableCommandName = "/exec";
        private const string ExecutableCommandNameAlt = "-exec";
        //x86 command
        private const string X86CommandName = "/x86";
        private const string X86CommandNameAlt = "-x86";
        //il command
        private const string ILCommandName = "/il";
        private const string ILCommandNameAlt = "-il";
        //x64 command
        private const string X64CommandName = "/x64";
        private const string X64CommandNameAlt = "-x64";
        //i386 command
        private const string I386CommandName = "/i386";
        private const string I386CommandNameAlt = "-i386";
        //ia64 command
        private const string IA64CommandName = "/ia64";
        private const string IA64CommandNameAlt = "-ia64";
        //AMD64 command
        private const string AMD64CommandName = "/amd64";
        private const string AMD64CommandNameAlt = "-amd64";
        //info command
        private const string InfoCommandName = "/info";
        private const string InfoCommandNameAlt = "-info";

        private readonly ScriptCompilerOptions m_options;
        private string m_sourceFile;

        private CompileToAssemblyCommand()
        {
            m_options = new ScriptCompilerOptions { CompileToAssembly = true, Executable = false };
        }

        public int Execute(TextWriter output, TextReader input)
        {
            if (output == null) throw new ArgumentNullException("output");
#if !DEBUG
            try
            {
#endif
                if (File.Exists(SourceFile))
                    DynamicScriptInterpreter.Compile(SourceFile, m_options, EmitDebugInfo);
                else return InvalidCommand.FileNotFound;
                return InvalidCommand.Success;
#if !DEBUG
            }
            catch (Win32Exception e)
            {
                output.WriteLine(e.Message);
                return e.ErrorCode;
            }
            catch (Exception e)
            {
                output.WriteLine(e.Message);
                return InvalidCommand.InvalidFunction;
            }
#endif
        }

        /// <summary>
        /// Sets additional information about assembly.
        /// </summary>
        public AssemblyVersionResource AssemblyInfo
        {
            set { m_options.AssemblyInfo = value; }
        }

        /// <summary>
        /// Sets generated PE file kind.
        /// </summary>
        public PortableExecutableKinds PEKind
        {
            set { m_options.PEKind = value; }
        }

        /// <summary>
        /// Sets generated image type.
        /// </summary>
        public ImageFileMachine ImageType
        {
            set { m_options.ImageType = value; }
        }

        /// <summary>
        /// Sets a value indicating that compiler should compile executable file.
        /// </summary>
        public bool Executable
        {
            set { m_options.Executable = value; }
        }

        /// <summary>
        /// Sets a value indicating that the debug information should be emitted.
        /// </summary>
        public bool EmitDebugInfo
        {
            private get;
            set;
        }

        /// <summary>
        /// Sets assembly name.
        /// </summary>
        public string AssemblyName
        {
            set { m_options.AssemblyName = value; }
        }

        /// <summary>
        /// Gets or sets source file to be compiled.
        /// </summary>
        public string SourceFile
        {
            private get { return m_sourceFile; }
            set
            {
                AssemblyName = Path.GetFileNameWithoutExtension(m_sourceFile = value);
            }
        }

        /// <summary>
        /// Gets or sets assembly version.
        /// </summary>
        public Version AssemblyVersion
        {
            get { return m_options.AssemblyVersion; }
            set { m_options.AssemblyVersion = value; }
        }

        private static Version ParseVersion(string version)
        {
            var result = default(Version);
            return Version.TryParse(version, out result) ? result : null;
        }

        private static AssemblyVersionResource ParseAssemblyInfo(string assemblyInfo)
        {
            if (assemblyInfo == null) assemblyInfo = String.Empty;
            const char Comma = ',';
            const char Equality = '=';
            var productName = default(string);
            var productVersion = default(Version);
            var company = default(string);
            var copyright = default(string);
            var trademark = default(string);
            foreach (var part in assemblyInfo.Split(Comma))
            {
                var pair = part.Trim().Split(Equality);
                if (pair.LongLength != 2L) continue;
                const string ProductNameField = "product";
                const string ProductVersionField = "version";
                const string CompanyField = "company";
                const string CopyrightField = "copyright";
                const string TrademarkField = "trademark";
                switch (pair[0])
                {
                    case ProductNameField: productName = pair[1]; break;
                    case ProductVersionField: Version.TryParse(pair[1], out productVersion); break;
                    case CompanyField: company = pair[1]; break;
                    case CopyrightField: copyright = pair[1]; break;
                    case TrademarkField: trademark = pair[1]; break;
                }
            }
            return string.IsNullOrEmpty(productName) ? null : new AssemblyVersionResource(productName, productVersion, company, copyright, trademark);
        }

        public static ICommand Parse(IEnumerator<string> args)
        {
            switch (args.MoveNext())
            {
                case true:
                    //Saves assembly name to the command.
                    var command = new CompileToAssemblyCommand { SourceFile = args.Current };
                    while (args.MoveNext()) switch (args.Current)
                        {
                            case VersionCommandName:
                            case VersionCommandNameAlt:
                                if (args.MoveNext()) command.AssemblyVersion = ParseVersion(args.Current); else return InvalidCommand.AssemblyVersionExpected;
                                continue;
                            case NameCommandName:
                            case NameCommandNameAlt:
                                if (args.MoveNext()) command.AssemblyName = args.Current; else return InvalidCommand.AssemblyNameExpected;
                                continue;
                            case DebugCommandName:
                            case DebugCommandNameAlt:
                                command.EmitDebugInfo = true;
                                continue;
                        case ExecutableCommandName:
                        case ExecutableCommandNameAlt:
                                command.Executable = true;
                                continue;
                        case X86CommandName:
                        case X86CommandNameAlt:
                                command.PEKind = PortableExecutableKinds.Required32Bit;
                                continue;
                        case ILCommandName:
                        case ILCommandNameAlt:
                                command.PEKind = PortableExecutableKinds.ILOnly;
                                continue;
                        case X64CommandName:
                        case X64CommandNameAlt:
                                command.PEKind = PortableExecutableKinds.PE32Plus;
                                continue;
                        case I386CommandName:
                        case I386CommandNameAlt:
                                command.ImageType = ImageFileMachine.I386;
                                continue;
                        case AMD64CommandName:
                        case AMD64CommandNameAlt:
                                command.ImageType = ImageFileMachine.AMD64;
                                continue;
                        case IA64CommandName:
                        case IA64CommandNameAlt:
                                command.ImageType = ImageFileMachine.IA64;
                                continue;
                        case InfoCommandName:
                        case InfoCommandNameAlt:
                                if (args.MoveNext()) command.AssemblyInfo = ParseAssemblyInfo(args.Current); else return InvalidCommand.AssemblyInfoExpected;
                                continue;
                        default: return command;
                        } ;
                    return command;
                default:
                    return InvalidCommand.ScriptFileNameExpected;
            }
        }
    }
}
