'use strict';

contosoMomentsApp
    .controller('uploadController', ['$scope', 'uploadService', '$uibModalInstance', '$timeout', 'selectedAlbum', 'appConfig',
        function ($scope, uploadService, $uibModalInstance, $timeout, selectedAlbum, appConfig) {
        var init = function () {
            $scope.progress = -1;
            $scope.uploading = false;
            $scope.hasError = false;
            $scope.progressType = "info";
        }

        var uploadOptions = {
            complete: function () {
                $scope.uploading = false;
                $timeout(function () {
                    $uibModalInstance.close($scope.selectedFile);
                }, 1000);

            },
            progress: function (progress) {
                $scope.progress = parseFloat(progress);
            },
            error: function () {
                $scope.uploading = false;
                $scope.hasError = true;
                $scope.progressType = "error";
            },
            userId: appConfig.userId,
            albumId: selectedAlbum.album.id
        }
        this.showProgress = function () {
            return ($scope.progress >= 0);
        }
        this.onFileChange = function (files) {
            $scope.selectedFile = files[0];
        }
        this.upload = function () {
            if (!angular.isUndefined($scope.selectedFile)) {
                $scope.progress = 0;
                uploadService($scope.selectedFile, uploadOptions);
                $scope.uploading = true;
            }
        }
        this.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        }
        this.reset = function () {
            init();
        }
        $scope.$on('modal.closing', function (event) {
            if ($scope.uploading) {
                event.preventDefault();
            }
        });

    }]);