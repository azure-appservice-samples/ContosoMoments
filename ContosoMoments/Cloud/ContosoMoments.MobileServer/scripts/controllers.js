(function (angular) {
    'use strict';

    var app = angular.module('app');
    app.controller('albumController', ['$scope', 'albumsService', 'imageService', 'appConfig', 'selectedImage', '$state', function ($scope, albumsService, imageService, appConfig, selectedImage, $state) {
        var self = this;
        this.currentIndex = 0;
        this.count = 24;
        this.curAlbum;

        var onImageGotten = function (images) {
            if (self.currentIndex === 0 && self.curAlbum.images.length === 0) {
                self.curAlbum.images = images;
                if (self.curAlbum.images.length === self.count) {
                    self.curAlbum.images[self.curAlbum.images.length - 1].showPlaceholder = true;
                }
            } else if (self.currentIndex > 0 && self.curAlbum.images.length < (self.currentIndex + self.count)) {
                self.curAlbum.images[self.curAlbum.images.length - 1].showPlaceholder = images.length <= 0;
                self.curAlbum.images = self.curAlbum.images.concat(images);
                if (self.curAlbum.images.length === self.currentIndex + self.count) {
                    self.curAlbum.images[self.curAlbum.images.length - 1].showPlaceholder = true;
                }
            }
        }
        this.getAlbumImages = function () {
            albumsService.getAlbum(appConfig.DefaultAlbumId, { start: self.currentIndex, count: self.count }).then(function (album) {
                self.curAlbum = album;
                return imageService.getImagesFromAlbum(album, { start: self.currentIndex, count: self.count });
            }).then(onImageGotten);
        }
        this.showNext = function () {
            this.currentIndex += this.count;
            imageService.getImagesFromAlbum(self.curAlbum, { start: self.currentIndex, count: self.count }).then(onImageGotten)
        }
        this.getImageURL = function (img, size) {
            return imageService.getImageURL(img, size);
        }
        this.onImageSelected = function (image) {
            selectedImage.image = image;
            selectedImage.album = self.curAlbum;
            $state.go('main.singleImage', { imageid: image.id });
        }

        this.getAlbumImages();


    }]);

    app.controller('imageController', ['currentImage', 'imageService', 'authContext','$q',function (currentImage, imageService, authContext,$q) {
        var self = this;
        $q.when(currentImage).then(function (curImage) {
            if (angular.isArray(curImage)){
                self.currentImage = curImage[0];
                self.currentAlbum = curImage[0].album
            }
            else{
                self.currentImage = curImage.image;
                self.currentAlbum = curImage.album;
            }
               
            self.currentAlbum.owner = authContext.currentUser.email;
        });
      
        this.getCurrentImageURL = function (size) {
            return imageService.getImageURL(this.currentImage, size);
        }
    }]);
    app.controller('navController', ['$scope', '$uibModal', '$location',function ($scope, $uibModal, $location, currentUser) {

        $scope.showUpload = true;

        $scope.$on('$stateChangeSuccess', function (e, toState) {
            $scope.showUpload = toState.name === 'main.gallery';
        })



        this.openUploadModal = function () {

            var modalInstance = $uibModal.open({
                animation: true,
                templateUrl: 'uploadModal.html',
                controller: 'uploadController',
                controllerAs: 'uploadCtrl'
            });

            modalInstance.result.then(function (uplodedFile) {
                console.log('upload modal completed at: ' + new Date());
            }, function () {
                console.log('Modal dismissed at: ' + new Date());
            });

        }




    }]);
    app.controller('uploadController', ['$scope', 'uploadService', '$uibModalInstance', '$timeout', function ($scope, uploadService, $uibModalInstance, $timeout) {
        $scope.progress = -1;
        $scope.uploading = false;
        var uploadOptions = {
            complete: function () {
                $scope.uploading = false;
                $timeout(function () {
                    $uibModalInstance.close($scope.selectedFile);
                }, 1500);

            },
            progress: function (progress) {
                $scope.progress = parseFloat(progress);
            },
            error: function () {
                $scope.uploading = false;
            }
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
        $scope.$on('modal.closing', function (event) {
            if ($scope.uploading) {
                event.preventDefault();
            }
        });

    }]);
})(angular)