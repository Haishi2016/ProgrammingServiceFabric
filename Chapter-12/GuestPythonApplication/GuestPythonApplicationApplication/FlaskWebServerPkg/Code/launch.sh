#!/bin/bash

sudo python -m pip install flask >> ../log/flask-install.txt 2>&1
pushd $(dirname "${0}") > /dev/null
BASEDIR=$(pwd -L)
popd > /dev/null
logger ${BASEDIR}
python ${BASEDIR}/flaskserver.py