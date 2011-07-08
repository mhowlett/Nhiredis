## Introduction

Nhiredis is a .NET client for Redis. It is a lighweight wrapper around hiredis, the recommend C client 
for Redis.

Nhiredis can be used under both Windows and Linux/Mono.


## Why Nhiredis?

There are two recommended clients for .NET listed on redis.io - ServiceStack.Redis and BookSleeve. 
Why Nhiredis?

_ServiceStack.Redis_ - I have used this client for some time, and it does the job. However:

1. It's a bit ugly that there are so many functions clumped together in a single namespace.
2. The names of the functions are different to the actual Redis commands (so I can never remember 
   the Redis commands when I work with the CLI).
3. There is currently no support for the redis command WATCH.
4. It's somewhat more coupled to other components than I would like.

_Booksleeve_ - I haven't looked at this library in detail, but on the surface it looks very 
good. Unfortunately if you are constrained to working with .NET versions earlier than C# 4.0 
like me, this is not an option.


## Examples

        var c = new RedisClient("localhost", 6379, TimeSpan.FromSeconds(2));

        // Send a PING command to redis using the loosly typed version of
        // the RedisCommand function. Internally, the reply from redis is
        // of type STATUS, with a string value of "PONG". Nhiredis returns
        // the string value (in this case PONG), however discards the 
        // explicit fact that the response type was STATUS. If the result
        // of the command was ERROR, an exception would be thrown containing
        // the error message sent from Redis.
        object objectReply = c.RedisCommand("PING");

        // Send a PING command to redis using the strongly typed RedisCommand
        // function. If it happened that the reply from redis can not be 
        // reasonably interpreted as type string, an exception would be thrown.
         string stringReply = c.RedisCommand<string>("PING");

        // Set a value in redis (ignoring the return value). Internally the
        // reply from redis will be type status together with a string value
        // of OK.
        c.RedisCommand("SET", "foo", "123");

        // Get a value from redis using the strongly typed version of 
        // RedisCommand so the result is of type string, not object.
        string str = c.RedisCommand<string>("GET", "foo");

        // List results from Redis can be interpreted as a dictionary, where
        // this is appropriate:
        Dictionary<string,string> result 
              = c.RedisCommand<Dictionary<string, string>>("HGETALL", "bar");
		 
		 
## Development Status

Currently, Nhiredis provides a wrapper around the blocking redisCommand function only (async 
funtion wrappers are not implemented). Of course, you can use this function to access the full
set of Redis commands. Currently only string parameters are supported, however it is a
a fairly trivial excersise to add support for binary parameters, something that is not 
yet done because I don't need it.

With the core framework in place, it is not a difficult task to do the required remaining 
implementation, and I expect to do this in the coming months.


## Benchmarks

The distribution includes a benchmark utility that repeatedly sets, gets and deletes keys in a 
redis database using Nhiredis and ServiceStack.Redis. It produced the following results on my 
laptop:

* Nhredis was approximately 10% slower.
* but Nhiredis consumed approximate 10% less memory.

The slightly worse performance is annoying, however:

* In practice, this will not normally be of concern. A more appealing (and complete) interface
  is more important to me, anyway.
* I have a few optimizations in mind that will hopefully help.
* Nhiredis can ultimately be extended with a fire-and-forget layer, which will allow for better
  performance in some scenarios.


## Building

You can download the binary files directly, however if you wish to build yourself, here's how:

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


## Library Component

_Nhiredis_ is the library you reference in your application

_hiredisx_ (hiredis 'extra') is a C wrapper around hiredis. It serves two purposes:

1. It adds functionality that makes marshalling values between .NET and hiredis easier and
   more efficient.
2. Under Windows, it provides the definitions required to create a .dll (rather than a static
   library). This is required for interfacing with .NET.
