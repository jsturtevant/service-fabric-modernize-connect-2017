#!/bin/zsh
clusterName=$1
appFolder=$2
version2="V2"

set -x #echo on

sfctl cluster select --endpoint http://$clusterName:19080
sfctl application upload --path $appFolder --show-progress
sfctl application provision --application-type-build-path $appFolder
sfctl application create --app-name fabric:/ContainerApplication --app-type SimpleContainerAppType --app-version 1.0.0
sfctl service create --name fabric:/ContainerApplication/nodejsFrontEnd --service-type nodejsfrontendType --stateless --instance-count 1 --app-id ContainerApplication  --singleton-scheme --constraints "NodeType == RedVM"
sfctl service create --name fabric:/ContainerApplication/pythonBackEnd --service-type pythonbackendType --stateless --instance-count 1 --app-id ContainerApplication  --dns-name pythonbackend.simplecontainerapp --singleton-scheme --constraints "NodeType == RedVM"

set +x #echo off
echo "\n\nPress any key to fix placement constraints\n\n"
read
set -x #echo on

sfctl service update --service-id ContainerApplication/nodejsFrontEnd --constraints "NodeType == BlueVM" --stateless
sfctl service update --service-id ContainerApplication/pythonBackEnd --constraints "NodeType == RedVM" --stateless

set +x #echo off
echo "\n\nPress any key to scale up\n\n"
read
set -x #echo on


sfctl service update --service-id ContainerApplication/nodejsFrontEnd  --instance-count 5 --stateless
sfctl service update --service-id ContainerApplication/pythonBackEnd --instance-count 5 --stateless

set +x #echo off
echo 
echo "\n\nPress any key to scale back down\n\n"
read
set -x #echo on


sfctl service update --service-id ContainerApplication/nodejsFrontEnd  --instance-count 3 --stateless
sfctl service update --service-id ContainerApplication/pythonBackEnd --instance-count 2  --stateless

set +x #echo off
echo "\n\nPress any key for a zero downtime rolling upgrade\n\n"
read
set -x #echo on

sfctl application upload --path $appFolder$version2 --show-progress
sfctl application provision --application-type-build-path $appFolder$version2 
sfctl application upgrade --app-id fabric:/ContainerApplication --app-version 2.0.0 --mode UnmonitoredAuto --parameters {}


set +x #echo off
echo "\n\nPress any key to clean up\n\n"
read
set -x #echo on

sfctl service delete --service-id ContainerApplication/pythonBackEnd
sfctl service delete --service-id ContainerApplication/nodejsFrontEnd
sfctl application delete --application-id ContainerApplication 

sfctl application  unprovision --application-type-name SimpleContainerAppType  --application-type-version 1.0.0
sfctl application  unprovision --application-type-name SimpleContainerAppType --application-type-version 2.0.0

sfctl store delete --content-path $appFolder
sfctl store delete --content-path $appFolder$version2 

