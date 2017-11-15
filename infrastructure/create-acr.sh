#!/bin/bash  
az group create --name sfconnect2017 -l westus2
az acr create --name sfconnect2017 --resource-group sfconnect2017 --sku Basic --admin-enabled
