/*
  Copyright (c) 2011, Matt Howlett
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
using System.Runtime.InteropServices;
using System.Text;

namespace Nhiredis
{
    public partial class RedisClient
    {
        private static class InteropLinux
        {
            [DllImport("hiredisx.so")]
            public static extern IntPtr n_redisConnectWithTimeout(
                [MarshalAs(UnmanagedType.LPStr)] string ip,
                int port,
                int timeoutSeconds,
                int timeoutMicroseconds);

            [DllImport("hiredisx.so")]
            public static extern void n_redisCommand(
                IntPtr redisContext,
                IntPtr args,
                int argsc,
                out int type,
                out long integer,
                [MarshalAs(UnmanagedType.LPStr)] StringBuilder strBuf,
                int strBufLen,
                out int len,
                out int elements,
                out IntPtr reply);

            [DllImport("hiredisx.so")]
            public static extern void n_retrieveElement(
                IntPtr replyObject,
                int index,
                out int type,
                out long integer,
                [MarshalAs(UnmanagedType.LPStr)] StringBuilder strBuf,
                int strBufLen,
                out int len,
                out IntPtr strPtr);

            [DllImport("hiredisx.so")]
            public static extern void n_freeReplyObject(
                IntPtr reply);

            [DllImport("hiredisx.so")]
            public static extern void n_retrieveStringAndFreeReplyObject(
                IntPtr replyObject,
                [MarshalAs(UnmanagedType.LPStr)] StringBuilder toStrPtr);

            [DllImport("hiredisx.so")]
            public static extern void n_retrieveElementString(
                IntPtr replyObject,
                int index,
                [MarshalAs(UnmanagedType.LPStr)] StringBuilder toStrPtr);

            [DllImport("hiredisx.so")]
            public static extern void n_setupArgumentArray(
                int length,
                out IntPtr arguments);

            [DllImport("hiredisx.so")]
            public static extern void n_setArgument(
                IntPtr arguments,
                int index,
                [MarshalAs(UnmanagedType.LPStr)] string argument,
                int len);
        }
    }
}
