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

#include "hiredisx.h"

#if defined(_MSC_VER)
//#include "config.h"
#endif

#include "hiredis.h"

#include <string.h>
#include <stdlib.h>
#include <assert.h>

//#define DEBUGVERBOSE

#ifdef DEBUGVERBOSE
#include <stdio.h>
#endif

HIREDISX_API
void* redisConnectWithTimeoutX(
		const char *ip,
		int ipLen,
		int port, 
		int timeout_seconds, 
		int timeout_microseconds)
{
#ifdef DEBUGVERBOSE
	printf("redisConnectWithTimeoutX:\n");
#endif
	redisContext *c;
	struct timeval tv;
	int i;

	char *ipStr = (char *)malloc((ipLen + 1) * sizeof(char *));
	for (i = 0; i<ipLen; ++i) { ipStr[i] = ip[i]; }
	ipStr[ipLen] = '\0';
#ifdef DEBUGVERBOSE
	printf(" ip: %s\n", ipStr);
	printf(" timeout_seconds: %d\n", timeout_seconds);
	printf(" timeout_microseconds: %d\n", timeout_microseconds);
	printf(" port: %d\n", port);
#endif

	tv.tv_sec = timeout_seconds;
	tv.tv_usec = timeout_microseconds;
	c = redisConnectWithTimeout(ipStr, port, tv);
	
	free(ipStr);

	if (c->err)
	{
        // TODO: do something better with c->errstr
		return NULL;
    }

	return (void *)c;
}

HIREDISX_API
void redisCommandX(
		void *context,
		void *args,
		int argc,
		int *type,
		long long *integer,
		char *strBuf,
		int strBufLen,
		int *len,
		int *elements,
		void **reply)
{
#ifdef DEBUGVERBOSE
	printf("redisCommandX:\n");
#endif

    redisReply *r = NULL;
	int i;
	char **argv = (char **)args;

	size_t* argvlen = (size_t *)malloc(argc * sizeof(size_t));
	for (i=0; i<argc; ++i)
	{
		argvlen[i] = *((int *)argv[i]);
		argv[i] = (char *)(((int *)argv[i]) + 1);
#ifdef DEBUGVERBOSE
		printf(" parameter #%d: len=%d, val=(%s)\n", i, argvlen[i], argv[i]);
#endif
	}

	r = (redisReply *)redisCommandArgv((redisContext *)context, argc, (const char **)argv, argvlen);

	free(argvlen);

	if (argc > 0)
	{
		for (i=0; i<argc; ++i)
		{
			argv[i] = (char *)(((int *)argv[i]) - 1);
			free(argv[i]);
		}
		free(argv);
	}
	
	*type = r->type;
	*integer = r->integer;
	*len = r->len;
	*elements = 0;
	*reply = NULL;

#ifdef DEBUGVERBOSE
	printf(" r->type: %d\n", r->type);
	printf(" r->integer: %d\n", r->integer);
	printf(" r->len: %d\n", r->len);
	printf(" r->str (addr): %d\n", r->str);
	if (r->len > 0 && r->type == REDIS_REPLY_STRING)
	{
		printf(" reply->str = %s\n", r->str);
	}
#endif

	// for these 4 cases, don't free the reply - still need it to get info out of.

	if (r->type == REDIS_REPLY_STRING || r->type == REDIS_REPLY_ERROR || r->type == REDIS_REPLY_STATUS)
	{
		*reply = r;
		return;
	}

	if (r->type == REDIS_REPLY_ARRAY)
	{
		*elements = r->elements;
		*reply = r;
		return; 
	}

	// for these cases, we're done with the reply already.

	if (r->type == REDIS_REPLY_NIL || r->type == REDIS_REPLY_INTEGER)
	{
		freeReplyObject(r);
		return;
	}

	// unknown reply type.
	assert(0);
}

HIREDISX_API
void retrieveElementX(
	void *reply, 
	int index, 
	int *type, 
	long long *integer, 
	char *strBuf, 
	int strBufLen,
	int *len,
	char **strPtr)
{
	int i;
	redisReply *r = (redisReply *)reply;

	*type = r->element[index]->type;
	*integer = r->element[index]->integer;
	*len = r->element[index]->len;
	*strPtr = NULL;

	if (r->element[index]->type == REDIS_REPLY_STRING || r->element[index]->type == REDIS_REPLY_ERROR || r->element[index]->type == REDIS_REPLY_STATUS)
	{
		if (r->element[index]->len <= strBufLen)
		{
			for (i=0; i<*len; ++i)
			{
				strBuf[i] = r->element[index]->str[i];
			}
		}
		else
		{
			*strPtr = r->element[index]->str;
			return;
		}
	}
}

HIREDISX_API
void freeReplyObjectX(void *reply)
{
	freeReplyObject((redisReply *)reply);
}

HIREDISX_API
void retrieveStringAndFreeReplyObjectX(
		void *reply, 
		char *toStrPtr)
{
#ifdef DEBUGVERBOSE
	printf("retrieveStringAndFreeReplyObjectX:\n");
	printf(" reply->len = %d\n", ((redisReply *)reply)->len);
	printf(" reply->str = ");
#endif
	int i;
	for (i=0; i<((redisReply *)reply)->len; ++i)
	{
#ifdef DEBUGVERBOSE
		printf("%c", ((redisReply *)reply)->str[i]);
#endif
		toStrPtr[i] = ((redisReply *)reply)->str[i];
	}
#ifdef DEBUGVERBOSE
	printf("\n");
#endif
	freeReplyObject((redisReply *)reply);
}

HIREDISX_API
void retrieveElementStringX(
		void *reply,
		int index,
		char *toStrPtr)
{
	int i;
	for (i=0; i<((redisReply *)reply)->element[index]->len; ++i)
	{
		toStrPtr[i] = ((redisReply *)reply)->element[index]->str[i];
	}
}

HIREDISX_API
void setupArgumentArrayX(
	int length,
	char **arguments)
{
	*arguments = (char *)malloc(length * sizeof(char *));
}

HIREDISX_API
void setArgumentX(
   void *arguments,
   int index,
   void *argument,
   int len)
{
	int i;
	char **args = (char **)arguments;
	args[index] = (char *)malloc(sizeof(int) + 1 + len * sizeof(char));
	for (i = 0; i<len; ++i) 
	{
		((char *)(args[index] + sizeof(int)))[i] = ((char *)argument)[i];
	}
	((char *)(args[index] + sizeof(int)))[len] = '\0'; // unnecessary for redis. useful for debug verbose printf's.
	*((int *)args[index]) = len;
}
