## Introduction

Nhiredis is lighweight .NET wrapper for hiredis, a client for Redis written in C.

## Alternatives .NET Redis Clients

There are two recommended .NET redis clients on redis.io - ServiceStack.Redis and BookSleeve. 
Why another client?

_ServiceStack.Redis_ - This is the client I have been successfully using for some time. 
Unfortunately WATCH functionality isn't currently supported. One option was to add support to
this library, however there are two other things I don't like so much about this library: 

1. There are too many functions all in a single namespace and there isn't a clear mapping from
   Redis commands to the function names.
2. Too many dependencies. I just want a lightweight redis client, nothing else.


_Booksleeve_ - Haven't looked in detail, but on the surface it looks great. However,
for people still working with C# 3, it's not an option.


## Building

update the hiredis_wrapper_win32 project 'include directories' and 'additional dependencies' to
point to the hiredis source directory and successfully built .lib.

antirez/hiredis on github currently does not build on Windows. I recommend my fork (mhowlett/hiredis),
which is the version I use in conjunction with Nhiredis.
