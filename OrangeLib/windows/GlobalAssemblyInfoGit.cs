/*
 * GlobalAssemblyInfo.cs
 *
 * This is free software. See COPYING for details.
 */



// VERSION=0.8.4
// ASM_VERSION=0.8.4


using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyProduct("OrangeShare")]

[assembly: AssemblyVersion("0.8.4")]
[assembly: AssemblyFileVersion("0.8.4")]
[assembly: AssemblyInformationalVersion("0.8.4")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

