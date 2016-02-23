'use strict';

contosoMomentsApp
    .controller('navController', ['$scope', '$uibModal', '$state', '$location',
        function ($scope, $uibModal, $state, $location) {

            $scope.showUpload = false;
            $scope.showCreateAlbum = false;
            $scope.showMenu = true;
            $scope.showDelImage = false;
            $scope.$on('$stateChangeSuccess', function (e, toState) {
                $scope.navCollapsed = true;
                $scope.showUpload = toState.name === 'main.gallery';
                $scope.showDelImage = toState.name === 'main.singleImage';
                $scope.showCreateAlbum = toState.name === 'main.albums';
                $scope.showMenu = toState.name !== 'main.singleImage';
            });

            $scope.$on('$noneAuthenticatedUser', function (e, toState) {
                $location.path('/auth');
            });
            var openModal = function (options) {
                return $uibModal.open(options);
            }

            this.openUploadModal = function () {

                openModal({
                    animation: true,
                    templateUrl: '/App/templates/uploadModal.html',
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
                    templateUrl: '/App/templates/createAlbum.html',
                    controller: 'createAlbumController',
                    controllerAs: 'crtAlbumCtrl'
                }).result.then(function (createdAlbum) { }, function () {
                    console.log('Modal dismissed at: ' + new Date());
                });
            }
            this.deleteAlbumModal = function () {
                openModal({
                    animation: true,
                    templateUrl: '/App/templates/deleteAlbum.html',
                    controller: 'deleteAlbumController',
                    controllerAs: 'delAlbumCtrl'
                }).result.then(function (album) {

                    $state.go('main.albums');
                }, function () {
                    console.log('Modal dismissed at: ' + new Date());
                });
            }
            this.deleteImageModal = function () {
                openModal({
                    animation: true,
                    templateUrl: '/App/templates/deleteImage.html',
                    controller: 'deleteimageController',
                    controllerAs: 'imgCtrl'
                }).result.then(function (createdAlbum) { }, function () {
                    console.log('Modal dismissed at: ' + new Date());
                });
            }

        }]);