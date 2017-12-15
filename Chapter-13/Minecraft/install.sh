#!/bin/bash

sfctl application upload --path Minecraft --show-progress
sfctl application provision --application-type-build-path Minecraft
sfctl application create --app-name fabric:/Minecraft --app-type MinecraftType --app-version 1.0.0
