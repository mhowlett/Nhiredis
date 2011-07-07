/*
  Copyright (c) 2011, Matt Howlett
  All rights reserved.

  Redistribution and use in source and binary forms, with or without modification, 
  are permitted provided that the following conditions are met:

  1. Redistributions of source code must retain the above copyright notice, this list 
     of conditions and the following disclaimer.

  2. Redistributions in binary form must reproduce the above copyright notice, this 
     list of conditions and the following disclaimer in the documentation and/or other
     materials provided with the distribution.

  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
  OF SUCH DAMAGE. 
*/

#include "hiredis_wrapper_win32.h"

#define HIREDIS_WIN
#include "hiredis.h"
#include "config.h"

#include <windows.h>
#include <string.h>


HIREDIS_WRAPPER_WIN32_API
void *n_redisConnectWithTimeout(
		const char *ip, 
		int port, 
		int timeout_seconds, 
		int timeout_microseconds)
{
	struct timeval tv;
	tv.tv_sec = timeout_seconds;
	tv.tv_usec = timeout_microseconds;
	return redisConnectWithTimeout(ip, port, tv);
}


HIREDIS_WRAPPER_WIN32_API
void n_redisCommand(
		void *context,			// in:  the redisContext to use to execute the command.
		void *args,				// in:  arguments of command.
		int argc,				// in:  number of arguments.
		int *type,				// out: the type of result.
		long long *integer,		// out: if result is integer, the result.
		char *strBuf,			// out: if result is string, the result is coppied to this buffer if it is less than strBufLen-1.
		int strBufLen,  		// in:  the size of strBuf.
		int *len,				// out: the length of the string in strBuf, or pointed to by reply.
		int *elements,			// out: number of elements in a multi-reply. If > 0, reply is a pointer to the reply, and ... method should be used to get array and free the reply.
		void **reply			// out: if !NULL, the reply from redisCommand, which is yet to be freed.
		)
{
    redisReply *r;
	redisContext *c;
	int i;
	char **argv = (char **)args;

	size_t* argvlen = malloc(argc * sizeof(size_t));
	for (i=0; i<argc; ++i)
	{
		argvlen[i] = *((int *)argv[i]);
		argv[i] = ((int *)argv[i]) + 1;
	}

	r = redisCommandArgv((redisContext *)context, argc, argv, argvlen);

	free(argvlen);

	if (argc > 0)
	{
		for (i=0; i<argc; ++i)
		{
			argv[i] = ((int *)argv[i]) - 1;
			free(argv[i]);
		}
		free(argv);
	}
	
	*type = r->type;
	*integer = r->integer;
	*len = r->len;
	*elements = 0;
	*reply = NULL;

	if (r->type == REDIS_REPLY_STRING || r->type == REDIS_REPLY_ERROR || r->type == REDIS_REPLY_STATUS)
	{
		if (r->len < strBufLen-1)
		{
			strcpy(strBuf, r->str);
		}
		else
		{
			*reply = r;
			return; // don't free reply yet.
		}
	}

	if (r->type == REDIS_REPLY_ARRAY)
	{
		*elements = r->elements;
		*reply = r;
		return; // don't free reply yet.
	}

	freeReplyObject(r);
}


HIREDIS_WRAPPER_WIN32_API
void n_retrieveElement(
	void *reply, 
	int index, 
	int *type, 
	long long *integer, 
	char *strBuf, 
	int strBufLen,
	int *len,
	char **strPtr)
{
	redisReply *r = (redisReply *)reply;

	*type = r->element[index]->type;
	*integer = r->element[index]->integer;
	*len = r->element[index]->len;
	*strPtr = NULL;

	if (r->element[index]->type == REDIS_REPLY_STRING)
	{
		if (r->element[index]->len < strBufLen-1)
		{
			strcpy(strBuf, r->element[index]->str);
		}
		else
		{
			*strPtr = r->element[index]->str;
			return;
		}
	}
}


HIREDIS_WRAPPER_WIN32_API
void n_freeReplyObject(void *reply)
{
	freeReplyObject((redisReply *)reply);
}


HIREDIS_WRAPPER_WIN32_API
void n_retrieveStringAndFreeReplyObject(
		void *reply, 
		char *toStrPtr)
{
	strcpy(toStrPtr, ((redisReply *)reply)->str);
	freeReplyObject((redisReply *)reply);
}


HIREDIS_WRAPPER_WIN32_API
void n_retrieveElementString(
		void *reply,
		int index,
		char *toStrPtr)
{
	strcpy(toStrPtr, ((redisReply *)reply)->element[index]->str);	
}


HIREDIS_WRAPPER_WIN32_API
void n_setupArgumentArray(
	int length,
	void **arguments)
{
	*arguments = (void *)malloc(length * sizeof(char *));
}


HIREDIS_WRAPPER_WIN32_API
void n_setArgument(
   void *arguments,
   int index,
   void *argument,
   int len)
{
	char **args = (char **)arguments;
	args[index] = malloc(sizeof(int) + (len + 1) * sizeof(char));
	strcpy((char *)(((BYTE *)args[index]) + sizeof(int)), argument);
	*((int *)args[index]) = len;
}
