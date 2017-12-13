#!/bin/bash -e

appplicationCount=`sfctl application list | grep ActorApplicationApplication | wc -l`
applicationTypeVersion="1.0.0"
if [[ "$#" != "0" && "$1" != "$applicationTypeVersion" ]];then 	
	applicationTypeVersion="$1"
fi
echo "Deploying with ApplicationTypeVersion $applicationTypeVersion"
if [[ "$appplicationCount" -eq "0" ]];then
    /bin/bash Scripts/install.sh $applicationTypeVersion
    if [[ "$?" -eq "0" ]]; then
    	echo "Successfully deployed application."
	else
		echo "Error occurred while deploying application."
		exit 1 
	fi 
else    
    echo "Application already exists. CleanUp: Undeploying existing app"
    /bin/bash Scripts/uninstall.sh $applicationTypeVersion
    if [[ "$?" -eq "0" ]]; then
    	echo "Successfully uninstalled application. Now re-deploying."
	else 
		echo "Error occurred while uninstalling application."
		exit 1
	fi
    /bin/bash Scripts/install.sh $applicationTypeVersion
    if [[ "$?" -eq "0" ]]; then
    	echo "Successfully deployed application."
	else
		echo "Error occurred while deploying application."
		exit 1 
	fi     
fi