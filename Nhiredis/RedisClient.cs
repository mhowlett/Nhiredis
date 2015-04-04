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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Nhiredis
{
    public partial class RedisClient
    {
        private const int REDIS_REPLY_STRING = 1;
        private const int REDIS_REPLY_ARRAY = 2;
        private const int REDIS_REPLY_INTEGER = 3;
        private const int REDIS_REPLY_NIL = 4;
        private const int REDIS_REPLY_STATUS = 5;
        private const int REDIS_REPLY_ERROR = 6;

        private static UTF8Encoding enc = new UTF8Encoding(false);

        private static byte[] StringToUtf8(string s)
        {
            return enc.GetBytes(s);
        }

        public static RedisContext RedisConnectWithTimeout(string host, int port, TimeSpan timeout)
        {
            int seconds = (int)(timeout.TotalSeconds);
            int milliseconds = (int)(timeout.TotalMilliseconds - seconds*1000);

            byte[] ipBytes = StringToUtf8(host);
            unsafe
            {
                fixed (byte* pIpBytes = ipBytes)
                {
                    var result = new RedisContext { NativeContext = Interop.redisConnectWithTimeoutX(new IntPtr(pIpBytes), ipBytes.Length, port, seconds, milliseconds * 1000) };

                    if (result.NativeContext == IntPtr.Zero)
                    {
                        throw new NhiredisException("Unable to establish redis connection [" + host + ":" + port + "]");
                    }

                    return result;
                }
            }
        }

        public static T RedisCommand<T>(RedisContext context, params object[] arguments)
        {
            object result = RedisCommandImpl(context, arguments, typeof(T));
            if (!(result is T))
            {
                if (result != null)
                {
                    throw new NhiredisException(
                        "Expecting RedisCommand reply to have type: " + typeof (T) +
                        ", but type was " + result.GetType());
                }
            }
            return (T)result;
        }

        public static object RedisCommand(
            RedisContext context, 
            params object[] arguments)
        {
            return RedisCommandImpl(context, arguments, null);
        }

        private static ArrayList flattenArgumentList(object[] arguments)
        {
            var args = new ArrayList();
            for (int i = 0; i < arguments.Length; ++i)
            {
                if (arguments[i] is IDictionary)
                {
                    foreach (DictionaryEntry v in (IDictionary)arguments[i])
                    {
                        args.Add(v.Key.ToString());
                        args.Add(v.Value.ToString());
                    }
                }
                else if (!(arguments[i] is byte[]) && !(arguments[i] is string) && arguments[i] is IEnumerable)
                {
                    foreach (var v in (IEnumerable)arguments[i])
                    {
                        if (v is byte[])
                        {
                            args.Add(v);
                        }
                        else
                        {
                            args.Add(v.ToString());
                        }
                    }
                }
                else
                {
                    if (arguments[i] is byte[])
                    {
                        args.Add(arguments[i]);
                    }
                    else
                    {
                        args.Add(arguments[i].ToString());
                    }
                }
            }

            return args;
        }


        private static object handleArrayResult(Type typeHint, IntPtr replyObject, int elements)
        {
            int currentByteBufLength = 64;
            var byteBuf = new byte[currentByteBufLength];

            var result_o = (typeHint == null || typeHint == typeof(List<object>)) ? new List<object>() : null;
            var result_s = (typeHint == typeof(List<string>) || typeof(IDictionary).IsAssignableFrom(typeHint)) ? new List<string>() : null;
            var result_l = typeHint == typeof(List<long>) ? new List<long>() : null;
            var result_i = typeHint == typeof(List<int>) ? new List<int>() : null;
            var result_b = typeHint == typeof(List<byte[]>) ? new List<byte[]>() : null;

            if (replyObject != IntPtr.Zero)
            {
                for (int i = 0; i < elements; ++i)
                {
                    IntPtr strPtr;
                    int type;
                    long integer;
                    int len;

                    Interop.retrieveElementX(
                        replyObject,
                        i,
                        out type,
                        out integer,
                        byteBuf,
                        currentByteBufLength,
                        out len,
                        out strPtr);

                    if (strPtr != IntPtr.Zero)
                    {
                        currentByteBufLength = len;
                        byteBuf = new byte[len];
                        Interop.retrieveElementStringX(replyObject, i, byteBuf);
                    }

                    switch (type)
                    {
                        case REDIS_REPLY_STRING:
                            if (result_s != null)
                            {
                                result_s.Add(enc.GetString(byteBuf, 0, len));
                            }
                            else if (result_o != null)
                            {
                                result_o.Add(enc.GetString(byteBuf, 0, len));
                            }
                            else if (result_b != null)
                            {
                                var res = new byte[len];
                                for (int k = 0; k < len; ++k)
                                {
                                    res[k] = byteBuf[k];
                                }
                                result_b.Add(res);
                            }
                            else if (result_l != null)
                            {
                                long result;
                                if (!long.TryParse(enc.GetString(byteBuf, 0, len), out result))
                                {
                                    result = long.MinValue;
                                }
                                result_l.Add(result);
                            }
                            else if (result_i != null)
                            {
                                int result;
                                if (!int.TryParse(enc.GetString(byteBuf, 0, len), out result))
                                {
                                    result = int.MinValue;
                                }
                                result_i.Add(result);
                            }
                            break;

                        case REDIS_REPLY_INTEGER:
                            if (result_l != null)
                            {
                                result_l.Add(integer);
                            }
                            else if (result_i != null)
                            {
                                result_i.Add((int)integer);
                            }
                            else if (result_o != null)
                            {
                                result_o.Add(integer);
                            }
                            else if (result_s != null)
                            {
                                result_s.Add(integer.ToString());
                            }
                            else if (result_b != null)
                            {
                                result_b.Add(BitConverter.GetBytes(integer));
                            }
                            break;

                        case REDIS_REPLY_ARRAY:
                            if (result_o != null)
                            {
                                result_o.Add(null);
                            }
                            else if (result_s != null)
                            {
                                result_s.Add(null);
                            }
                            else if (result_l != null)
                            {
                                result_l.Add(long.MinValue);
                            }
                            else if (result_i != null)
                            {
                                result_i.Add(int.MinValue);
                            }
                            else if (result_b != null)
                            {
                                result_b.Add(null);
                            }
                            break;

                        case REDIS_REPLY_NIL:

                            if (result_o != null)
                            {
                                result_o.Add(null);
                            }
                            else if (result_s != null)
                            {
                                result_s.Add(null);
                            }
                            else if (result_l != null)
                            {
                                result_l.Add(long.MinValue);
                            }
                            else if (result_i != null)
                            {
                                result_i.Add(int.MinValue);
                            }
                            else if (result_b != null)
                            {
                                result_b.Add(null);
                            }
                            break;

                        case REDIS_REPLY_STATUS:
                            if (result_s != null)
                            {
                                result_s.Add(enc.GetString(byteBuf, 0, len));
                            }
                            else if (result_o != null)
                            {
                                result_o.Add(enc.GetString(byteBuf, 0, len));
                            }
                            else if (result_l != null)
                            {
                                long result;
                                if (!long.TryParse(enc.GetString(byteBuf, 0, len), out result))
                                {
                                    result = long.MinValue;
                                }
                                result_l.Add(result);
                            }
                            else if (result_i != null)
                            {
                                int result;
                                if (!int.TryParse(enc.GetString(byteBuf, 0, len), out result))
                                {
                                    result = int.MinValue;
                                }
                                result_i.Add(result);
                            }
                            else if (result_b != null)
                            {
                                var res = new byte[len];
                                for (int k = 0; k < len; ++k)
                                {
                                    res[k] = byteBuf[k];
                                }
                                result_b.Add(res);
                            }
                            break;

                        case REDIS_REPLY_ERROR:
                            if (result_s != null)
                            {
                                result_s.Add(enc.GetString(byteBuf, 0, len));
                            }
                            else if (result_o != null)
                            {
                                result_o.Add(enc.GetString(byteBuf, 0, len));
                            }
                            else if (result_l != null)
                            {
                                result_l.Add(long.MinValue);
                            }
                            else if (result_i != null)
                            {
                                result_i.Add(int.MinValue);
                            }
                            else if (result_b != null)
                            {
                                result_b.Add(null);
                            }
                            break;

                        default:
                            throw new NhiredisException("Unknown redis return type: " + type);
                    }
                }
                Interop.freeReplyObjectX(replyObject);
            }

            if (result_s != null)
            {
                if (typeof(IDictionary).IsAssignableFrom(typeHint))
                {
                    return Utils.ConstructDictionary(result_s, typeHint);
                }
                return result_s;
            }
            if (result_o != null)
            {
                return result_o;
            }
            if (result_l != null)
            {
                return result_l;
            }
            if (result_i != null)
            {
                return result_i;
            }
            if (result_b != null)
            {
                return result_b;
            }

            return null;
        }

        private static object handleIntegerResult(Type typeHint, long integer)
        {
            if (typeHint == typeof(int) || typeHint == typeof(int?))
            {
                return (int) integer;
            }
            if (typeHint == typeof(string))
            {
                return integer.ToString();
            }
            if (typeHint == typeof (long) || typeHint == typeof(long?))
            {
                return (long) integer;
            }
            if (typeHint == typeof(double) || typeHint == typeof(double?))
            {
                return (double) integer;
            }
            if (typeHint == typeof(float) || typeHint == typeof(float?))
            {
                return (float) integer;
            }
            if (typeHint == typeof(bool) || typeHint == typeof(bool?))
            {
                if (integer == 1)
                {
                    return true;
                }
                if (integer == 0)
                {
                    return false;
                }
                return typeHint == typeof(bool?) ? null : new bool?(true);
            }
            if (typeHint == typeof(short) || typeHint == typeof(short?))
            {
                return (short) integer;
            }
            if (typeHint == typeof(byte) || typeHint == typeof(byte?))
            {
                return (byte) integer;
            }
            if (typeHint == typeof(UInt16) || typeHint == typeof(UInt16?))
            {
                return (UInt16) integer;
            }
            if (typeHint == typeof(UInt32) || typeHint == typeof(UInt32?))
            {
                return (UInt32) integer;
            }
            if (typeHint == typeof(UInt64) || typeHint == typeof(UInt64?))
            {
                return (UInt64) integer;
            }
            if (typeHint == typeof (Decimal) || typeHint == typeof (Decimal?))
            {
                return (Decimal) integer;
            }
            return integer;
        }

        private static object handleStringResult(Type typeHint, IntPtr replyObject, int resultLen)
        {
            var byteBuf = new byte[resultLen];
            if (replyObject != IntPtr.Zero)
            {
                Interop.retrieveStringAndFreeReplyObjectX(replyObject, byteBuf);
            }

            if (resultLen == 0)
            {
                return null;
            }
            if (typeHint == typeof(int) || typeHint == typeof(int?))
            {
                return int.Parse(enc.GetString(byteBuf, 0, resultLen));
            }
            if (typeHint == typeof(long) || typeHint == typeof(long?))
            {
                return long.Parse(enc.GetString(byteBuf, 0, resultLen));
            }
            if (typeHint == typeof(float) || typeHint == typeof(float?))
            {
                return float.Parse(enc.GetString(byteBuf, 0, resultLen));
            }
            if (typeHint == typeof(double) || typeHint == typeof(double?))
            {
                return double.Parse(enc.GetString(byteBuf, 0, resultLen));
            }
            if (typeHint == typeof(bool) || typeHint == typeof(bool?))
            {
                string s = enc.GetString(byteBuf, 0, resultLen);
                if (s == "1")
                {
                    return true;
                }
                if (s == "0")
                {
                    return false;
                }
                if (s.ToLower() == "true")
                {
                    return true;
                }
                if (s.ToLower() == "false")
                {
                    return false;
                }
                return null;
            }
            if (typeHint == typeof(byte[]))
            {
                byte[] bytes = new byte[resultLen];
                for (var i = 0; i < resultLen; ++i)
                {
                    bytes[i] = byteBuf[i];
                }
                return bytes;
            }
            if (typeHint == typeof(byte) || typeHint == typeof(byte?))
            {
                return byte.Parse(enc.GetString(byteBuf, 0, resultLen));
            }
            if (typeHint == typeof(short) || typeHint == typeof(short?))
            {
                return short.Parse(enc.GetString(byteBuf, 0, resultLen));
            }
            if (typeHint == typeof(UInt16) || typeHint == typeof(UInt16?))
            {
                return UInt16.Parse(enc.GetString(byteBuf, 0, resultLen));
            }
            if (typeHint == typeof(UInt32) || typeHint == typeof(UInt32?))
            {
                return UInt32.Parse(enc.GetString(byteBuf, 0, resultLen));
            }
            if (typeHint == typeof(UInt64) || typeHint == typeof(UInt64?))
            {
                return UInt64.Parse(enc.GetString(byteBuf, 0, resultLen));
            }
            if (typeHint == typeof(Decimal) || typeHint == typeof(Decimal?))
            {
                return Decimal.Parse(enc.GetString(byteBuf, 0, resultLen));
            }

            return enc.GetString(byteBuf, 0, resultLen);
        }


        private static object RedisCommandImpl(
            RedisContext context,
            object[] arguments,
            Type typeHint)
        {
            int currentByteBufLength = 64;

            int type;
            long integer;
            var byteBuf = new byte[currentByteBufLength];
            int elements;
            IntPtr replyObject;
            int len;

            var args = flattenArgumentList(arguments);   


            // STEP 1. Set specify arguments.

            IntPtr argumentsPtr = IntPtr.Zero;
            if (args.Count > 0)
            {
                Interop.setupArgumentArrayX(args.Count, out argumentsPtr);
                for (int i = 0; i < args.Count; ++i)
                {
                    if (args[i] is byte[])
                    {
                        Interop.setArgumentX(argumentsPtr, i, (byte[])args[i], ((byte[])args[i]).Length);
                    }
                    else
                    {
                        byte[] arg = StringToUtf8((string) args[i]);
                        Interop.setArgumentX(argumentsPtr, i, arg, arg.Length);
                    }
                }
            }


            // STEP 2. Execute command.

            Interop.redisCommandX(
                context.NativeContext,
                argumentsPtr,
                args.Count,
                out type,
                out integer,
                byteBuf,
                currentByteBufLength,
                out len,
                out elements,
                out replyObject);


            // STEP 3. Handle result.

            switch (type)
            {
                case REDIS_REPLY_STRING:
                    return handleStringResult(typeHint, replyObject, len);

                case REDIS_REPLY_ARRAY:
                    return handleArrayResult(typeHint, replyObject, elements);

                case REDIS_REPLY_INTEGER:
                    return handleIntegerResult(typeHint, integer);

                case REDIS_REPLY_NIL:
                    return null;

                case REDIS_REPLY_STATUS:
                    if (replyObject != IntPtr.Zero)
                    {
                        currentByteBufLength = len;
                        byteBuf = new byte[currentByteBufLength];
                        Interop.retrieveStringAndFreeReplyObjectX(replyObject, byteBuf);
                    }

                    if (typeHint == typeof(int) || typeHint == typeof(int?))
                    {
                        return int.Parse(enc.GetString(byteBuf, 0, len));
                    }
                    if (typeHint == typeof(long) || typeHint == typeof(long?))
                    {
                        return long.Parse(enc.GetString(byteBuf, 0, len));
                    }
                    if (typeHint == typeof(float) || typeHint == typeof(float?))
                    {
                        return float.Parse(enc.GetString(byteBuf, 0, len));
                    }
                    if (typeHint == typeof(double) || typeHint == typeof(double?))
                    {
                        return double.Parse(enc.GetString(byteBuf, 0, len));
                    }

                    return enc.GetString(byteBuf, 0, len);

                case REDIS_REPLY_ERROR:
                    if (replyObject != IntPtr.Zero)
                    {
                        currentByteBufLength = len ;
                        byteBuf = new byte[currentByteBufLength];
                        Interop.retrieveStringAndFreeReplyObjectX(replyObject, byteBuf);
                    }
                    throw new NhiredisException(enc.GetString(byteBuf, 0, len));

                default:
                    throw new NhiredisException("Unknown redis return type: " + type);

            }
        }

        private RedisContext _context;
        public RedisClient(string host, int port, TimeSpan timeout)
        {
            _context = RedisConnectWithTimeout(host, port, timeout);
        }

        public T RedisCommand<T>(params object[] arguments)
        {
            return RedisCommand<T>(_context, arguments);
        }

        public object RedisCommand(params object[] arguments)
        {
            return RedisCommand(_context, arguments);
        }
    }
}
