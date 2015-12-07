(function(angular){
    'use strict';

    var app=angular.module('app',['ui.router','ui.bootstrap','azureBlobUpload']);

    app.config(['$locationProvider', '$stateProvider', function ($locationProvider, $stateProvider) {
        $stateProvider
            .state('main', {
                abstract: true,
                template: "<ui-view/>",
                resolve: {
                    authContext: ['authService',function (authService) {
                        return authService.getAuthContext();
                    }]
                }
            })
            .state('main.albums', {
                url: '/',
                templateUrl: '/templates/albums.html',
                controller: 'albumsController as albumsCtrl',
                onEnter:['selectedAlbum',function(selectedAlbum){
                    selectedAlbum.album=null; 
                }]

            })
            .state('main.gallery', {
                url: '/album/:albumid',
                templateUrl: '/templates/gallery.html',
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
                url:'/image/:imageid',
                templateUrl: '/templates/singleimage.html',
                controller: 'imageController as imageCtrl',
                resolve: {
                    currentImage: ['selectedImage', '$stateParams', 'imageService', function (selectedImage, $stateParams, imageService) {
                        if (selectedImage.image)  return selectedImage;
                        if ($stateParams.imageid) {
                            return imageService.getImageById($stateParams.imageid);
                        }
                    }]
                }
            });
     
        $locationProvider.html5Mode(true);

    }])
    
})(angular)