#!/usr/bin/env bash

# in the case a machine has already been provisioned, running this script shouldn't be a problem.

# quick and dirty test to see if update has been run yet.
if [ ! -d /opt ]
  then
    apt-get update
fi

apt-get install -y mono-devel
apt-get install -y mono-xbuild
apt-get install -y g++
apt-get install -y make
apt-get install -y libtool
apt-get install -y autoconf
apt-get install -y automake
apt-get install -y uuid-dev
apt-get install -y git
apt-get install -y unzip
apt-get install -y libcurl-openssl-dev
apt-get install -y libcurl4-openssl-dev
apt-get install -y wget
apt-get install -y screen
apt-get install -y libc6-dev-i386

if [ ! -d /opt ]
  then
    mkdir /opt
fi

# install redis
if [ ! -f /usr/local/bin/redis-server ]
  then
    cd /opt
    wget http://redis.googlecode.com/files/redis-2.6.14.tar.gz
    gunzip redis-2.6.14.tar.gz
    tar xvf redis-2.6.14.tar
    cd redis-2.6.14
    make
    cd src
    cp redis-cli /usr/local/bin
    cp redis-server /usr/local/bin
fi


# acquire and install hiredis (same old version as I'm using for windows)
if [ ! -f /usr/local/lib/libnanomsg.a ]
  then
    cd /opt
    
    git clone https://github.com/mhowlett/hiredis.git
    cd hiredis
	make
	make install
	ldconfig
fi
