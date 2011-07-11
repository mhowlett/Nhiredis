## Introduction

Nhiredis is a .NET client for Redis. It is a lightweight wrapper around hiredis, the recommend client for C developers. It provides a simple, yet flexible and complete API. And it is fast.

Nhiredis can be used under both Windows and Linux/Mono.


## Why Nhiredis?

There are two .NET clients recommended on redis.io - ServiceStack.Redis and BookSleeve. Why do we need Nhiredis?

**ServiceStack.Redis** - I am a long time user of this library. It does a reliable job, but the API doesn't really appeal to me. First of all the command function names are different from the actual Redis commands. The problem with this is I can never remember what the Redis commands are when I'm working with a different client, in particular the CLI. Also consider, for example, AddItemToList, EnqueueItemOnList and PushItemToList - they all do the same thing. Why the duplication? Why isn't RPUSH a better name than all three of them? (I regularly get confused as to whether items are placed at the front or end of the list with these functions).

_Compared to ServiceStack.Redis, Nhiredis provides a flexible, simple and elegant API - and it is faster - about 15% faster according to a simple locally run get-set-delete benchmark_

**Booksleeve** - I haven't looked at this library in detail, but on the surface it does look good. Like ServiceStack.Redis, this library provides separate C# functions for each Redis command, and again these aren't the same as the actual redis commands, but the choice of names seems to be better. Also, Booksleeve makes use of C# 4/5 features, so if you are constrained to working with .NET versions earlier than C# 4.0 like me, this is not an option. 

Nhiredis appears as to be about as fast as Booksleeve (probably a little bit faster) for the blocking case. Nhiredis does not yet support fire-and-forget functionality.


## Example

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
		 
I thoroughly enjoy using the Nhiredis API.


## Development Status

Nhiredis is used by the website http://backrecord.com. The data schema associated with this website
is large and complex enough that it pushes the boundaries of what is appropriate use of Redis. Although
backrecord.com is not publically accessible yet, it is under active development and Nhiredis is given
a workout every day. _I personally rely on Nhiredis_.

Currently, Nhiredis provides a wrapper around the (blocking) redisCommand function only (async 
function wrappers are not yet implemented). Of course, RedisCommand can be used to access the full
array of Redis functionality.

Only string parameters are currently supported, however binary parameters are fully implemented in the
hiredisx layer (see below) and it will be a fairly trivial exercise to add support to Nhiredis.dll; it
is not done yet only because I don't personally need it.

With the core framework in place, the remaining implementation is not a difficult task, and I
expect to do this in the coming months.



## Benchmarks

The distribution includes a simple benchmark utility that repeatedly sets, gets and deletes keys in a 
redis database using Nhiredis and ServiceStack.Redis. On my laptop, this utility suggests that:

* Nhiredis is between 15-20% faster than ServiceStack.Redis.
* and Nhiredis consumes approximately 5-10% less memory than ServiceStack.Redis.


## Building

You can download the binary files directly, however if you wish to build Nhiredis yourself, here's how:

### Windows

First, obtain hiredis and compile it. Note that antirez/hiredis on github currently does not
build on Windows. I recommend using my fork (mhowlett/hiredis), which is the version I 
personally use in conjunction with Nhiredis (note: the compatibility changes are largely
the work of others, not me).

The solution/project files were produced by Visual Studio 2008. In visual studio, update the
hiredisx project 'include directories' and 'additional dependencies' to point to the hiredis 
source directory and hiredis.lib that you have successfuly built.

Build the solution.


### Linux

Obtain hiredis and compile it.

There is no makefile to enable .NET libraries to be compiled with mono under linux. You should
either obtain the binary .dlls directly, compile Nhiredis under Windows and use the .dlls thus
produced under linux, or write a Makefile yourself... 

edit hiredisx/Makefile to reference hiredis.a you have successfully built.
build libhiredisx.so but typing "make" in the hiredisx directory.

copy libhiredisx.so into the same directory you will be using Nhiredis.dll from.


## Library Components

_Nhiredis_ is the library you reference in your application

_hiredisx_ (hiredis 'extra') is a C wrapper around hiredis. It serves two purposes:

1. It adds functionality that makes marshalling values between .NET and hiredis easier and
   more efficient.
2. Under Windows, it provides the definitions required to create a .dll (rather than a static
   library). This is required for interfacing with .NET.
