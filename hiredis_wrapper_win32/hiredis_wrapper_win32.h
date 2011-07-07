// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the NHIREDIS_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// NHIREDIS_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef HIREDIS_WRAPPER_WIN32_EXPORTS
#define HIREDIS_WRAPPER_WIN32_API __declspec(dllexport)
#else
#define HIREDIS_WRAPPER_WIN32_API __declspec(dllimport)
#endif


HIREDIS_WRAPPER_WIN32_API
void *n_redisConnectWithTimeout(
		const char *ip, 
		int port, 
		int timeout_seconds, 
		int timeout_microseconds
);

HIREDIS_WRAPPER_WIN32_API
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

HIREDIS_WRAPPER_WIN32_API
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

HIREDIS_WRAPPER_WIN32_API
void n_freeReplyObject(
	void *reply
);

HIREDIS_WRAPPER_WIN32_API
void n_retrieveStringAndFreeReplyObject(
	void *reply, 
	char *toStrPtr
);

HIREDIS_WRAPPER_WIN32_API
void n_retrieveElementString(
	void *reply,
	int index,
	char *toStrPtr
);

HIREDIS_WRAPPER_WIN32_API
void n_setupArgumentArray(
	int length,
	char **arguments
);

HIREDIS_WRAPPER_WIN32_API
void n_setArgument(
   char *arguments,
   int index,
   char *argument,
   int len
);
