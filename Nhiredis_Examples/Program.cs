using System;
using System.Collections.Generic;
using Nhiredis;

namespace Nhiredis_Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = new RedisClient("localhost", 6379, TimeSpan.FromSeconds(2));

            // Send a PING command to the redis server and interpret the 
            // reply as a string.
            var pingReply = c.RedisCommand<string>("PING");
            Console.WriteLine(pingReply);

            // Set a key/value pair. The parameter 42 is not a string, nor is it
            // an IEnumerable or IDictionary (which would first be automatically 
            // flattened by Nhiredis). By default then, it will be interpreted as
            // a string by Nhiredis via application of the .ToString() object 
            // method.
            c.RedisCommand("SET", "foo", 42);

            // Get a value from redis, interpreting the result (which internally
            // will be a string for the GET command) as an int.
            int intResult = c.RedisCommand<int>("GET", "foo");
            Console.WriteLine(intResult);

            // Set multiple hash values. The dictionary parameter is flattened
            // automatically, so the following example is the same as calling:
            // c.RedisCommand("HMSET", "bar", "a", "7", "b", "\u00AE");
            // Unicode characters are supported, and will be encoded as UTF8 in Redis.
            var hashValues = new Dictionary<string, string> {{"a", "a"}, {"b", "\u00AE"}};
            c.RedisCommand("HMSET", "bar", hashValues);

            // Get all entries in a hash, interpreting the result as a 
            // Dictionary<string, string> (internally, redis returns 
            // an array of string values).
            var hashReply = c.RedisCommand<Dictionary<string, string>>("HGETALL", "bar");
            Console.WriteLine(hashReply["a"] + " " + hashReply["b"]);

            // return values can be interpreted as bools.
            if (c.RedisCommand<bool>("EXISTS", "foo"))
            {
                Console.WriteLine("Foo exists!");
            }

            // Example transaction. To test, put a break point before EXEC 
            // and change foo using the CLI.
            c.RedisCommand("WATCH", "foo");
            var foo = c.RedisCommand<int>("GET", "foo");
            c.RedisCommand("MULTI");
            c.RedisCommand("SET", "foo2", foo + 2);
            var execResult = c.RedisCommand<List<string>>("EXEC");
            if (execResult == null)
            {
                Console.WriteLine("EXEC failed!");
            }
            else
            {
                Console.WriteLine("Command status: " + execResult[0]);
            }

        }
    }
}
