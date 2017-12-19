#!/bin/bash

sfctl application delete --application-id WatchDogApp
sfctl application unprovision --application-type-name WatchDogAppType --application-type-version 1.0.0
sfctl store delete --content-path WatchDogApp
