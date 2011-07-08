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

        public static RedisContext RedisConnectWithTimeout(string host, int port, TimeSpan timeout)
        {
            int seconds = (int)(timeout.TotalSeconds);
            int milliseconds = (int)(timeout.TotalMilliseconds - seconds*1000);
            return new RedisContext {NativeContext = Interop.redisConnectWithTimeout(host, port, seconds, milliseconds * 1000)};
        }

        public static T RedisCommand<T>(RedisContext context, params object[] arguments)
        {
            object result = RedisCommandImpl(context, arguments, typeof(T));
            if (!(result is T))
            {
                if (result != null)
                {
                    throw new Exception(
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
            int currentSbLen = 64;

            int type;
            long integer;
            var sb = new StringBuilder(currentSbLen);
            int elements;
            IntPtr replyObject;
            int len;

            // currently only support string format arguments.
            IntPtr argumentsPtr = IntPtr.Zero;
            if (arguments.Length > 0)
            {
                Interop.setupArgumentArray(arguments.Length, out argumentsPtr);
                for (int i = 0; i < arguments.Length; ++i)
                {
                    // currently don't support anything other than ascii string data.
                    Interop.setArgument(argumentsPtr, i, (string) arguments[i], ((string) arguments[i]).Length);
                }
            }

            Interop.redisCommand(
                context.NativeContext,
                argumentsPtr,
                arguments.Length,
                out type,
                out integer,
                sb,
                currentSbLen,
                out len,
                out elements,
                out replyObject);

            switch (type)
            {
                case REDIS_REPLY_STRING:
                    if (replyObject != IntPtr.Zero)
                    {
                        currentSbLen = len + 1;
                        sb = new StringBuilder(currentSbLen);
                        Interop.retrieveStringAndFreeReplyObject(replyObject, sb);
                    }
                    return sb.ToString();

                case REDIS_REPLY_ARRAY:
                    List<object> result_o = (typeHint == null || typeHint == typeof (List<object>))
                                                ? new List<object>()
                                                : null;
                    List<string> result_s = (typeHint == typeof (List<string>) ||
                                             typeHint == typeof (Dictionary<string, string>))
                                                ? new List<string>()
                                                : null;
                    List<long> result_i = typeHint == typeof (List<long>) ? new List<long>() : null;

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
                                sb,
                                currentSbLen,
                                out len,
                                out strPtr);

                            if (strPtr != IntPtr.Zero)
                            {
                                currentSbLen = len + 1;
                                sb = new StringBuilder(currentSbLen);
                                Interop.retrieveElementString(replyObject, i, sb);
                            }

                            switch (type)
                            {
                                case REDIS_REPLY_STRING:
                                    if (result_s != null)
                                    {
                                        result_s.Add(sb.ToString());
                                    }
                                    else if (result_o != null)
                                    {
                                        result_o.Add(sb.ToString());
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
                                    break;

                                case REDIS_REPLY_ARRAY:
                                    //
                                    break;
                                case REDIS_REPLY_NIL:

                                    if (typeHint == typeof (List<object>))
                                    {
                                        result_o.Add(null);
                                    }
                                    if (typeHint == typeof (List<string>) ||
                                        typeHint == typeof (Dictionary<string, string>))
                                    {
                                        result_s.Add(null);
                                    }
                                    break;
                                case REDIS_REPLY_STATUS:
                                    //
                                    break;
                                case REDIS_REPLY_ERROR:
                                    //
                                    break;

                                default:
                                    throw new Exception("Unknown redis return type: " + type);
                            }
                        }
                        Interop.freeReplyObject(replyObject);
                    }

                    if (result_s != null)
                    {
                        if (typeHint == typeof (Dictionary<string, string>))
                        {
                            var result_d = new Dictionary<string, string>();
                            int count = result_s.Count%2 == 0 ? result_s.Count : result_s.Count - 1;
                            for (int i = 0; i < count; i += 2)
                            {
                                result_d.Add(result_s[i], result_s[i + 1]);
                            }
                            return result_d;
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

                    return null;

                case REDIS_REPLY_INTEGER:
                    return integer;

                case REDIS_REPLY_NIL:
                    return null;

                case REDIS_REPLY_STATUS:
                    if (replyObject != IntPtr.Zero)
                    {
                        currentSbLen = len + 1;
                        sb = new StringBuilder(currentSbLen);
                        Interop.retrieveStringAndFreeReplyObject(replyObject, sb);
                    }
                    return sb.ToString();

                case REDIS_REPLY_ERROR:
                    if (replyObject != IntPtr.Zero)
                    {
                        currentSbLen = len + 1;
                        sb = new StringBuilder(currentSbLen);
                        Interop.retrieveStringAndFreeReplyObject(replyObject, sb);
                    }
                    throw new Exception(sb.ToString());

                default:
                    throw new Exception("Unknown redis return type: " + type);

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
