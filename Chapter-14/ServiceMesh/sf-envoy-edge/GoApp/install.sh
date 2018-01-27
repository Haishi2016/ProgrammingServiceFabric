#!/bin/bash

sfctl application upload --path GoApp --show-progress
sfctl application provision --application-type-build-path GoApp
sfctl application create --app-name fabric:/GoApp --app-type GoAppType --app-version 1.0.0
