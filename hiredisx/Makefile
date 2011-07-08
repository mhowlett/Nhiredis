# hiredisx Makefile
# Copyright (C) 2011 Matt Howlett <matt dot howlett at gmail dot com>
# This file is released under the BSD license, see the LICENSE.txt file

HIREDIS_PATH=../../hiredis

all: libhiredisx

libhiredisx: hiredisx
	gcc -shared -Wl,-soname,libhiredisx.so.1 -o libhiredisx.so hiredisx.o

hiredisx: hiredisx.c hiredisx.h
	gcc -I$(HIREDIS_PATH) -c -fPIC hiredisx.c -o hiredisx.o
