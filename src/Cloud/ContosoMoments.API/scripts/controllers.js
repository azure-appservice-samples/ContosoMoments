(function (angular) {
    'use strict';

    var app = angular.module('app');
    app.controller('albumsController', ['$scope','albumsService', 'authContext','$state', function ($scope,albumsService, authContext,$state) {
        var self = this;
        self.albums=[];
        albumsService.getUserAlbums(authContext.userId).then(function (albums) {
            if (albums && angular.isArray(albums)) {
                self.albums = albums;
            }
        });
        $scope.$on('albumCreated', function (e,album) {
            self.albums.push(album);
        });
        self.goToAlbum=function(album){
            $state.go('main.gallery', { albumid: album.id });
        }

    }]);
    app.controller('albumController', ['$scope', 'albumsService', 'imageService', 'appConfig', 'selectedImage', '$state','selectedAlbum',function ($scope, albumsService, imageService, appConfig, selectedImage, $state,selectedAlbum) {
        var self = this;
        this.currentIndex = 0;
        this.count = 24;
        this.curAlbum = selectedAlbum.album ;

        var onImageGotten = function (images) {
            if (self.currentIndex === 0 && self.curAlbum.images.length === 0) {
                self.curAlbum.images = images;
                if (self.curAlbum.images.length >= (self.count-1)) {
                    self.curAlbum.images[self.curAlbum.images.length - 1].showPlaceholder = true;
                }
            } else if (self.currentIndex > 0 && self.curAlbum.images.length < (self.currentIndex + self.count-1)) {
                self.curAlbum.images[self.curAlbum.images.length - 1].showPlaceholder = images.length <= 0;
                self.curAlbum.images = self.curAlbum.images.concat(images);
                if (self.curAlbum.images.length === self.currentIndex + self.count) {
                    self.curAlbum.images[self.curAlbum.images.length - 1].showPlaceholder = true;
                }
            }
        }
        this.getAlbumImages = function () {
            imageService.getImagesFromAlbum(self.curAlbum, { start: self.currentIndex, count: self.count }).then(onImageGotten);
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
        
       $scope.$on('imageUploaded', function (e, imageId) {
           imageService.getImageById(imageId).then(function (img) {
               self.curAlbum.images.unshift(img[0]);
           });
        
       });

    }]);
    app.controller('createAlbumController', ['$scope', 'albumsService', '$uibModalInstance', 'authService', 'selectedAlbum', function ($scope, albumsService, $uibModalInstance, authService, selectedAlbum) {
        var self = this;
        self.postingAlbum = false;
        self.modalTitle = "Create Album";
        var editAlbum = function () {
            self.postingAlbum = true;
            albumsService.updateAlbum(selectedAlbum.album.id, self.currentAlbum.albumName).then(function (res) {
                if (res) {
                    selectedAlbum.album = res;
                }
                $uibModalInstance.close(res);
            }).finally(function () {
                self.postingAlbum = false;
            });
        }
        var createAlbum = function () {
            var auth = authService.currentContext();
            self.postingAlbum = true;
            albumsService.createAlbum(self.currentAlbum.albumName, auth.userId).then(function (res) {
                $uibModalInstance.close(res);
            }).finally(function () {
                self.postingAlbum = false;
            });

        }
        var postFunc = createAlbum;
        if (selectedAlbum.album != null) {
            self.currentAlbum = angular.copy(selectedAlbum.album);
            self.modalTitle = "Edit Album";
            postFunc = editAlbum;
        }
        self.post = function () {
            postFunc();
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
    
    app.controller('deleteAlbumController', ['$scope', 'albumsService', '$uibModalInstance', 'selectedAlbum','$state', function ($scope, albumsService, $uibModalInstance, selectedAlbum,$state) {
        var self = this;
        $scope.albumName = selectedAlbum.album.albumName;
        self.deletingAlbum = false;
        self.deleteAlbum = function () { 
            self.deletingAlbum = true;
            albumsService.deleteAlbum(selectedAlbum.album.id).then(function (res) {
                $uibModalInstance.close(res);
            }).finally(function () {
                self.deletingAlbum = false;
            });

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
      
        self.getCurrentImageURL = function (size) {
            return imageService.getImageURL(this.currentImage, size);
        }

        self.hasBeenLiked = false;
        self.like = function () {
            if (!self.hasBeenLiked) {
                imageService.likeImage(self.currentImage.id).then(function (res) {
                    self.hasBeenLiked = res;
                });
            }
           
        }
    }]);
    app.controller('navController', ['$scope', '$uibModal', '$state',function ($scope, $uibModal, $state) {

        $scope.showUpload = false;
        $scope.showCreateAlbum = false;
        $scope.showMenu = true;

        $scope.$on('$stateChangeSuccess', function (e, toState) {
            $scope.navCollapsed = true;
            $scope.showUpload = toState.name === 'main.gallery';
            $scope.showCreateAlbum = toState.name === 'main.albums';
            $scope.showMenu = toState.name !== 'main.singleImage';
        });

        var openModal = function (options) {
            return $uibModal.open(options);
        }

        this.openUploadModal = function () {

            openModal({
                animation: true,
                templateUrl: 'uploadModal.html',
                controller: 'uploadController',
                controllerAs: 'uploadCtrl'
            }).result.then(function (uplodedFile) {
                console.log('upload modal completed at: ' + new Date());
            }, function () {
                console.log('Modal dismissed at: ' + new Date());
            });

        }
        this.openAlbummModal = function (isEdit) {
            openModal({
                animation: true,
                templateUrl: 'createAlbum.html',
                controller: 'createAlbumController',
                controllerAs: 'crtAlbumCtrl'
            }).result.then(function (createdAlbum) {}, function () {
                console.log('Modal dismissed at: ' + new Date());
            });
        }
        this.deleteAlbumModal = function () {
            openModal({
                animation: true,
                templateUrl: 'deleteAlbum.html',
                controller: 'deleteAlbumController',
                controllerAs: 'delAlbumCtrl'
            }).result.then(function (album) {
                $state.go('main.albums')
            }, function () {
                console.log('Modal dismissed at: ' + new Date());
            });
        }
      

       

    }]);
    app.controller('titleController', ['$scope', 'selectedAlbum', function ($scope, selectedAlbum) {
        $scope.$on('userAuthenticated', function (e, authContext) {
            $scope.userEmail = authContext.currentUser.email;
        });
        $scope.curAlbum = selectedAlbum;
      
    }]);
    app.controller('uploadController', ['$scope', 'uploadService', '$uibModalInstance', '$timeout', 'authService', 'selectedAlbum', function ($scope, uploadService, $uibModalInstance, $timeout, authService, selectedAlbum) {
        

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
            userId: authService.currentContext().userId,
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
})(angular)