using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("WalkAbout")]
[assembly: AssemblyDescription("The WalkAbout mod allows you to take an available kerbal from the Astronaut Complex and have him/her placed outside any door at the KSC.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(KspWalkAbout.LegalMamboJambo.Company)]
[assembly: AssemblyProduct(KspWalkAbout.LegalMamboJambo.Product)]
[assembly: AssemblyCopyright(KspWalkAbout.LegalMamboJambo.Copyright)]
[assembly: AssemblyTrademark(KspWalkAbout.LegalMamboJambo.Trademark)]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("68ff2bb0-ad31-49c4-a847-612269617cce")]

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
[assembly: AssemblyVersion(KspWalkAbout.Version.Number)]
[assembly: AssemblyFileVersion(KspWalkAbout.Version.Number)]
[assembly: KSPAssembly("KspWalkAbout", KspWalkAbout.Version.major, KspWalkAbout.Version.minor)]
[assembly: KSPAssemblyDependency("KSPe", 2, 5)]
[assembly: KSPAssemblyDependency("KSPe.UI", 2, 5)]
