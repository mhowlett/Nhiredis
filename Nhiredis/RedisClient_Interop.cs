using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Nhiredis
{
    public partial class RedisClient
    {
        private static class Interop
        {
            [DllImport("hiredis_wrapper_win32.dll")]
            public static extern IntPtr n_redisConnectWithTimeout(
                [MarshalAs(UnmanagedType.LPStr)] string ip, 
                int port, 
                int timeoutSeconds,
                int timeoutMicroseconds);


            [DllImport("hiredis_wrapper_win32.dll")]
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


            [DllImport("hiredis_wrapper_win32.dll")]
            public static extern void n_retrieveElement(
                IntPtr replyObject,
                int index,
                out int type,
                out long integer,
                [MarshalAs(UnmanagedType.LPStr)] StringBuilder strBuf,
                int strBufLen,
                out int len,
                out IntPtr strPtr);


            [DllImport("hiredis_wrapper_win32.dll")]
            public static extern void n_freeReplyObject(
	            IntPtr reply);


            [DllImport("hiredis_wrapper_win32.dll")]
            public static extern void n_retrieveStringAndFreeReplyObject(
                IntPtr replyObject,
                [MarshalAs(UnmanagedType.LPStr)] StringBuilder toStrPtr);


            [DllImport("hiredis_wrapper_win32.dll")]
            public static extern void n_retrieveElementString(
		        IntPtr replyObject,
		        int index,
                [MarshalAs(UnmanagedType.LPStr)] StringBuilder toStrPtr);


            [DllImport("hiredis_wrapper_win32.dll")]
            public static extern void n_setupArgumentArray(
	            int length,
	            out IntPtr arguments);


            [DllImport("hiredis_wrapper_win32.dll")]
            public static extern void n_setArgument(
                IntPtr arguments,
                int index,
                [MarshalAs(UnmanagedType.LPStr)] string argument,
                int len);

        }
    }
}
