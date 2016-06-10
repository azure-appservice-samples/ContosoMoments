'use strict';

contosoMomentsApp
    .factory('albumsService', ['$http', '$q', '$cacheFactory', 'mobileServicesClient', '$rootScope',
    function ($http, $q, $cacheFactory, mobileServicesClient, $rootScope) {
        var albumCache = $cacheFactory('albums');
        var albumService = {
            getAlbum: function (id) {
                var defered = $q.defer();
                var currentAlbum = albumCache.get(id);
                if (angular.isUndefined(currentAlbum)) {
                    var albumTable = mobileServicesClient.getTable('album');
                    albumTable.lookup(id).done(function (result) {
                        currentAlbum = result;
                        currentAlbum.images = [];
                        albumCache.put(id, currentAlbum);
                        defered.resolve(currentAlbum);
                    }, function (error) {
                        console.log(error);
                        defered.reject(error);
                    });
                } else {
                    defered.resolve(currentAlbum);
                }
                return defered.promise;
            },
            getUserAlbums: function (userId) {
                var defered = $q.defer();

                var albumTable = mobileServicesClient.getTable('album');

                albumTable.orderByDescending("createdAt")
                .read()
                .done(function (results) {
                    albumTable.where
                    defered.resolve(results);
                }, function (error) {
                    console.log(error);
                    defered.reject(error);
                });
                return defered.promise;
            },
            createAlbum: function (name, userId) {
                var defered = $q.defer();
                var albumTable = mobileServicesClient.getTable('album');
                albumTable.insert({
                    albumName: name,
                    isDefault: false,
                    UserId: userId
                }).done(function (res) {
                    $rootScope.$broadcast('albumCreated', res);
                    defered.resolve(res);
                }, function (err) {
                    defered.reject(err);
                });

                return defered.promise;
            },
            updateAlbum: function (albumId, name) {
                var defered = $q.defer();
                var albumTable = mobileServicesClient.getTable('album');
                albumTable.update({
                    id: albumId,
                    albumName: name
                }).done(function (res) {
                    defered.resolve(res);
                }, function (err) {
                    defered.reject(err);
                });

                return defered.promise;
            },
            deleteAlbum: function (albumId) {
                var defered = $q.defer();
                var albumTable = mobileServicesClient.getTable('album');
                albumTable.del({
                    id: albumId
                }).done(function (res) {
                    defered.resolve(res);
                }, function (err) {
                    defered.reject(err);
                });

                return defered.promise;
            }
        }
        return albumService;
    }]);