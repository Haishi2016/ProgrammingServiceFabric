#!/bin/bash

sfctl application delete --application-id Minecraft
sfctl application unprovision --application-type-name MinecraftType --application-type-version 1.0.0
sfctl store delete --content-path Minecraft
