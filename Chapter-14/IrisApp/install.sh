#!/bin/bash

sfctl application upload --path IrisApp --show-progress
sfctl application provision --application-type-build-path IrisApp
sfctl application create --app-name fabric:/IrisApp --app-type IrisAppType --app-version 1.0.0
