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
        static void Nhiredis_Benchmark()
        {
            var c = new RedisClient("localhost", 6382, TimeSpan.FromSeconds(2));

            DateTime startTime = DateTime.UtcNow;

            for (int i = 0; i < 10000; ++i)
            {
                string key = Guid.NewGuid().ToString();
                string value = Guid.NewGuid().ToString();

                c.RedisCommand("SET", key, value);
                var v = c.RedisCommand<string>("GET", key);
                System.Diagnostics.Debug.Assert(v == value);
                var r = c.RedisCommand<long>("DEL", key);
                System.Diagnostics.Debug.Assert(r == 1);
            }

            TimeSpan executionTime = DateTime.UtcNow - startTime;

            Console.WriteLine("Execution time: " + executionTime);
        }

        static void ServiceStack_Benchmark()
        {
            ServiceStack.Redis.RedisClient c = new ServiceStack.Redis.RedisClient("localhost", 6382);

            DateTime startTime = DateTime.UtcNow;

            for (int i = 0; i < 10000; ++i)
            {
                string key = Guid.NewGuid().ToString();
                string value = Guid.NewGuid().ToString();

                c.Set(key, value);
                string v = c.GetValue(key);
                System.Diagnostics.Debug.Assert(v == value);
                int r = c.Del(key);
                System.Diagnostics.Debug.Assert(r == 1);
            }

            TimeSpan executionTime = DateTime.UtcNow - startTime;

            Console.WriteLine("Execution time: " + executionTime);
        }

        static void Main(string[] args)
        {
            Nhiredis_Benchmark();
            ServiceStack_Benchmark();
        }

    }
}
