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
            return new RedisContext {NativeContext = Interop.n_redisConnectWithTimeout(host, port, seconds, milliseconds * 1000)};
        }

        public static T RedisCommand<T>(RedisContext context, string format, params object[] arguments)
        {
            object result = RedisCommandImpl(context, format, arguments, typeof(T));
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
            string format, params object[] arguments)
        {
            return RedisCommandImpl(context, format, arguments, null);
        }

        private static object RedisCommandImpl(
            RedisContext context, 
            string format,
            object[] arguments,
            Type typeHint)
        {
            const int defaultMaxStrLen = 64;
            int currentSbLen = defaultMaxStrLen;

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
                Interop.n_setupArgumentArray(arguments.Length, out argumentsPtr);
                for (int i = 0; i < arguments.Length; ++i)
                {
                    // currently don't support anything other than ascii string data.
                    Interop.n_setArgument(argumentsPtr, i, (string) arguments[i]);
                }
            }

            Interop.n_redisCommand(
                context.NativeContext,
                format,
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
                        Interop.n_retrieveStringAndFreeReplyObject(replyObject, sb);
                    }
                    return sb.ToString();

                case REDIS_REPLY_ARRAY:
                    List<object> result_o = (typeHint == null || typeHint == typeof(List<object>)) ? new List<object>() : null;
                    List<string> result_s = (typeHint == typeof(List<string>) || typeHint == typeof(Dictionary<string,string>)) ? new List<string>() : null;
                    List<long> result_i = typeHint == typeof(List<long>) ? new List<long>() : null;

                    if (replyObject != IntPtr.Zero)
                    {
                        for (int i=0; i<elements; ++i)
                        {
                            IntPtr strPtr;
                            Interop.n_retrieveElement(
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
                                Interop.n_retrieveElementString(replyObject, i, sb);
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

                                    if (typeHint == typeof(List<object>))
                                    {
                                        result_o.Add(null);
                                    }
                                    if (typeHint == typeof(List<string>) ||
                                        typeHint == typeof(Dictionary<string, string>))
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
                        Interop.n_freeReplyObject(replyObject);
                    }

                    if (result_s != null)
                    {
                        if (typeHint == typeof(Dictionary<string, string>))
                        {
                            var result_d = new Dictionary<string, string>();
                            int count = result_s.Count%2 == 0 ? result_s.Count : result_s.Count - 1;
                            for (int i=0; i<count; i += 2)
                            {
                                result_d.Add(result_s[i], result_s[i+1]);
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
                        Interop.n_retrieveStringAndFreeReplyObject(replyObject, sb);
                    }
                    return sb.ToString();

                case REDIS_REPLY_ERROR:
                    if (replyObject != IntPtr.Zero)
                    {
                        currentSbLen = len + 1;
                        sb = new StringBuilder(currentSbLen);
                        Interop.n_retrieveStringAndFreeReplyObject(replyObject, sb);
                    }
                    throw new Exception(sb.ToString());

                default:
                    throw new Exception("Unknown redis return type: " + type);

            }
        }

    }
}
