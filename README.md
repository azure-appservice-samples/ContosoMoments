# ContosoMoments
Welcome to ContosoMoments Demo!

Blah blah blah..

Deployment script require the following paramaters:

1. ServerLocation: Azure region where application gateway will be deployed. 
    a. Default value: West US
2. dbAdminUser: The database admin username that you wish to create.
    a. Default value: dbAdmin 
3. dbAdminPassword: Password for database admin user that you wish to use. Replace for enchanced security!
    a. Default value: P@ssw0rd!
4. dbServerName: The name of the SQL database server that you wish to create. The value must be all low case
    a. Default value: contosomomentsdb
5. dbName: The name of the database that you wish to create
    a. Default value: ContosoMomentsDB
6. storageAccountName: The name of the storage account that you wish to create. The value must be all low case
    a. Default value: contosomomentsstorage
7. storageAccountType: The type of the storage account app that you wish to create
    a. Default value: Standard_GRS
8. storageContainerName: The name of storage container that you wish to use
    a. Default value: images
9. hostPlanName: The hosting service plan name that you wish to create
    a. Default value: ContosoMomentsHostingPlan
10. sku: The pricing tier for the hosting plan
    a. Default value: Free
11. webName: The name of the web client app that you wish to create
    a. Default value: ContosoMomentsWeb
12. webApiName: The name of the mobile web service app that you wish to create
    a. Default value: ContosoMomentsMobileWeb
13. webJobSiteName: The name of the web job hosting app that you wish to create
    a. Default value: ContosoMomentsWebJobs
14. ResizeQueueName: The name of 'resize' queue that you wish to use
    a. Default value: resizerequest
15. DefaultUserId: The default userId that you wish to use for unauthenticated users
    a. Default value: 11111111-1111-1111-1111-111111111111
16. DefaultAlbumId: The default albumId that you wish to use for unauthenticated users
    a. Default value: 11111111-1111-1111-1111-111111111111
17. FacebookAuthString: Use any valid URL
    a. Default value: http://www.facebook.com
18. MobileWebApiPackageURL: Location of the Mobile Web API deployment package
    a. Default value: Location of Web Deploy package on GitHub
19. WebClientPackageURL: Location of the Web Client deployment package
    a. Default value: Location of Web Deploy package on GitHub
20. WebJobsPackageURL: Location of the Web Jobs deployment package
    a. Default value: Location of Web Deploy package on GitHub


<a href="https://azuredeploy.net/?repository=https://github.com/azure-appservice-samples/ContosoMoments/" target="_blank">
    <img src="http://azuredeploy.net/deploybutton.png"/>
</a>
