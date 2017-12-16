#!/bin/bash

sudo docker plugin install --alias azure --grant-all-permissions docker4x/cloudstor:17.09.0-ce-azure1  \
    CLOUD_PLATFORM=AZURE \
    AZURE_STORAGE_ACCOUNT="minecraftdata" \
    AZURE_STORAGE_ACCOUNT_KEY="fVsImqLaQxOBwnfuMoV6DToz+tNLdcJ0jksmkv5Lc2wcCppaXBe24kZY/akpAPgd65zPvhA8Jey1SV6qiMY8bA==" \
    DEBUG=1
