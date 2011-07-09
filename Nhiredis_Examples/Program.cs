using System;
using Nhiredis;

namespace Nhiredis_Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = new RedisClient("localhost", 6379, TimeSpan.FromSeconds(2));

            // Send a PING command to redis using the loosly typed version of
            // the RedisCommand function. Internally, the reply from redis is
            // of type STATUS, with a string value of "PONG". Nhiredis returns
            // the string value (in this case PONG), however discards the 
            // explicit fact that the response type was STATUS. If the result
            // of the command was ERROR, an exception would be thrown containing
            // the error message sent from Redis.
            object objectReply = c.RedisCommand("PING");
            Console.WriteLine(objectReply);

            string s = c.RedisCommand<string>("HGET", "ei-5", "u");

            // Send a PING command to redis using the strongly typed RedisCommand
            // function. If it happened that the reply from redis can not be 
            // reasonably interpreted as type string, an exception would be thrown.
            string stringReply = c.RedisCommand<string>("PING");
            Console.WriteLine(stringReply);

            // Set a value in redis (ignoring the return value). Internally the
            // reply from redis will be type status together with a string value
            // of OK.
            c.RedisCommand("SET", "foo", "123");

            // Get a value from redis using the strongly typed version of 
            // RedisCommand so the result is of type string, not object.
            string str = c.RedisCommand<string>("GET", "foo");
            Console.WriteLine(str);
        }
    }
}
