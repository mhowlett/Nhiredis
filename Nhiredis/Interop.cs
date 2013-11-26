/*
  Copyright (c) 2013, Matt Howlett
  All rights reserved.

  Redistribution and use in source and binary forms, with or without modification, 
  are permitted provided that the following conditions are met:

  1. Redistributions of source code must retain the above copyright notice, this list 
     of conditions and the following disclaimer.

  2. Redistributions in binary form must reproduce the above copyright notice, this 
     list of conditions and the following disclaimer in the documentation and/or other
     materials provided with the distribution.

  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
  OF SUCH DAMAGE. 
*/

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Nhiredis
{

        /// <summary>
        /// This class can allow platforms to provide a custom method for loading the nanomsg library.
        /// 
        /// This uses the convention of a library being in:
        ///   Win32 - [architecture]/module.dll
        ///   Posix - [architecture]/libmodule.so
        /// </summary>
    public static class LibraryLoader
    {
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetProcAddress(IntPtr hModule, String procname);

        [DllImport("libdl.so")]
        static extern IntPtr dlopen(String fileName, int flags);

        [DllImport("libdl.so")]
        static extern IntPtr dlerror();

        [DllImport("libdl.so")]
        static extern IntPtr dlsym(IntPtr handle, String symbol);

        static LibraryLoader()
        {
            if (Environment.OSVersion.Platform.ToString().Contains("Win32"))
            {
                CustomLoadLibrary = LoadWindowsLibrary;
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix ||
                     Environment.OSVersion.Platform == PlatformID.MacOSX ||
                     (int)Environment.OSVersion.Platform == 128)
            {
                CustomLoadLibrary = LoadPosixLibrary;
            }
        }

        static IntPtr LoadWindowsLibrary(string libName, out SymbolLookupDelegate symbolLookup)
        {
            string libFile = libName + ".dll";
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;

            var paths = new[]
                {
                    Path.Combine(rootDirectory, "bin", Environment.Is64BitProcess ? "x64" : "x86", libFile),
                    Path.Combine(rootDirectory, Environment.Is64BitProcess ? "x64" : "x86", libFile),
                    Path.Combine(rootDirectory, libFile)
                };

            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    var addr = LoadLibrary(path);
                    if (addr == IntPtr.Zero)
                    {
                        // Not using NanomsgException because it depends on nn_errno.
                        throw new Exception("LoadLibrary failed: " + path);
                    }
                    symbolLookup = GetProcAddress;
                    return addr;
                }
            }

            throw new Exception("LoadLibrary failed: unable to locate library " + libFile + ". Searched: " + paths.Aggregate((a, b) => a + "; " + b));
        }

        static IntPtr LoadPosixLibrary(string libName, out SymbolLookupDelegate symbolLookup)
        {
            const int RTLD_NOW = 2;
            string libFile = "lib" + libName.ToLower() + ".so";
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;

            var paths = new[]
                {
                    Path.Combine(rootDirectory, "bin", Environment.Is64BitProcess ? "x64" : "x86", libFile),
                    Path.Combine(rootDirectory, Environment.Is64BitProcess ? "x64" : "x86", libFile),
                    Path.Combine(rootDirectory, libFile),
                    Path.Combine("/usr/local/lib", libFile),
                    Path.Combine("/usr/lib", libFile)
                };

            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    var addr = dlopen(path, RTLD_NOW);
                    if (addr == IntPtr.Zero)
                    {
                        // Not using NanosmgException because it depends on nn_errno.
                        throw new Exception("dlopen failed: " + path + " : " + Marshal.PtrToStringAnsi(dlerror()));
                    }
                    symbolLookup = dlsym;
                    return addr;
                }
            }

            throw new Exception("dlopen failed: unable to locate library " + libFile + ". Searched: " + paths.Aggregate((a, b) => a + "; " + b));
        }

        public delegate IntPtr SymbolLookupDelegate(IntPtr addr, string name);

        public delegate IntPtr LoadLibraryDelegate(string libName, out SymbolLookupDelegate symbolLookup);

        public static LoadLibraryDelegate CustomLoadLibrary { get; set; }
    }
    
    
    [System.Security.SuppressUnmanagedCodeSecurity]
    static class Interop
    {
        static Interop()
        {
            if (LibraryLoader.CustomLoadLibrary != null)
            {
                LibraryLoader.SymbolLookupDelegate symbolLookup;
                var hiredisxAddr = LibraryLoader.CustomLoadLibrary("Hiredisx", out symbolLookup);
                
                InitializeDelegates(hiredisxAddr, symbolLookup);
            }
        }

        private static void InitializeDelegates(IntPtr hiredisxAddr, LibraryLoader.SymbolLookupDelegate lookup)
        {
            // When running under mono and the native hiredisx libraries are in a non-standard location (e.g. are placed in application_dir/x86|x64), 
            // we cannot just load the native libraries and have everything automatically work. Hence all these delegates. 

            // TODO: The performance impact of this over conventional P/Invoke is evidently not good - there is about a 50% increase in overhead.
            // http://ybeernet.blogspot.com/2011/03/techniques-of-calling-unmanaged-code.html

            // There appears to be an alternative "dllmap" approach:
            // http://www.mono-project.com/Interop_with_Native_Libraries
            // but this requires config file entries.

            // Perhaps the method of calling native methods would better depend on which platform is being used. 

            // anyway, delegates work everywhere so that's what we'll use for now. get it working first, optimize later.

            redisConnectWithTimeoutX = (redisConnectWithTimeoutX_delegate)Marshal.GetDelegateForFunctionPointer(lookup(hiredisxAddr, "redisConnectWithTimeoutX"), typeof(redisConnectWithTimeoutX_delegate));
            redisCommandX = (redisCommandX_delegate)Marshal.GetDelegateForFunctionPointer(lookup(hiredisxAddr, "redisCommandX"), typeof(redisCommandX_delegate));
            retrieveElementX = (retrieveElementX_delegate)Marshal.GetDelegateForFunctionPointer(lookup(hiredisxAddr, "retrieveElementX"), typeof(retrieveElementX_delegate));
            freeReplyObjectX = (freeReplyObjectX_delegate)Marshal.GetDelegateForFunctionPointer(lookup(hiredisxAddr, "freeReplyObjectX"), typeof(freeReplyObjectX_delegate));
            retrieveStringAndFreeReplyObjectX = (retrieveStringAndFreeReplyObjectX_delegate)Marshal.GetDelegateForFunctionPointer(lookup(hiredisxAddr, "retrieveStringAndFreeReplyObjectX"), typeof(retrieveStringAndFreeReplyObjectX_delegate));
            retrieveElementStringX = (retrieveElementStringX_delegate)Marshal.GetDelegateForFunctionPointer(lookup(hiredisxAddr, "retrieveElementStringX"), typeof(retrieveElementStringX_delegate));
            setupArgumentArrayX = (setupArgumentArrayX_delegate)Marshal.GetDelegateForFunctionPointer(lookup(hiredisxAddr, "setupArgumentArrayX"), typeof(setupArgumentArrayX_delegate));
            setArgumentX = (setArgumentX_delegate)Marshal.GetDelegateForFunctionPointer(lookup(hiredisxAddr, "setArgumentX"), typeof(setArgumentX_delegate));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr redisConnectWithTimeoutX_delegate(
            IntPtr ip,
            int ipLen,
            int port,
            int timeoutSeconds,
            int timeoutMicroseconds);
        public static redisConnectWithTimeoutX_delegate redisConnectWithTimeoutX;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void redisCommandX_delegate(
            IntPtr redisContext,
            IntPtr args,
            int argsc,
            out int type,
            out long integer,
            byte[] strBuf,
            int strBufLen,
            out int len,
            out int elements,
            out IntPtr reply);
        public static redisCommandX_delegate redisCommandX;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retrieveElementX_delegate(
            IntPtr replyObject,
            int index,
            out int type,
            out long integer,
            byte[] strBuf,
            int strBufLen,
            out int len,
            out IntPtr strPtr);
        public static retrieveElementX_delegate retrieveElementX;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void freeReplyObjectX_delegate(
            IntPtr reply);
        public static freeReplyObjectX_delegate freeReplyObjectX;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retrieveStringAndFreeReplyObjectX_delegate(
            IntPtr replyObject,
            byte[] toStrPtr);
        public static retrieveStringAndFreeReplyObjectX_delegate retrieveStringAndFreeReplyObjectX;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retrieveElementStringX_delegate(
            IntPtr replyObject,
            int index,
            byte[] toStrPtr);
        public static retrieveElementStringX_delegate retrieveElementStringX;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void setupArgumentArrayX_delegate(
            int length,
            out IntPtr arguments);
        public static setupArgumentArrayX_delegate setupArgumentArrayX;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void setArgumentX_delegate(
            IntPtr arguments,
            int index,
            byte[] argument,
            int len);
        public static setArgumentX_delegate setArgumentX;

    }
}
