#!/bin/bash  
image=$1
version=$2


docker login -u sfconnect2017 -p iNbWQw==/81yJNOqqtgoSR0qIPeMdXGT sfconnect2017.azurecr.io
docker tag $image:latest sfconnect2017.azurecr.io/$image:$version
docker push sfconnect2017.azurecr.io/$image:$version
