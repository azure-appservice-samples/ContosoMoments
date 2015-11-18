(function(angular){
    'use strict';

    var app=angular.module('app',['ngRoute']);

    app.config(['$locationProvider','$routeProvider',function($locationProvider,$routeProvider) {
        $routeProvider.when('/',{
           templateUrl:'templates/gallery.html',
           controller:'albumController',
           controllerAs:'albumCtrl'

        })
        .when('/albums/:albumid/images/:imageid',{
            templateUrl: 'templates/singleimage.html',
            controller:'imageController',
            controllerAs:'imageCtrl',   
            resolve:{
                imageData:['$route','albumsService',function($route,albumsService){
                    console.log('images route...');
                    return albumsService.getAlbum($route.current.params.albumid).then(function(album){
                        var img = albumsService.getImage($route.current.params.imageid, album);
                        img.url = albumsService.getImageURL(img, 'lg');
                        var imageData = {
                                album:album,
                                image: img
                        }
                       
                        return imageData;
                     })
                    
                }]
            }
        })
        .otherwise({
            template:"Page not found"
        })

        $locationProvider.html5Mode(true);

    }])


    app.factory('albumsService', ['$http', '$q', '$cacheFactory', '$interpolate', function ($http, $q, $cacheFactory, $interpolate) {
        var albumCache=$cacheFactory('albums');
        var urlExp = $interpolate('{{image.containerName}}/{{size}}/{{image.fileName}}.jpg');
  
        var getImageFromAlbum = function (album, id) {
            if(album && album.images){
                var images=album.images.filter(function(item){
                    return item.id===id;
                });

                if(images.length>0){
                    return images[0];
                }
                else{
                    return null;
                }
            }
            return null;
        }

        var getAlbumFromCache=function(albumid){
            return albumCache.get(albumid);
        }

        var albumService = {
           getImageURL:function(img,imgSize){
               return urlExp({ image: img, size: imgSize });
           },
          getImage:function(id,currentAlbum){

            var img;        
            if(angular.isDefined(currentAlbum)){
                img=getImageFromAlbum(currentAlbum,id);
                
            }
               
            return img;
          },
          getAlbum:function(id){

            var defered=$q.defer();

            
            var currentAlbum=albumCache.get(id);
            if(angular.isUndefined(currentAlbum)){
                //TODO:Wrap with $http
                $http.get('http://localhost:31475/tables/image').then(function(res){
                    currentAlbum = { id: 1, name: 'Portraits', owner: 'John Doe' };
                    currentAlbum.images=res.data;
                    albumCache.put(id,currentAlbum);
                    defered.resolve(currentAlbum);
                },
                function(err){
                    //TODO: Error Handling
                    console.log(err);
                    defered.reject(err);
                });
            } else {
                defered.resolve(currentAlbum);
            }
            return defered.promise;
          } 
        }  
        return albumService;         
    }])


    app.controller('albumController', ['albumsService',function (albumsService) {
        var self=this;
        albumsService.getAlbum('1').then(function (album) {
            self.curAlbum=album;
            setTimeout(function () {
                objectFit.polyfill({
                    selector: 'img',
                    fittype: 'cover'
                });
            },1000);
        });

        self.getImageURL = function (img, size) {
            return albumsService.getImageURL(img, size);
        }
        
    }]);
    app.controller('imageController', ['imageData', 'albumsService', function (imageData, albumsService) {
        this.currentImage=imageData.image;
        this.currentAlbum = imageData.album;
        this.getCurrentImageURL = function (size) {
            return albumsService.getImageURL(this.currentImage, size);
        }


    }]);


})(angular)