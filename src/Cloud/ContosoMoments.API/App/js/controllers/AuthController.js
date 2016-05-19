'use strict';

contosoMomentsApp
    .controller('authController', ['$scope', '$state', 'appConfig', 'mobileServicesClient',
        function ($scope, $state, appConfig, mobileServicesClient) {

            this.serviceLogin = function (provider) {
                mobileServicesClient.login(provider)
                    .done(function (result) {

                        mobileServicesClient.invokeApi("ManageUser", {
                            method: "get"
                        }).done(function (results) {

                            appConfig.userId = results.result;

                        }, function (error) {

                            appConfig.userId = appConfig.DefaultUserId;

                        });

                        $state.go('main.albums');
                    }
                    , function (error) {
                        console.log('login error in authController.')
                    });
            }

        }]);