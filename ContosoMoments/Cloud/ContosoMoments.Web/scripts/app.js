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
                       return {
                                album:album,
                                image:albumsService.getImage($route.current.params.imageid,album)
                            }
                     })
                    
                }]
            }
        })
        .otherwise({
            template:"Page not found"
        })

        $locationProvider.html5Mode(true);

    }])


    app.factory('albumsService', ['$http','$q','$cacheFactory',function($http,$q,$cacheFactory){
        var albumCache=$cacheFactory('albums');

        var getImageFromAlbum=function(album,id){
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

        var albumService={
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
                currentAlbum={
                            id:"1",
                            name:'Protraits',
                            owner:'Jonn Doe',
                            images:[
                                {
                                    id:'1',
                                    name:'autoprefixer',
                                    url:'images/img1.jpg'
                                },
                                {
                                    id:'2',
                                    name:'autoprefixer',
                                    url: 'images/img2.jpg'
                                },
                                {
                                    id:'3',
                                    name:'autoprefixer',
                                    url: 'images/img3.jpg'
                                },
                                {
                                    id:'4',
                                    name:'autoprefixer',
                                    url: 'images/img4.jpg'
                                },
                                {
                                    id:'5',
                                    name:'autoprefixer',
                                    url: 'images/img5.jpg'
                                },
                                {
                                    id:'6',
                                    name:'autoprefixer',
                                    url: 'images/img6.jpg'
                                },
                                {
                                    id:'7',
                                    name:'autoprefixer',
                                    url: 'images/img7.jpg'
                                },{
                                    id:'8',
                                    name:'autoprefixer',
                                    url: 'images/img8.jpg'
                                },
                                {
                                    id:'9',
                                    name:'autoprefixer',
                                    url: 'images/img9.jpg'
                                },
                                {
                                    id:'10',
                                    name:'autoprefixer',
                                    url: 'images/img10.jpg'
                                },
                                {
                                    id:'11',
                                    name:'autoprefixer',
                                    url: 'images/img11.jpg'
                                },
                                {
                                    id:'12',
                                    name:'autoprefixer',
                                    url:'images/img12.jpg'
                                } 
                           ]
                };
                albumCache.put(id,currentAlbum);
            }
            
            
            defered.resolve(currentAlbum);
            return defered.promise;
          } 
        }  
        return albumService;         
    }])


    app.controller('albumController', ['albumsService',function(albumsService){
        var self=this;
        albumsService.getAlbum('1').then(function(album){
            self.curAlbum = album;
            setTimeout(function () {
                objectFit.polyfill({
                    selector: 'img',
                    fittype: 'cover'
                });
            },1000);
        });
        
    }]);
    app.controller('imageController', ['imageData',function(imageData){
        this.currentImage=imageData.image;
        this.currentAlbum=imageData.album;

    }]);


})(angular)