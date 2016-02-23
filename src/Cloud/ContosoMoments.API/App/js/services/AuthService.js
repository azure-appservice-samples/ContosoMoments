'use strict';

var AuthContext = function (userid, isAuthenticated, currentUser) {
    this.userId = userid;
    this.currentUser = currentUser;
    this.isAuthenticated = isAuthenticated;
};

contosoMomentsApp
    .factory('authService', ['userService', 'appConfig', '$q', '$rootScope', '$http',
    function (userService, appConfig, $q, $rootScope, $http) {

        var context;

        return {

            getAuthContext: function () {
                var defered = $q.defer();
                if (context) {
                    defered.resolve(context);
                }
                else {

                    $http({
                        method: 'GET',
                        url: '/.auth/me'
                    }).then(function successCallback(returnData) {

                        var authData = returnData.data[0];
                        var sendData = "";
                        if (authData.provider_name == "aad") {
                            sendData = authData.user_id;
                        }
                        if (authData.provider_name == "facebook") {
                            sendData = authData.access_token
                        }

                        $http.get("/api/ManageUser/?data=" + sendData + "&provider=" + authData.provider_name)
                                    .then(function (getRes) {

                                        userService.getUser(getRes.data).then(function (res) {
                                            context = new AuthContext(getRes.data, true, res);
                                            defered.resolve(context);
                                            $rootScope.$broadcast('userAuthenticated', context);
                                        }, function (err) {
                                            defered.reject(context);
                                        });

                                        return;
                                    });

                    }, function errorCallback(response) {
                        $http.get("/api/ManageUser")
                                    .then(function (getRes) {

                                        userService.getUser(getRes.data).then(function (res) {
                                            context = new AuthContext(getRes.data, true, res);
                                            defered.resolve(context);
                                            $rootScope.$broadcast('userAuthenticated', context);
                                            $rootScope.$broadcast('$noneAuthenticatedUser', context);

                                        }, function (err) {
                                            defered.reject(context);
                                        });

                                        return;
                                    });
                    });
                }
                return defered.promise;
            },
            currentContext: function () {
                return context;
            }
        }
    }]);