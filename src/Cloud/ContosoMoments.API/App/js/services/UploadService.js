'use strict';

contosoMomentsApp
    .factory('uploadService', ['azureBlob', '$http', 'appConfig', '$rootScope',
    function (azureBlob, $http, appConfig, $rootScope) {
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