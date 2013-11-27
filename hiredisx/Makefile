# hiredisx Makefile
# Copyright (C) 2011 Matt Howlett <matt dot howlett at gmail dot com>
# This file is released under the BSD license, see the LICENSE.txt file

HIREDIS_PATH=/opt/hiredis
FLAGS=-O2

all: libhiredisx

libhiredisx: hiredisx
	gcc -shared -Wl,-soname,libhiredisx.so.1 -o libhiredisx.so hiredisx.o $(HIREDIS_PATH)/libhiredis.a

hiredisx: hiredisx.c hiredisx.h
	gcc $(FLAGS) -I$(HIREDIS_PATH) -c -fPIC hiredisx.c -o hiredisx.o

clean:
	rm -rf libhiredisx.so
	rm -rf hiredisx.o
	rm -rf *~

