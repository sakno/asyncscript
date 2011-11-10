using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CLSCompliantAttribute = System.CLSCompliantAttribute;
using NeutralResourcesLanguageAttribute = System.Resources.NeutralResourcesLanguageAttribute;
using InterpreterHostAttribute = DynamicScript.InterpreterHostAttribute;
using PEFileKinds = System.Reflection.Emit.PEFileKinds;

[assembly: CLSCompliant(false)]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: SuppressIldasm]
[assembly: InterpreterHost(PEFileKinds.WindowApplication)]

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DynamicScript Host for Windows")]
[assembly: AssemblyDescription("DynamicScript console-detached host")]
#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RETAIL")]
#endif
[assembly: AssemblyCompany("Apache 2.0 License")]
[assembly: AssemblyProduct("DynamicScript Programming Language")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("1a290e3a-05c7-413f-915b-76c2748c5d8e")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.8.*")]
[assembly: AssemblyFileVersion("0.8.0.0")]
[assembly: AssemblyInformationalVersion("0.8 beta")]
