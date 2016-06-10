'use strict';

contosoMomentsApp
    .factory('mobileServicesClient', ['appConfig', function (appConfig) {

        var serviceClient = new WindowsAzure.MobileServiceClient(appConfig.DefaultServiceUrl || '/', appConfig.MobileClientAppKey || '');
        return serviceClient;

    }]);