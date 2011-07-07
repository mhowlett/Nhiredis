## Introduction

Nhiredis is a lighweight .NET wrapper around hiredis, the recommend C client for Redis. 


## Alternative .NET Redis Clients

There are two recommended clients for .NET listed on redis.io - ServiceStack.Redis and BookSleeve. 
Why do we need another?

_ServiceStack.Redis_ - I have used this client for some time. The impetus for development of
Nhiredis was actually the lack of WATCH support in ServiceStack.Redis. It would have been
possible to add WATCH support to this library - or wait for it to arrive, however there are two
other issues I have with this library: 

1. There are too many functions in a single namespace and the names of the functions are different
   to the actual Redis commands.
2. I just want a lightweight redis client, nothing else. 

_Booksleeve_ - I haven't looked at this library in detail, but on the surface it looks very 
good. Unfortunately if you are constrained to working with C# 3.0, this is not an option.



## Building

antirez/hiredis on github currently does not build on Windows. I recommend my fork (mhowlett/hiredis),
which is the version I use in conjunction with Nhiredis.

update the hiredisx project 'include directories' and 'additional dependencies' to
point to the hiredis source directory and successfully built .lib.


## Component Overview

hiredisx (hiredis 'extra') is a C wrapper around hiredis. It serves two purposes:

1. Under Windows, it provides the definitions required to create a .dll (rather than a static
   library). This is required for interfacing with .NET.
2. It adds functionality that makes marshalling values between .NET and hiredis easier and
   more efficient.

