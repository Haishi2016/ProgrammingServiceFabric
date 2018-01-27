#!/bin/bash

sfctl application delete --application-id IrisApp
sfctl application unprovision --application-type-name IrisAppType --application-type-version 1.0.0
sfctl store delete --content-path IrisApp
