using System.Reflection;

// This file generates version numbers for all DLL's. For the installer to properly overwrite a previous install, this number must be changed.
// For now, we are just incrementing the last digit.

// The reason we are doing this here instead of just updating AssemblyInfo.cs, is because this propogates to all DLL's.

[assembly: AssemblyVersion("8.0.2.01")]
