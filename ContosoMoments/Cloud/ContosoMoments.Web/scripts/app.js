(function(angular){
    'use strict';

    var app=angular.module('app',['ngRoute','ui.bootstrap','azureBlobUpload']);

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

    app.factory('uploadService', ['azureBlob','$http', function(azureBlob,$http){
        var getSasUrl=function(){
            return $http.get("http://localhost:31475/api/GetSasUrl").then(function (res) {
                return res.data;
            });
        }
        var commit=function (sasurl) {
            return $http.post("http://localhost:31475/api/CommitBlob", {
                isMobile:true,
                UserId: "11111111-1111-1111-1111-111111111111",
                AlbumId: "11111111-1111-1111-1111-111111111111",
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