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

// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the NHIREDIS_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// NHIREDIS_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#if defined(_MSC_VER)

#ifdef HIREDISX_EXPORTS
#define HIREDISX_API __declspec(dllexport)
#else
#define HIREDISX_API __declspec(dllimport)
#endif

#else

#define HIREDIS_API

#endif


HIREDISX_API
void *n_redisConnectWithTimeout(
		const char *ip, 
		int port, 
		int timeout_seconds, 
		int timeout_microseconds
);

HIREDISX_API
void n_redisCommand(
		void *context,
		void *args,
		int argsc,
		int *type,
		long long *integer,
		char *strBuf,
		int strBufLen,
		int *len,
		int *elements,
		void **reply
);

HIREDISX_API
void n_retrieveElement(
	void *reply, 
	int index, 
	int *type, 
	long long *integer, 
	char *strBuf, 
	int strBufLen,
	int *len,
	char **strPtr
);

HIREDISX_API
void n_freeReplyObject(
	void *reply
);

HIREDISX_API
void n_retrieveStringAndFreeReplyObject(
	void *reply, 
	char *toStrPtr
);

HIREDISX_API
void n_retrieveElementString(
	void *reply,
	int index,
	char *toStrPtr
);

HIREDISX_API
void n_setupArgumentArray(
	int length,
	char **arguments
);

HIREDISX_API
void n_setStringArgument(
   char *arguments,
   int index,
   char *argument,
   int len
);
