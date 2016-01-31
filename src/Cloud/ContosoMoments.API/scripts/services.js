(function (angular) {
    'use strict';

    var app = angular.module('app');
    app.constant('appConfig', (configJson || {}));
    var AuthContext = function (userid, isAuthenticated, currentUser) {
        this.userId = userid;
        this.currentUser = currentUser;
        this.isAuthenticated = isAuthenticated;
    };

    app.factory('authService', ['userService', 'appConfig', '$q', '$rootScope', '$http', function (userService, appConfig, $q, $rootScope, $http) {
        var context;

        return {
            getAuthContext: function () {
                var defered = $q.defer();
                if (context) {
                    defered.resolve(context);
                }
                else {


                    $http.get("/.auth/me")
                        .then(function (returnData) {
                            //var test = [{ "access_token": "CAAQnF6QGbqMBAIQK9ZClLwE0nB1rdr0BGclLqFwZBQTkY8FnedvKVILTYy6DOwOogH3wWp88DkSuscsp5ZBQl5pPIO9fXYFn9hot7b7F4QkuLrUe4Uz5566QLrG5Uy4ZCZArQX5Km7Lb1BrEdcYB37jgdfAZBT56NTy6TWGBv1oNGixR8caBdlKZAriRenctYXYdeZCjDJOJBQZDZD", "expires_on": "2016-03-28T18:53:36.7621289Z", "provider_name": "facebook", "user_claims": [{ "typ": "http:\/\/schemas.xmlsoap.org\/ws\/2005\/05\/identity\/claims\/nameidentifier", "val": "10153792943454216" }, { "typ": "http:\/\/schemas.xmlsoap.org\/ws\/2005\/05\/identity\/claims\/name", "val": "Rotem Or" }], "user_id": "10153792943454216" }];

                            var authData = returnData.data[0];

                            if (authData == null) {
                                $http.get("/api/ManageUser")
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
                            }


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



                        });


                    //userService.getUser(appConfig.DefaultUserId).then(function (res) {
                    //    context = new AuthContext(appConfig.DefaultUserId, true, res);
                    //    defered.resolve(context);
                    //    $rootScope.$broadcast('userAuthenticated', context);
                    //}, function (err) {
                    //    defered.reject(context);
                    //});
                }


                return defered.promise;
            },
            currentContext: function () {
                return context;
            }
        }
    }]);

    app.factory('mobileServicesClient', ['appConfig', function (appConfig) {

        var serviceClient = new WindowsAzure.MobileServiceClient(appConfig.DefaultServiceURL || '/', appConfig.MobileClientAppKey || '');
        return serviceClient;


    }]);

    app.factory('albumsService', ['$http', '$q', '$cacheFactory', 'mobileServicesClient', '$rootScope', function ($http, $q, $cacheFactory, mobileServicesClient, $rootScope) {
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

                albumTable.where(function (currentUserId) {
                    return this.userId == currentUserId || this.id == "11111111-1111-1111-1111-111111111111";
                }, userId)

                .orderByDescending("createdAt")
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

    app.factory('imageService', ['mobileServicesClient', '$interpolate', '$http', '$q', 'appConfig', '$rootScope', function (mobileServicesClient, $interpolate, $http, $q, appConfig, $rootScope) {
        var urlExp = $interpolate('{{image.containerName}}/{{size}}/{{image.fileName}}');
        var imageDefaultOptions = {
            start: 0,
            count: 50
        };
        var getImageOptions = function (options) {
            return angular.extend(imageDefaultOptions, options);
        };

        var getImageFromAlbum = function (album, id) {
            if (album && album.images) {
                var images = album.images.filter(function (item) {
                    return item.id === id;
                });

                if (images.length > 0) {
                    return images[0];
                }
                else {
                    return null;
                }
            }
            return null;
        };

        var setLikeForImage = function (id) {

            return $http.post("/api/like", { imageId: id });
        }

        return {
            getImageURL: function (img, imgSize) {
                return urlExp({ image: img, size: imgSize });
            },
            getImagesFromAlbum: function (album, options) {
                var defered = $q.defer();
                if (album) {
                    var reqOptions = getImageOptions(options);
                    var imageTable = mobileServicesClient.getTable('image');
                    imageTable.where({
                        'Album/Id': album.id
                    })
                    .orderByDescending("createdAt")
                    .skip(reqOptions.start)
                    .take(reqOptions.count)
                    .read()
                    .done(function (results) {
                        defered.resolve(results);
                    }, function (error) {
                        console.log(error);
                        defered.reject(error);
                    });
                }
                else {
                    defered.reject('album is undefined');
                }
                return defered.promise;
            },
            getImageById: function (id) {
                var defered = $q.defer();
                var imageTable = mobileServicesClient.getTable('image');
                imageTable.read('$expand=Album&$filter=id eq \'' + id + '\'')
                    .done(function (results) {
                        defered.resolve(results);
                    }, function (error) {
                        console.log(error);
                        defered.reject(error);
                    });
                return defered.promise;
            },
            deleteImage: function (id) {
                var defered = $q.defer();
                $http.delete('/api/image/' + id, []).success(function (data, status) {
                    defered.resolve(data);
                    $rootScope.$broadcast('imageDeleted', id);
                    console.log("***************  " + data);
                });
                return defered.promise;
            },

            likeImage: setLikeForImage

        }
    }]);

    app.factory('uploadService', ['azureBlob', '$http', 'appConfig', '$rootScope', function (azureBlob, $http, appConfig, $rootScope) {
        var getSasUrl = function () {
            return $http.get("/api/GetSasUrl").then(function (res) {
                return res.data;
            });
        }
        var commit = function (sasurl, options) {
            return $http.post('/api/CommitBlob', {
                isMobile: false,
                UserId: options.userId,
                AlbumId: options.albumId,
                SasUrl: sasurl,
                //sendNotification: store.sendNotification(blobface.selectedFile.name)
            }).then(function (res) {
                return res.data;
            });
        }
        return function upload(currentFile, options) {
            var config = options || {};

            getSasUrl().then(function (res) {
                var sasurl = res;
                var urlParts = res.split('?');
                azureBlob.upload({
                    baseUrl: urlParts[0],// baseUrl for blob file uri (i.e. http://<accountName>.blob.core.windows.net/<container>/<blobname>),
                    sasToken: '?' + urlParts[1], // Shared access signature querystring key/value prefixed with ?,
                    file: currentFile, // File object using the HTML5 File API,
                    progress: config.progress || angular.noop, // progress callback function,
                    complete: function () {
                        commit(sasurl, config).then(function (res) {
                            if (angular.isFunction(config.complete)) {
                                config.complete(res);
                            }
                            if (res.success) {
                                $rootScope.$broadcast('imageUploaded', res.imageId);
                            }


                        })

                    },// complete callback function,
                    error: config.error || angular.noop// error callback function,                       
                });
                //setFile($("#file")[0].files[0], res); 
            }, function (err) {
                if (typeof (config.error) === 'function') {
                    config.error();
                }
            });
        };
    }]);

    app.factory('userService', ['mobileServicesClient', '$q', function (mobileServicesClient, $q) {
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


    app.value('selectedImage', { image: null, album: null });

    app.value('selectedAlbum', { album: null });



})(angular)
