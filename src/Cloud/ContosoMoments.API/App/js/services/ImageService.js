'use strict';

contosoMomentsApp
    .factory('imageService', ['mobileServicesClient', '$interpolate', '$http', '$q', 'appConfig', '$rootScope',
    function (mobileServicesClient, $interpolate, $http, $q, appConfig, $rootScope) {
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
                // The Storage account was set to public when this code was created, changing to implement SAS token retrieval
                return urlExp({ image: img, size: imgSize });
                //return $http.post('/tables/Image/'+ img.id +'/StorageToken', {});
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