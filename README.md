## Introduction

Nhiredis is a .NET client for Redis. It is a lighweight wrapper around hiredis, the recommend C client.

Nhiredis can be used under both Windows and Linux/Mono.


## Why Nhiredis?

redis.io recommends two clients for .NET - ServiceStack.Redis and BookSleeve. Why do we need Nhiredis?

_ServiceStack.Redis_ - I have used this client for some time. What I don't like about it is the command function names are not the same as the actual Redis commands (which in turn means that I can never remember the Redis commands when I work with a different client, in particular the CLI). Also, in some cases, for example AddItemToSortedSet, the order of the parameters is different. Also, I don't think some of the names are very good. For example we have AddItemToList, EnqueueItemOnList and PushItemToList which all do the same thing. When I'm coding and look through the list of methods and see each of these, I wonder which, if any add to the left, or right. Ditto for removing elements.

My original list of complaints was a longer, but when I went to justify them here (having already built Nhiredis), I found I couldn't - my attitude was just overly tainted by the above. In hindsight, the functionality underlying ServiceStack.Redis is pretty good and using a decent API on top of this would have probably been a better solution that wrapping hiredis.

_Booksleeve_ - I haven't looked at this library in detail, but on the surface it looks very good. Unfortunately if you are constrained to working with .NET versions earlier than C# 4.0 like me, this is not an option.


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

Nhiredis is used by the website http://backrecord.com. The data schema associated with this website
is large and complex enough that it pushes the boundaries of what is appropriate use of Redis. Although
backrecord.com is not publically accessible yet, it is under active development and Nhiredis is given
a workout every day. _I personally rely on Nhiredis_.

Currently, Nhiredis provides a wrapper around the (blocking) redisCommand function only (async 
function wrappers are not yet implemented). Of course, RedisCommand can be used to access the full
array of Redis functionality. Also, only string parameters are currently supported. It would be a
fairly trivial exercise to add support for binary parameters; it is not done yet only because I 
don't personally need it.

With the core framework in place, the remaining implementation is not a difficult task, and I
expect to do this in the coming months.


## Benchmarks

The distribution includes a benchmark utility that repeatedly sets, gets and deletes keys in a 
redis database using Nhiredis and ServiceStack.Redis. It produced the following results on my 
laptop:

* Nhiredis was approximately 10% slower than ServiceStack.Redis.
* but Nhiredis consumed approximately 10% less memory than ServiceStack.Redis.
* CPU usage was approximately 30-40% higher with Nhiredis.

The worse performance is a bit annoying, however:

* This is not a practical concern for most people. A more appealing (and complete) interface
  is more important.
* I have a few optimizations in mind that will hopefully help the situation. It will also be
  interesting to benchmark hiredis itself against ServiceStack.Redis to understand better where
  the inefficiencies lie.
* Nhiredis will likely ultimately be extended with a fire-and-forget layer, which will allow 
  for better performance in some scenarios.


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
