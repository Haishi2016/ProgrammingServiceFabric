#!/bin/bash

sfctl application delete --application-id GoApp
sfctl application unprovision --application-type-name GoAppType --application-type-version 1.0.0
sfctl store delete --content-path GoApp
