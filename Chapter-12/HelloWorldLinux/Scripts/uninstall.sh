#!/bin/bash -e

if [[ "$#" != "0" ]];then 
	version="$1"
else 
	version="1.0.0"
fi

appplicationCount=`sfctl application list | grep fabric:/HelloWorldLinuxApplication | wc -l`
if [[ "$appplicationCount" -eq "0" ]];then
	echo "Nothing to uninstall"
	exit 0
fi	

sfctl application delete --application-id HelloWorldLinuxApplication
if [ $? -ne 0 ]; then
    echo "Application removal failed."
    exit 1
fi

sfctl application unprovision --application-type-name HelloWorldLinuxApplicationType --application-type-version $version
if [ $? -ne 0 ]; then
    echo "Unregistering application type failed."
    exit 1
fi

sfctl store delete --content-path HelloWorldLinuxApplication
if [ $? -ne 0 ]; then
    echo "Unable to delete image store content."
    exit 1
fi
echo "Uninstall script executed successfully."
