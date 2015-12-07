(function(angular){
    'use strict';

    var app=angular.module('app',['ngRoute','ui.bootstrap','azureBlobUpload']);

    app.constant('appConfig',(configJson || {})); 
    app.config(['$locationProvider','$routeProvider',function($locationProvider,$routeProvider) {
        $routeProvider.when('/',{
           templateUrl:'/templates/gallery.html',
           controller:'albumController',
           controllerAs:'albumCtrl'

        })
        .when('/albums/:albumid/images/:imageid',{
            templateUrl: '/templates/singleimage.html',
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

    app.factory('mobileServicesClient', [function () {
        
        return function (appUrl, appKey) {
            var MobileServiceClient = WindowsAzure.MobileServiceClient;
            return new MobileServiceClient(appUrl, appKey);
        }


    }]);

    app.factory('albumsService', ['$http', '$q', '$cacheFactory', '$interpolate', 'appConfig', 'mobileServicesClient', function ($http, $q, $cacheFactory, $interpolate, appConfig, mobileServicesClient) {
        var albumCache = $cacheFactory('albums');  
       
        var urlExp = $interpolate('{{image.containerName}}/{{size}}/{{image.fileName}}.jpg');
        var imageUrlExp = $interpolate(appConfig.DefaultServiceUrl + 'tables/image?start={{start}}&count={{count}}');
        var albumDefaultOptions = {
            start: 0,
            count: 50
        };
        var mobileSrvClient = mobileServicesClient(appConfig.DefaultServiceUrl);
        var getAlbumReqOptions = function (options) {
            return angular.extend(albumDefaultOptions, options);
        }
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
        var getAlbumCacheKey = function (id, options) {
            return id + options.start + options.count;
        }
        var getAlbumFromCache=function(key){
            return albumCache.get(key);
        }

        var albumService = {
          getAlbum:function(id,options){

            var defered = $q.defer();
            var reqOptions = getAlbumReqOptions(options);
            var albumCacheKey = getAlbumCacheKey(id, reqOptions);
            var currentAlbum = getAlbumFromCache(albumCacheKey);
            if(angular.isUndefined(currentAlbum)){
                var reqUrl = imageUrlExp(reqOptions);

                var imageTable = mobileSrvClient.getTable('image');
                imageTable.skip(reqOptions.start).take(reqOptions.count).read().done(function (results) {
                    currentAlbum = { id: 1, name: 'Portraits', owner: 'John Doe' };
                    currentAlbum.images = results;
                    albumCache.put(albumCacheKey, currentAlbum);
                    defered.resolve(currentAlbum);
                }, function (error) {
                    console.log(error);
                    defered.reject(error);
                });
            } else {
                defered.resolve(currentAlbum);
            }
            return defered.promise;
          },
          getUserAlbums: function () {

          }
        }  
        return albumService;         
    }])

    app.factory('uploadService', ['azureBlob','$http','appConfig',function(azureBlob,$http,appConfig){
        var getSasUrl=function(){
            return $http.get(appConfig.DefaultServiceUrl+"/api/GetSasUrl").then(function (res) {
                return res.data;
            });
        }
        var commit=function (sasurl) {
            return $http.post(appConfig.DefaultServiceUrl+'api/CommitBlob', {
                isMobile:false,
                UserId: appConfig.DefaultUserId,
                AlbumId: appConfig.DefaultAlbumId,
                SasUrl: sasurl,
                //sendNotification: store.sendNotification(blobface.selectedFile.name)
            }).then(function (res) {
                return res.data;
            });
        }
        return function upload(currentFile,options){
            var config=options || {};

            getSasUrl().then(function(res){
                var sasurl = res;
                var urlParts=res.split('?');   
                azureBlob.upload({
                  baseUrl: urlParts[0],// baseUrl for blob file uri (i.e. http://<accountName>.blob.core.windows.net/<container>/<blobname>),
                  sasToken: '?'+urlParts[1], // Shared access signature querystring key/value prefixed with ?,
                  file:currentFile, // File object using the HTML5 File API,
                  progress:config.progress || angular.noop, // progress callback function,
                  complete:function(){
                    commit(sasurl).then(function(){
                        if(angular.isFunction(config.complete)){
                            config.complete();
                        }
                    })
                    
                  },// complete callback function,
                  error: config.error || angular.noop// error callback function,                       
                });
                //setFile($("#file")[0].files[0], res); 
            });
        };
    }]);


    app.controller('albumController', ['albumsService',function (albumsService) {
        var self = this;
        this.currentIndex = 0;
        this.count = 24;

        this.getAlbumImages = function () {
            albumsService.getAlbum('1', { start: self.currentIndex, count: self.count }).then(function (album) {
                if (album.images && album.images.length >= self.count) {
                    album.images[album.images.length - 1].showPlaceholder = true;
                }

                if (self.currentIndex === 0) {
                    self.curAlbum = album;
                }
                else {
                    self.curAlbum.images[self.curAlbum.images.length - 1].showPlaceholder = false;
                    self.curAlbum.images = self.curAlbum.images.concat(album.images);
                }
               
            });
        }
        this.showNext = function () {
            this.currentIndex += this.count;
            this.getAlbumImages();
        }


       
        this.getAlbumImages();
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

    app.controller('navController', ['$uibModal',function($uibModal){
        

        this.openUploadModal=function(){

            var modalInstance = $uibModal.open({
              animation: true,
              templateUrl: 'uploadModal.html',
              controller: 'uploadController',
              controllerAs:'uploadCtrl'
            });

            modalInstance.result.then(function (uplodedFile) {
              console.log('upload modal completed at: ' + new Date());
            }, function () {
              console.log('Modal dismissed at: ' + new Date());
            });
            
        }


      

    }]);
    app.controller('uploadController',['$scope','uploadService','$uibModalInstance','$timeout',function($scope,uploadService,$uibModalInstance,$timeout){
        $scope.progress=-1; 
        var uploadOptions={
            complete:function(){
                $timeout(function(){
                    $uibModalInstance.close($scope.selectedFile);
                },1500);
                 
            },
            progress:function(progress){
                $scope.progress=parseFloat(progress);
            }
        }
        this.showProgress=function(){
            return ($scope.progress >=0);
        }
        this.onFileChange=function(files){
            $scope.selectedFile=files[0];
        }
        this.upload=function(){
            if(!angular.isUndefined($scope.selectedFile)){
                $scope.progress=0;
                uploadService($scope.selectedFile,uploadOptions);
            }
        }
        this.cancel=function(){
            $uibModalInstance.dismiss('cancel');
        }

    }]);

    app.directive('fileChange',[function(){
        return {    
            restrict:'A',
            scope:{
                fileChange:'&'
            },
            link:function(scope,elem,attr){

                if(elem[0].tagName==='INPUT' && attr.type==='file'){

                    elem.on('change',function(ev){

                        scope.$apply(function(){
                            scope.fileChange({files:ev.target.files,event:ev});

                        })

                    });


                }

            }
        }

    }]);


})(angular)