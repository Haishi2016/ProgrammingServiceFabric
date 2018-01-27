#!/bin/bash

sfctl application upload --path WatchDogApp --show-progress
sfctl application provision --application-type-build-path WatchDogApp 
sfctl application create --app-name fabric:/WatchDogApp --app-type WatchDogAppType --app-version 1.0.0
