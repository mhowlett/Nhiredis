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
            public static IntPtr redisConnectWithTimeout(
                byte[] ip,
                int ipLen,
                int port,
                int timeoutSeconds,
                int timeoutMicroseconds)
            {
                if (UsingWindows)
                {
                    return InteropWin32.redisConnectWithTimeoutX(ip, ipLen, port, timeoutSeconds, timeoutMicroseconds);
                }
                else
                {
                    return InteropLinux.redisConnectWithTimeoutX(ip, ipLen, port, timeoutSeconds, timeoutMicroseconds);                    
                }
            }

            public static void redisCommand(
                IntPtr redisContext,
                IntPtr args,
                int argsc,
                out int type,
                out long integer,
                byte[] strBuf,
                int strBufLen,
                out int len,
                out int elements,
                out IntPtr reply)
            {
                if (UsingWindows)
                {
                    InteropWin32.redisCommandX(redisContext, args, argsc, out type, out integer, strBuf, strBufLen, out len, out elements, out reply);
                }
                else
                {
                    InteropLinux.redisCommandX(redisContext, args, argsc, out type, out integer, strBuf, strBufLen, out len, out elements, out reply);
                }
            }

            public static void retrieveElement(
                IntPtr replyObject,
                int index,
                out int type,
                out long integer,
                byte[] strBuf,
                int strBufLen,
                out int len,
                out IntPtr strPtr)
            {
                if (UsingWindows)
                {
                    InteropWin32.retrieveElementX(replyObject, index, out type, out integer, strBuf, strBufLen, out len, out strPtr);
                }
                else
                {
                    InteropLinux.retrieveElementX(replyObject, index, out type, out integer, strBuf, strBufLen, out len, out strPtr);
                }
            }

            public static void freeReplyObject(
                IntPtr reply)
            {
                if (UsingWindows)
                {
                    InteropWin32.freeReplyObjectX(reply);
                }
                else
                {
                    InteropLinux.freeReplyObjectX(reply);
                }
            }

            public static void retrieveStringAndFreeReplyObject(
                IntPtr replyObject,
                byte[] toStrPtr)
            {
                if (UsingWindows)
                {
                    InteropWin32.retrieveStringAndFreeReplyObjectX(replyObject, toStrPtr);
                }
                else
                {
                    InteropLinux.retrieveStringAndFreeReplyObjectX(replyObject, toStrPtr);
                }
            }

            public static void retrieveElementString(
                IntPtr replyObject,
                int index,
                byte[] toStrPtr)
            {
                if (UsingWindows)
                {
                    InteropWin32.retrieveElementStringX(replyObject, index, toStrPtr);
                }
                else
                {
                    InteropLinux.retrieveElementStringX(replyObject, index, toStrPtr);
                }
            }

            public static void setupArgumentArray(
                int length,
                out IntPtr arguments)
            {
                if (UsingWindows)
                {
                    InteropWin32.setupArgumentArrayX(length, out arguments);
                }
                else
                {
                    InteropLinux.setupArgumentArrayX(length, out arguments);
                }
            }

            public static void setArgument(
                IntPtr arguments,
                int index,
                byte[] argument,
                int len)
            {
                if (UsingWindows)
                {
                    InteropWin32.setArgumentX(arguments, index, argument, len);
                }
                else
                {
                    InteropLinux.setArgumentX(arguments, index, argument, len);
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
                        _usingWindows = os_s.Contains("windows");
                    }

                    return _usingWindows.Value;
                }
            }
        }
    }
}
