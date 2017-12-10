#!/bin/bash -e

protocol='http'
clientKey=0
clientCert=0
certArgs=''
ip='localhost'
port='19080'


while (( "$#" )); do
    if [ "$1" == "-ip" ]; then
        shift
        ip=$1
    elif [ "$1" == "-port" ]; then
        shift
        port=$1    
    elif [ "$1" == "-clientKey" ]; then
        shift
        clientKey=$1
    elif [ "$1" == "-clientCert" ]; then
        shift
        clientCert=$1
    fi
    shift
done

echo "parameters: ip=$ip, port=$port, clientKey=$clientKey, clientCert=$clientCert"

if [ "${ip}" = "" ] || [ "${port}" = "" ]; then
    echo "IP and Port has to be non-empty."
    exit 1
fi

if [ "$clientKey" != "0" ] && [ "$clientKey" != "" ] && [ "$clientCert" != "0" ] && [ "$clientCert" != "" ]; then
    protocol='https'
    certArgs="--key $clientKey --cert $clientCert --no-verify"
fi

url=${protocol}://${ip}:${port}

#In case of local cluster, check if the cluster is up, if not, then try to bring it up first.
if [[ $url == *"localhost"* ]]; then
    echo "For local cluster, check if the cluster is already up or not and accordingly set up the cluster."
    if [ `systemctl | grep -i fabric | wc -l` == 1 ]; then
        echo "Local cluster is up, now will try to connect to the cluster."
    else 
        echo "cluster is not up, set up the cluster first."
        if [[ $EUID -ne 0 ]]; then
            echo "Cluster-setup script must be run as root, please open your IDE as root to set-up the local cluster." 1>&2
            exit 1
        fi
        sudo /opt/microsoft/sdk/servicefabric/common/clustersetup/devclustersetup.sh
        if [ $? -ne 0 ]; then
            echo "Dev cluster set-up failed."
            exit 1
        fi
        echo -n "Setting up cluster."
        n=`ps -eaf | grep -i "filestoreservice" | grep -v grep | wc -l`
        until [ $n -eq 3 ]; do
            echo -n "."
            n=`ps -eaf | grep -i "filestoreservice" | grep -v grep | wc -l`
            sleep 30
        done
    fi
fi

echo "Connecting to $url"
sfctl cluster select --endpoint $url $certArgs
if [ $? != 0 ]; then
    echo "Something went wrong while connecting to cluster."
    exit 1
fi
echo "Connected to:$url"
