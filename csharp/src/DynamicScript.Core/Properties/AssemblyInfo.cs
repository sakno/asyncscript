using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CLSCompliantAttribute = System.CLSCompliantAttribute;
using NeutralResourcesLanguageAttribute = System.Resources.NeutralResourcesLanguageAttribute;

[assembly: CLSCompliant(false)]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: InternalsVisibleTo("dsi, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c993b0fc86dbb0800ab9b7bca5a00a45b6f026721bd494f79d76afc0c71d98103ce650703007da70f81c39ca853fd7a4136329303914c39c41422101af26f595041d32bfc0d2d234a091f8ced7e5c0dbc2f738547ebc1fdc402c44844ce328a7e3ab3c7545db79e402f97b900b1aaaac15df178598324954b52dc14129a28ab0")]

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DynamicScript Compiler Routines")]
[assembly: AssemblyDescription("DynamicScript Compiler Routines")]
#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif
[assembly: AssemblyCompany("Apache 2.0 License")]
[assembly: AssemblyProduct("DynamicScript Programming Language")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a4ba4aa0-cc0b-48b6-a765-dd03822e701e")]

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
[assembly: AssemblyVersion("0.8.4.*")]
[assembly: AssemblyFileVersion("0.8.4.0")]
[assembly: AssemblyInformationalVersion("0.8.4 beta")]
