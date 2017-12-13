#!/bin/bash
BASEDIR=$(dirname $0)
cd $BASEDIR
export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:$(pwd)/lib
echo $LD_LIBRARY_PATH
java -Djava.library.path=$LD_LIBRARY_PATH -jar MyStatefulService.jar
