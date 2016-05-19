'use strict';

contosoMomentsApp
    .controller('imageController', ['currentImage', 'imageService', '$q', '$scope',
    function (currentImage, imageService, $q, $scope) {
        var self = this;
        $q.when(currentImage).then(function (curImage) {
            if (angular.isArray(curImage)) {
                self.currentImage = curImage[0];
                self.currentAlbum = curImage[0].album
            }
            else {
                self.currentImage = curImage.image;
                self.currentAlbum = curImage.album;
            }
        });

        self.getCurrentImageURL = function (size) {
            return imageService.getImageURL(this.currentImage.id, size).then(function (data) { return data; });
        }

        self.hasBeenLiked = false;
        self.like = function () {
            if (!self.hasBeenLiked) {
                imageService.likeImage(self.currentImage.id).then(function (res) {
                    self.hasBeenLiked = res;
                });
            }
        }


        $scope.$on('imageDeleted', function (e, imageId) {
            for (var i = 0; i < self.currentAlbum.images.length; i++) {
                if (self.currentAlbum.images[i].id == imageId) {
                    self.currentAlbum.images.splice(i, 1);
                    break;
                }
            }
        });


    }]);

contosoMomentsApp
    .controller('deleteimageController', ['$scope', 'imageService', '$uibModalInstance', '$state', '$rootScope', '$location',
    function ($scope, imageService, $uibModalInstance, $state, $rootScope, $location) {
        var self = this;
        self.currantImageId = $state.params["imageid"];

        self.delete = function () {

            imageService.deleteImage(self.currantImageId).then(function (res) {
                $uibModalInstance.close(res);

                $location.path('/album/' + $rootScope.lastAlbum.id);
                //$state.go('main.gallery', { albumid: $rootScope.lastAlbum.id })
            }).finally(function () {
                //;
            });

            ;
        }

        self.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        }

        $scope.$on('modal.closing', function (event) {
            if ($scope.uploading) {
                event.preventDefault();
            }
        });

    }]);
