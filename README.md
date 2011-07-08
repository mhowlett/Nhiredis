## Introduction

Nhiredis is a .NET client for Redis. It is a lighweight wrapper around hiredis, the recommend C client 
for Redis. 

Nhiredis can be used under both Windows and Linux/Mono.


## Other .NET Redis Clients

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

	// create a RedisContext
	var rc = RedisClient.RedisConnectWithTimeout(
                    "localhost", 6379, TimeSpan.FromSeconds(2));
	
	// use the strongly typed RedisCommand function to coerce a reply from redis into
	// a convenient data type:
	Dictionary<string, string> result
		 = RedisClient.RedisCommand<Dictionary<string, string>>(rc, "HGETALL", "thedict");
		 
		 
## Development Status

Currently, Nhiredis provides a wrapper around the blocking redisCommand function only (async 
funtion wrappers are not implemented). Of course, you can use this function to access the full
set of Redis commands. Currently only string parameters are supported.

With the core framework in place, it is not a difficult task to do the required remaining 
implementation, and I expect to do this in the coming months.


## Benchmarks

The distribution includes a benchmark utility that repeatedly sets, gets and deletes keys in a 
redis database using Nhiredis and ServiceStack.Redis. It produced the following results on my 
laptop:

* Nhredis was approximately 10% slower.
* but Nhiredis consumed approximate 10% less memory.


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
