'use strict';

contosoMomentsApp
    .factory('uploadService', ['azureBlob', '$http', 'appConfig', '$rootScope',
    function (azureBlob, $http, appConfig, $rootScope) {
        var getSasUrl = function () {

            var newImageId = uuid.v4();
            return $http.post('/tables/Image/' + newImageId + '/StorageToken', {
                "Permissions": "Write",
                "TargetFile": {
                    "Id": newImageId,
                    "Name": newImageId,
                    "TableName": "Image",
                    "ParentId": newImageId,
                    "ContentMD5": null,
                    "LastModified": null,
                    "StoreUri": "/" + appConfig.UploadContainerName + "-lg" + "/" + newImageId,
                    "Metadata": {}
                },
                "ScopedEntityId": null,
                "ProviderName": null
            })
            .then(function (res) {
                return res.data;
            });
        }
        var commit = function (sasurl, options) {
            // TODO: Change to Mobile Service Table Object.
            return $http.post('/tables/Image', {
                "Id": sasurl.EntityId,
                "UploadFormat": "Web Upload",
                "AlbumId": options.albumId,
                "UserId": options.userId,
            }).then(function (res) {
                return res.data;
            });
        }
        return function upload(currentFile, options) {
            var config = options || {};

            getSasUrl().then(function (sas) {
                var sasurl = sas;
                azureBlob.upload({
                    baseUrl: sas.ResourceUri + "/" + sas.EntityId,// baseUrl for blob file uri (i.e. http://<accountName>.blob.core.windows.net/<container>/<blobname>),
                    sasToken: sas.RawToken, // Shared access signature querystring key/value prefixed with ?,
                    file: currentFile, // File object using the HTML5 File API,
                    progress: config.progress || angular.noop, // progress callback function,
                    complete: function () {
                        commit(sasurl, config).then(function (res) {
                            if (angular.isFunction(config.complete)) {
                                config.complete(res);
                                $rootScope.$broadcast('imageUploaded', res.id);
                            }
                        })
                    },// complete callback function,
                    error: config.error || angular.noop// error callback function,                       
                });
            }, function (err) {
                if (typeof (config.error) === 'function') {
                    config.error();
                }
            });
        };
    }]);