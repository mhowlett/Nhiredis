## Introduction

Nhiredis is a lighweight .NET wrapper around hiredis, the recommend C client for Redis. 


## Alternative .NET Redis Clients

There are two recommended .NET clients listed on redis.io - ServiceStack.Redis and BookSleeve. 
Why do we need another?

_ServiceStack.Redis_ - I have used this client for some time. The impetus for development of
Nhiredis was actually the lack of WATCH support in ServiceStack.Redis. One option would have
been to add support for this to this library, or wait for it to arrive, however there are two
other things I don't like so much about it: 

1. There are too many functions in a single namespace and the names of the functions are different
   to the actual Redis commands.
2. Too many dependencies. I just want a lightweight redis client, nothing else.

_Booksleeve_ - I haven't looked at this library detail, but on the surface it looks very 
promising. Unfortunately for those people constrained to working with C# 3, this is not an 
option.


## Building

update the hiredis_wrapper_win32 project 'include directories' and 'additional dependencies' to
point to the hiredis source directory and successfully built .lib.

antirez/hiredis on github currently does not build on Windows. I recommend my fork (mhowlett/hiredis),
which is the version I use in conjunction with Nhiredis.
