## Introduction

Nhiredis is a .NET client for Redis. It is a lightweight wrapper around hiredis, the official C client. It provides a simple, flexible API.

Nhiredis can be used under both Windows and Linux/Mono and works with the aspnet50 target. It's available as a package on nuget.org


## Why Nhiredis?

I built Nhiredis because it provides the API I would most like to use. Highlights:

1. Parameters and return types of the RedisCommand (which you use for everything) can be conveniently coerced into what you need. This is very flexible: 

            c.RedisCommand("SET", "foo", 42);
            c.RedisCommand<int>("GET", "foo");
            c.RedisCommand<int?>("GET", "foo");
            c.RedisCommand("HMSET", "foo", myDict);
            // note: myDict is automatically flattened into
            // key1, value1, key2, value2, ... 
            c.RedisCommand<Dictionary<string, string>>("HGETALL", "foo");
	    
2. You don't need to learn one set of commands for the C# client and another for the CLI - the redis command name is just passed as the first parameter of the RedisCommand function.


## Longer Example

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
            
            // binary values / return types are supported:
            c.RedisCommand("SET", "foo", new byte[] {0, 4, 3});


## Development Status

Nhiredis is used by the website http://backrecord.com as well as a number of other projects - it is given a workout every day.

Currently, Nhiredis provides a wrapper around the (blocking) redisCommand function only (async 
functionality is not yet implemented). Of course, RedisCommand can be used to access the full
array of Redis functionality.

The current version is 0.9.0.


## Benchmarks

The distribution includes a simple benchmark utility that repeatedly sets, gets and deletes keys in a 
redis database using Nhiredis and ServiceStack.Redis. On my Windows laptop, this utility suggests that 
the performance of Nhiredis is approximately the same as ServiceStack.Redis.


## Building

Library files are included in the repository but if you wish to build Nhiredis yourself, here's how:

### Windows

1. Obtain an [OpenMSTech version of redis](https://github.com/msopentech/redis) which has been
   modified to compile under Windows.
2. In this project, set Config Properties / C/C++ / Code Generation / Struct Member Alignment 
   to 1 Byte for both Win32 and x64 targets for the hiredis and Win32_Interop projects.
3. Compile in Release mode for both Win32 and x64 targets. It is important to use Release
   mode so as to link against the runtime libraries that are redistributable.
4. In Visual Studio, update the hiredisx project 'include directories' and 'additional dependencies'
   for both Win32 and x64 targets to point to the hiredis source directory and hiredis.lib that 
   you have successfuly built. You also need to link against Win32_Interop.lib, that was 
   built by OpenMSTech redis.
5. The hiredisx project should now build (for both Win32 and x64 targets) creating hiredisx.dll
   for both targets.
6. Manually copy these .dlls to the Nhiredis project under x68 and x64. Set the CopyToOutputDirectory
   property on these files to something other than Do Not Copy.
7. If you want to use Nhiredis under linux, you'll need to create libhiredisx.so for x86 and x64
   and add them to the Nhiredis solution as well.
8. Run the nugetpkg_make.bat script in the Nhiredis project directory.
9. in the nugetpkg directory this script creates, run NuGet pack.


### Linux

Obtain hiredis and compile it.

There is no makefile to enable .NET libraries to be compiled with mono under linux. You can compile
Nhiredis under Windows and use the .dlls thus produced under linux, or write a Makefile yourself... 

edit hiredisx/Makefile to reference hiredis.a you have successfully built.
build libhiredisx.so but typing "make" in the hiredisx directory.


## Library Components

_Nhiredis_ is the library you reference in your application

_hiredisx_ (hiredis 'extra') is a C wrapper around hiredis. It serves two purposes:

1. It adds functionality that makes marshalling values between .NET and hiredis easier and
   more efficient.
2. Under Windows, it provides the definitions required to create a .dll (rather than a static
   library). This is required for interfacing with .NET.

