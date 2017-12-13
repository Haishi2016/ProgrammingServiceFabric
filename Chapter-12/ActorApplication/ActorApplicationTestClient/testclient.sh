#!/bin/bash -e
BASEDIR=$(dirname $0)
cd $BASEDIR
export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:$(pwd)/out/lib/lib:/opt/microsoft/servicefabric/bin/Fabric/Fabric.Code/
java -Djava.library.path=$LD_LIBRARY_PATH -jar out/lib/ActorApplication-test.jar
