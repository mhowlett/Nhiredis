using System;
using System.Text;

namespace Nhiredis
{
    public partial class RedisClient
    {
        // we can't use compile time constants to make this process easier becasue
        // required functionality is this be able to work on both platforms without
        // recompilation.

        private class Interop
        {
            public static IntPtr n_redisConnectWithTimeout(
                string ip,
                int port,
                int timeoutSeconds,
                int timeoutMicroseconds)
            {
                if (UsingWindows)
                {
                    return InteropWin32.n_redisConnectWithTimeout(ip, port, timeoutSeconds, timeoutMicroseconds);
                }
                else
                {
                    return InteropLinux.n_redisConnectWithTimeout(ip, port, timeoutSeconds, timeoutMicroseconds);                    
                }
            }

            public static void n_redisCommand(
                IntPtr redisContext,
                IntPtr args,
                int argsc,
                out int type,
                out long integer,
                StringBuilder strBuf,
                int strBufLen,
                out int len,
                out int elements,
                out IntPtr reply)
            {
                if (UsingWindows)
                {
                    InteropWin32.n_redisCommand(redisContext, args, argsc, out type, out integer, strBuf, strBufLen, out len, out elements, out reply);
                }
                else
                {
                    InteropLinux.n_redisCommand(redisContext, args, argsc, out type, out integer, strBuf, strBufLen, out len, out elements, out reply);
                }
            }

            public static void n_retrieveElement(
                IntPtr replyObject,
                int index,
                out int type,
                out long integer,
                StringBuilder strBuf,
                int strBufLen,
                out int len,
                out IntPtr strPtr)
            {
                if (UsingWindows)
                {
                    InteropWin32.n_retrieveElement(replyObject, index, out type, out integer, strBuf, strBufLen, out len, out strPtr);
                }
                else
                {
                    InteropLinux.n_retrieveElement(replyObject, index, out type, out integer, strBuf, strBufLen, out len, out strPtr);
                }
            }

            public static void n_freeReplyObject(
                IntPtr reply)
            {
                if (UsingWindows)
                {
                    InteropWin32.n_freeReplyObject(reply);
                }
                else
                {
                    InteropLinux.n_freeReplyObject(reply);
                }
            }

            public static void n_retrieveStringAndFreeReplyObject(
                IntPtr replyObject,
                StringBuilder toStrPtr)
            {
                if (UsingWindows)
                {
                    InteropWin32.n_retrieveStringAndFreeReplyObject(replyObject, toStrPtr);
                }
                else
                {
                    InteropLinux.n_retrieveStringAndFreeReplyObject(replyObject, toStrPtr);
                }
            }

            public static void n_retrieveElementString(
                IntPtr replyObject,
                int index,
                StringBuilder toStrPtr)
            {
                if (UsingWindows)
                {
                    InteropWin32.n_retrieveElementString(replyObject, index, toStrPtr);
                }
                else
                {
                    InteropLinux.n_retrieveElementString(replyObject, index, toStrPtr);
                }
            }

            public static void n_setupArgumentArray(
                int length,
                out IntPtr arguments)
            {
                if (UsingWindows)
                {
                    InteropWin32.n_setupArgumentArray(length, out arguments);
                }
                else
                {
                    InteropLinux.n_setupArgumentArray(length, out arguments);
                }
            }

            public static void n_setArgument(
                IntPtr arguments,
                int index,
                string argument,
                int len)
            {
                if (UsingWindows)
                {
                    InteropWin32.n_setArgument(arguments, index, argument, len);
                }
                else
                {
                    InteropLinux.n_setArgument(arguments, index, argument, len);
                }
            }


            private static bool? _usingWindows;

            private static bool UsingWindows
            {
                get
                {
                    if (_usingWindows == null)
                    {
                        string os_s = Environment.OSVersion.ToString().ToLower();
                        if (os_s.Contains("windows"))
                        {
                            _usingWindows = true;
                        }
                        _usingWindows = false;
                    }

                    return _usingWindows.Value;
                }
            }
        }
    }
}
