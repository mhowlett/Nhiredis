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
            var result = new RedisContext {NativeContext = Interop.redisConnectWithTimeout(ipBytes, ipBytes.Length, port, seconds, milliseconds * 1000)};
            if (result.NativeContext == IntPtr.Zero)
            {
                throw new NhiredisException("Unable to establish redis connection [" + host + ":" + port + "]");
            }
            
            return result;
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

            // flatten arguments list, and interpret all elements as a string as appropriate.
            var args = new ArrayList();
            for (int i=0; i<arguments.Length; ++i)
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
                        args.Add(v.ToString());
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

            IntPtr argumentsPtr = IntPtr.Zero;
            if (args.Count > 0)
            {
                Interop.setupArgumentArray(args.Count, out argumentsPtr);
                for (int i = 0; i < args.Count; ++i)
                {
                    if (args[i] is byte[])
                    {
                        Interop.setArgument(argumentsPtr, i, (byte[])args[i], ((byte[])args[i]).Length);
                    }
                    else
                    {
                        byte[] arg = StringToUtf8((string) args[i]);
                        Interop.setArgument(argumentsPtr, i, arg, arg.Length);
                    }
                }
            }

            Interop.redisCommand(
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

            switch (type)
            {
                case REDIS_REPLY_STRING:
                    if (replyObject != IntPtr.Zero)
                    {
                        currentByteBufLength = len ;
                        byteBuf = new byte[currentByteBufLength];
                        Interop.retrieveStringAndFreeReplyObject(replyObject, byteBuf);
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
                    if (typeHint == typeof(bool) || typeHint == typeof(bool?))
                    {
                        string s = enc.GetString(byteBuf, 0, len);
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
                        // else don't understand.
                    }
                    if (typeHint == typeof(byte[]))
                    {
                        byte[] bytes = new byte[len];
                        for (int i=0; i<len; ++i)
                        {
                            bytes[i] = byteBuf[i];
                        }
                        return bytes;
                    }

                    return enc.GetString(byteBuf, 0, len);

                case REDIS_REPLY_ARRAY:
                    List<object> result_o = (typeHint == null || typeHint == typeof (List<object>))
                                                ? new List<object>()
                                                : null;
                    List<string> result_s = (typeHint == typeof (List<string>) ||
                                             typeof (IDictionary).IsAssignableFrom(typeHint))
                                                ? new List<string>()
                                                : null;
                    List<long> result_i = typeHint == typeof (List<long>) ? new List<long>() : null;
                    List<byte[]> result_b = typeHint == typeof (List<byte[]>) ? new List<byte[]>() : null;
 
                    if (replyObject != IntPtr.Zero)
                    {
                        for (int i = 0; i < elements; ++i)
                        {
                            IntPtr strPtr;
                            Interop.retrieveElement(
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
                                Interop.retrieveElementString(replyObject, i, byteBuf);
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
                                        for (int k=0; k<len; ++k)
                                        {
                                            res[k] = byteBuf[k];
                                        }
                                        result_b.Add(res);
                                    }
                                    else if (result_i != null)
                                    {
                                        long result;
                                        if (!long.TryParse(enc.GetString(byteBuf, 0, len),out result))
                                        {
                                            result = long.MinValue;
                                        }
                                        result_i.Add(result);
                                    }
                                    break;

                                case REDIS_REPLY_INTEGER:
                                    if (result_i != null)
                                    {
                                        result_i.Add(integer);
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
                                    else if (result_i != null)
                                    {
                                        result_i.Add(long.MinValue);
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
                                    else if (result_i != null)
                                    {
                                        long result;
                                        if (!long.TryParse(enc.GetString(byteBuf, 0, len), out result))
                                        {
                                            result = long.MinValue;
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
                                    else if (result_i != null)
                                    {
                                        result_i.Add(long.MinValue);
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
                        Interop.freeReplyObject(replyObject);
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
                    if (result_i != null)
                    {
                        return result_i;
                    }
                    if (result_b != null)
                    {
                        return result_b;
                    }

                    return null;

                case REDIS_REPLY_INTEGER:
                    if (typeHint == typeof(int) || typeHint == typeof(int?))
                    {
                        return (int) integer;
                    }
                    if (typeHint == typeof(string))
                    {
                        return integer.ToString();
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
                        // else don't understand.
                    }
                    return integer;

                case REDIS_REPLY_NIL:
                    return null;

                case REDIS_REPLY_STATUS:
                    if (replyObject != IntPtr.Zero)
                    {
                        currentByteBufLength = len;
                        byteBuf = new byte[currentByteBufLength];
                        Interop.retrieveStringAndFreeReplyObject(replyObject, byteBuf);
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
                        Interop.retrieveStringAndFreeReplyObject(replyObject, byteBuf);
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
