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
using Nhiredis;

namespace Nhiredis_Test
{
    public class Program
    {
        static void Main(string[] args)
        {
            var rc = RedisClient.RedisConnectWithTimeout(
                    "localhost", 6379, TimeSpan.FromSeconds(2));

            // Send a PING command to redis using the loosly typed version of
            // the RedisCommand function. Internally, the reply from redis is
            // of type STATUS, with a string value of "PONG". Nhiredis discards
            // the fact that the reply type was STATUS, however returns
            // the string value (in this case PONG). If the result of the 
            // query was ERROR, an exception would be thrown containing the 
            // error message.
            object objectReply = RedisClient.RedisCommand(rc, "PING");
            Console.WriteLine(objectReply);

            // Send a PING command to redis using the strongly typed RedisCommand
            // function. If it happened that the reply from redis can not be 
            // reasonably interpreted as type string, an exception would be thrown.
            string stringReply = RedisClient.RedisCommand<string>(rc, "PING");
            Console.WriteLine(stringReply);

            // Set a value in redis (ignoring the return value). Internally the
            // reply from redis will be type status together with a string value
            // of OK.
            RedisClient.RedisCommand(rc, "SET", "foo", "123");
            
            // Get a value from redis, explicitly defining the result is expected
            // to be a string.
            string str = RedisClient.RedisCommand<string>(rc, "GET", "foo");
            Console.WriteLine(str);
        }
    }
}
