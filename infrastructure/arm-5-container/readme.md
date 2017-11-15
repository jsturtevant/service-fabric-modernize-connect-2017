## Set up the cluster
1. Update the `deploy.ps1` and `parameters.json` file with your settings
2. Create a password using `$pw = ConvertTo-SecureString "yourpassword" -AsPlainText -Force`
3. Create the cluster with `.\deploy.ps1 -adminPassword $pw`. 

## Set up monitoring
Follow the steps below to add to an existing cluster.  Alternatively you could modify the arm template to deploy the features below.

1. Add the OMS solution
2. hook up the storage accounts
3. Run `az vmss extension set --name MicrosoftMonitoringAgent --publisher Microsoft.EnterpriseCloud.Monitoring --resource-group <nameOfResourceGroup> --vmss-name <nameOfNodeType> --settings "{'workspaceId':'<OMSworkspaceId>'}" --protected-settings "{'workspaceKey':'<OMSworkspaceKey>'}"`
4. Turn on performance metrics in oms portal