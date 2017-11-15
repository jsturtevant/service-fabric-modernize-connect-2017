1. deploy the 5 node cluster using the powershell script and instructions in the [readme.md](arm-5-container/readme.md)
2. create ACR using the script `create-acr.sh`
3. run `az acr credential show -n sfconnect2017 -g sfconnect2017` to get the password and username for the admin user  (note: in production you would use AD credentials or service principal)