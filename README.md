## Introduction

Nhiredis is a .NET client for Redis. It is a lighweight wrapper around hiredis, the recommend client for C developers.

Nhiredis can be used under both Windows and Linux/Mono.


## Why Nhiredis?

There are two .NET clients recommended on redis.io - ServiceStack.Redis and BookSleeve. Why do we need Nhiredis?

_ServiceStack.Redis_ - I am a long time user of this library. What I don't like about it are the command function names. Firstly, they are not the same as the actual Redis commands so I can never remember what the Redis commands are when I'm working with a different client, in particular the CLI. Also, I don't really like the choice of names. For example AddItemToList, EnqueueItemOnList and PushItemToList which all do the same thing. But do they place items at the front or end of a list? Why the duplication? Why isn't RPUSH a better name than all three of them?

_Booksleeve_ - I haven't looked at this library in detail, but on the surface it looks very good. Unfortunately if you are constrained to working with .NET versions earlier than C# 4.0 like me, this is not an option.


## Examples

            var c = new RedisClient("localhost", 6379, TimeSpan.FromSeconds(2));

            // Send a PING command to the redis server and interpret the 
            // reply as a string.
            var pingReply = c.RedisCommand<string>("PING");
            Console.WriteLine(pingReply);

            // Set a key/value pair. The parameter 42 is not a string, nor is it
            // an IEnumerable or IDictionary (which would first be automatically 
            // flattened by Nhiredis). By default then, it will be interpreted as
            // a string via application of the .ToString() object method.
            c.RedisCommand("SET", "foo", 42);

            // Get a value from redis, interpreting the result as an int.
            int intResult = c.RedisCommand<int>("GET", "foo");
            Console.WriteLine(intResult);

            // Set multiple hash values. The dictionary parameter is flattened
            // automatically, so the following is the same as calling:
            // c.RedisCommand("HMSET", "bar", "a", "7", "b", "\u00AE");
            // Unicode characters are supported, and will be encoded as UTF8
            // in Redis.
            var hashValues = new Dictionary<string, string> {{"a", "a"}, {"b", "\u00AE"}};
            c.RedisCommand("HMSET", "bar", hashValues);

            // Get all entries in a hash, interpreting the result as a 
            // Dictionary<string, string>
            var hashReply = c.RedisCommand<Dictionary<string, string>>("HGETALL", "bar");
            Console.WriteLine(hashReply["a"] + " " + hashReply["b"]);
		 
		 
## Development Status

Nhiredis is used by the website http://backrecord.com. The data schema associated with this website
is large and complex enough that it pushes the boundaries of what is appropriate use of Redis. Although
backrecord.com is not publically accessible yet, it is under active development and Nhiredis is given
a workout every day. _I personally rely on Nhiredis_.

Currently, Nhiredis provides a wrapper around the (blocking) redisCommand function only (async 
function wrappers are not yet implemented). Of course, RedisCommand can be used to access the full
array of Redis functionality. Also, only string parameters are currently supported (Unicode is stored
as UTF8). It would be a fairly trivial exercise to add support for binary parameters; it is not done
yet only because I don't personally need it.

With the core framework in place, the remaining implementation is not a difficult task, and I
expect to do this in the coming months.



## Benchmarks

The distribution includes a simple benchmark utility that repeatedly sets, gets and deletes keys in a 
redis database using Nhiredis and ServiceStack.Redis. On my laptop, this utility suggests that:

* Nhiredis is between 1-2% slower than ServiceStack.Redis.
* but Nhiredis consumes approximately 5-10% less memory than ServiceStack.Redis.

These differences will not be significant in any practical situation. That said, it would be nice to be able to say Nhiredis is faster than ServiceStack.Redis, and I have a few optimizations in mind that i'll probably implement that may be able to achieve this.


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
