using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyDescription("Mediolanum_RMA_FILTER")]
// to activate the platform case on a new platform x64 or x86
// Add $(PlatformTarget) to the property <DefineConstants>
// As follows <DefineConstants>TRACE;$(PlatformTarget)</DefineConstants>
// Make sure the property <PlatformTarget> is defined before <DefineConstants>
// Ex:
//      <PlatformTarget>x64</PlatformTarget>
//      <DefineConstants>TRACE;$(PlatformTarget)</DefineConstants>
#if x86
[assembly: AssemblyTitle("Mediolanum_RMA_FILTER (x86)")]
#elif x64
[assembly: AssemblyTitle("Mediolanum_RMA_FILTER (x64)")]
#else
[assembly: AssemblyTitle("Mediolanum_RMA_FILTER (Any CPU)")]
#endif
[assembly: AssemblyProduct("Mediolanum_RMA_FILTER")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("aa73d840-7174-4094-a76f-bcd1af2a8937")]

[assembly: InternalsVisibleTo("MediolanumRMAQuery")]

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
[assembly: AssemblyVersion(MEDIOLANUM.VERSION)] // Never change this one for WF compatibility
[assembly: AssemblyFileVersion(MEDIOLANUM.VERSION)] // File Version