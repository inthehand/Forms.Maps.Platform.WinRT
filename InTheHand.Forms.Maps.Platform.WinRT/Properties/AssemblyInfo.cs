﻿using System.Resources;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
#if WINDOWS_UWP
[assembly: AssemblyTitle("InTheHand.Forms.Maps.Platform.UWP")]
[assembly: AssemblyProduct("InTheHand.Forms.Maps.Platform.UWP")]
#else
[assembly: AssemblyTitle("InTheHand.Forms.Maps.Platform.WinRT")]
[assembly: AssemblyProduct("InTheHand.Forms.Maps.Platform.WinRT")]
#endif
[assembly: AssemblyDescription("Xamarin Forms Maps for Windows")]
#if WINDOWS_UWP
[assembly: AssemblyConfiguration("Windows 10")]
#elif WINDOWS_APP
[assembly: AssemblyConfiguration("Windows 8.1")]
#else
[assembly: AssemblyConfiguration("Windows Phone 8.1")]
#endif
[assembly: AssemblyCompany("In The Hand Ltd")]

[assembly: AssemblyCopyright("Copyright © In The Hand 2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en")]

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
[assembly: AssemblyVersion("2.0.0.0")]
[assembly: AssemblyFileVersion("2.0.1223.0")]
