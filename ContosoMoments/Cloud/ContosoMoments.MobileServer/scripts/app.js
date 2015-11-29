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
            .state('main.gallery', {
                url: '/',
                templateUrl: '/templates/gallery.html',
                controller: 'albumController as albumCtrl'
            })
            .state('main.singleImage', {
                url:'/images/:imageid',
                templateUrl: '/templates/singleimage.html',
                controller: 'imageController as imageCtrl',
                resolve: {
                    currentImage: ['selectedImage', '$stateParams', function (selectedImage, $stateParams) {
                        return selectedImage;
                    }]
                }
            });
     
        $locationProvider.html5Mode(true);

    }])
    
})(angular)