## Introduction

Nhiredis is a .NET client for Redis. It is a lighweight wrapper around hiredis, the recommend C client 
for Redis. 

Nhiredis can be used under both Windows and Linux/Mono.


## Other .NET Redis Clients

There are two recommended clients for .NET listed on redis.io - ServiceStack.Redis and BookSleeve. 
Why do we need another?

_ServiceStack.Redis_ - I have used this client for some time, and it does the job. However:

1. It's a bit ugly that there are so many functions clumped together in a single namespace.
2. The names of the functions are different to the actual Redis commands (so I can never remember 
   the Redis commands when I work with the CLI).
3. There is currently no support for the redis command WATCH.
4. It's somewhat more coupled to other components than I would like.

_Booksleeve_ - I haven't looked at this library in detail, but on the surface it looks very 
good. Unfortunately if you are constrained to working with .NET versions earlier than C# 4.0 like
me, this is not an option.


## Examples

	// create a RedisContext
	var rc = RedisClient.RedisConnectWithTimeout(
                    "localhost", 6379, TimeSpan.FromSeconds(2));
	
	// use the strongly typed RedisCommand function to coerse a reply from redis into
	// a convenient data type:
	Dictionary<string, string> result
		 = RedisClient.RedisCommand<Dictionary<string, string>>(rc, "HGETALL", "testdictionary");
		 
		 
## Development Status

Currently, Nhiredis provides a wrapper around the blocking redisCommand function only (async 
funtion wrappers are not implemented). Of course, you can use this function to access the full
set of Redis commands.

With the core framework in place, it is not a difficult task to do the required implementation
for other methods, and I expect to do this in the coming months.


## Benchmarks

the distribution includes a benchmark test that repeatedly sets, gets and deletes keys in a redis
database using Nhiredis and ServiceStack.Redis. It produced the following results on my laptop:

* Running time was approximately 10% slower with Nhiredis.
* Nhiredis consumed approximate 10% less memory.


## Building

antirez/hiredis on github currently does not build on Windows. I recommend my fork (mhowlett/hiredis),
which is the version I use in conjunction with Nhiredis.

update the hiredisx project 'include directories' and 'additional dependencies' to
point to the hiredis source directory and successfully built .lib.


## Component Overview

hiredisx (hiredis 'extra') is a C wrapper around hiredis. It serves two purposes:

1. It adds functionality that makes marshalling values between .NET and hiredis easier and
   more efficient.
2. Under Windows, it provides the definitions required to create a .dll (rather than a static
   library). This is required for interfacing with .NET.

