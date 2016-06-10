'use strict';

var contosoMomentsApp = angular.module('contosoMoments', ['ui.router', 'ui.bootstrap', 'azureBlobUpload']);

contosoMomentsApp.constant('appConfig', (configJson || {}));

contosoMomentsApp.value('selectedImage', { image: null, album: null });
contosoMomentsApp.value('selectedAlbum', { album: null });

contosoMomentsApp.config(['$locationProvider', '$stateProvider', '$urlRouterProvider', function ($locationProvider, $stateProvider, $urlRouterProvider) {
    $stateProvider
        .state('main', {
            abstract: true,
            template: "<ui-view/>",
        })

        .state('main.albums', {
            url: '/',
            templateUrl: 'app/templates/albums.html',
            controller: 'albumsController as albumsCtrl',
            onEnter: ['selectedAlbum', function (selectedAlbum) {
                selectedAlbum.album = null;
            }]

        })
         .state('main.auth', {
             url: '/auth',
             templateUrl: 'app/templates/auth.html'

         })
         .state('main.about', {
             url: '/about',
             templateUrl: 'app/templates/about.html'

         })
        .state('main.gallery', {
            url: '/album/:albumid',
            templateUrl: 'app/templates/gallery.html',
            controller: 'albumController as albumCtrl',
            resolve: {
                currentAlbum: ['albumsService', '$stateParams', function (albumsService, $stateParams) {
                    return albumsService.getAlbum($stateParams.albumid, { start: 0, count: 24 });
                }]
            },
            onEnter: ['selectedAlbum', 'currentAlbum', function (selectedAlbum, currentAlbum) {
                selectedAlbum.album = currentAlbum;
            }]
        })
        .state('main.singleImage', {
            url: '/image/:imageid',
            templateUrl: 'app/templates/singleimage.html',
            controller: 'imageController as imageCtrl',
            resolve: {
                currentImage: ['selectedImage', '$stateParams', 'imageService', function (selectedImage, $stateParams, imageService) {
                    if (selectedImage.image) return selectedImage;
                    if ($stateParams.imageid) {
                        return imageService.getImageById($stateParams.imageid);
                    }
                }]
            }
        })
        .state('error', {
            url: '/error/:code?',
            template: '<h2 ng-bind="errorText">',
            controller: ['$scope', '$stateParams', function ($scope, $stateParams) {
                var errCode = $stateParams.code || 500;
                switch (errCode) {
                    case 404:
                        $scope.errorText = 'Whoops, page not found.';
                    case 401:
                        $scope.errorText = 'Not Authenticated,Please Login.';
                    default:
                        $scope.errorText = 'Whoops, an error has occured.';

                }
            }]
        });

    $urlRouterProvider.when('/_=_', '/')
        .when('_-_', '/')
        .when('app/index.html', '/')
        .otherwise('/error/404');

    $locationProvider.html5Mode(true);

}]);