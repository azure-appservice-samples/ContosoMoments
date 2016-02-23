'use strict';

contosoMomentsApp
    .factory('userService', ['mobileServicesClient', '$q', function (mobileServicesClient, $q) {

        var usersTable = mobileServicesClient.getTable('user');

        return {
            getUser: function (id) {
                var defered = $q.defer();
                usersTable.lookup(id).done(function (res) {
                    defered.resolve(res);
                }, function (error) {
                    defered.reject(error);
                });
                return defered.promise;
            }
        }
    }]);