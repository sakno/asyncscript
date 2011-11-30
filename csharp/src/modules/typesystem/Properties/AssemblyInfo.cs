using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CLSCompliantAttribute = System.CLSCompliantAttribute;
using CompiledScriptAttribute = DynamicScript.Runtime.Hosting.CompiledScriptAttribute;
using Module = DynamicScript.Modules.TypeSystem.CustomTypeFactory;

[assembly: CLSCompliant(false)]
[assembly: CompiledScript(typeof(Module))]

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Custom Type System Support Library")]
[assembly: AssemblyDescription("Custom Type System Support Library")]
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
[assembly: Guid("df6c2454-172b-4bef-8c85-07c2ac272f3c")]

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
[assembly: AssemblyVersion("0.8.3.*")]
[assembly: AssemblyFileVersion("0.8.3.0")]
[assembly: AssemblyInformationalVersion("0.8.3 beta")]
