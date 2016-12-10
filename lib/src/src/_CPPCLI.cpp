// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

#pragma runtime_checks( "", off )


// there appears to be a BIG ISSUE with making the native DLL /clr, as opposed to NOCLR
// when we are /clr and we are debugging either the .exe output or .net assemblies loading this dll,
// the debugger gets confused and cannot restart the processes
// We did this mess so we could have this native-dll 'follow' any .net assemblies referencing it.
// we are going to try using a Debug/Release build switch to only do CppCLI in release mode.
// A another approach if this fails might be ot this assembly into a COM assembly for following. 
// to be explored.

//#if !defined(_DEBUG)

#if defined(_WIN32) || defined(_WIN64)

// this class does nothing, but it is important on the windows platform
// because the class is referenced from the main C# class library,
// the native DLL will be copied into the output of any projects that reference the main C# class library
// for that reason, do not delete this class, or references to it from the main C# class library.
public ref class DummyReferenceClass
{
public:
	DummyReferenceClass() {	}
};
#endif
//#endif
