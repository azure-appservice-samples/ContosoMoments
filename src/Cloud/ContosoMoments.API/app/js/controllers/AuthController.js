'use strict';

contosoMomentsApp
    .controller('authController', ['$scope', 'mobileServicesClient',
    function($scope, mobileServicesClient){

        this.serviceLogin = function(provider){
            mobileServicesClient.login(provider);
        }
        
    }]);