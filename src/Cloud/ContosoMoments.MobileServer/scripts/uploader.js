(function(angular){
    'use strict';

    var module=angular.module('blobUploader',['azureBlobUpload']);

    module.controller('appController', ['$scope', function($scope){
        
        $scope.onFileChange=function(files,ev){
            console.log(files);
            console.log(ev);
        }

    }])
    
    module.directive('fileChange',[function(){
        
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
    
    module.directive('uploader', ['azureBlob','$http','$document',function (azureBlob,$http,$document) {
        var getSasUrl=function(){
            return $http.get("http://localhost:3242/api/GetSasUrl").then(function (res) {
                return res.data;
            });
        }
        var currentFile;
        return {
            template:'<input type="file" id="file" name="file" style="width: 50%" /><button>Upload</button>',
            link:function(scope,elem,attrs){
                var input=elem.find("input");
                input.on('change',function(e){
                    currentFile=e.target.files[0];
                    alert('selected');
                });
                elem.find("button").on('click',function(e){
                    var selectedFileName = input.val().replace(/^.*[\\\/]/, '');
                    var fileName = selectedFileName;
                    getSasUrl().then(function(res){
                        var sasurl = res;
                        var urlParts=res.split('?');   
                        azureBlob.upload({
                          baseUrl: urlParts[0],// baseUrl for blob file uri (i.e. http://<accountName>.blob.core.windows.net/<container>/<blobname>),
                          sasToken: '?'+urlParts[1], // Shared access signature querystring key/value prefixed with ?,
                          file:currentFile, // File object using the HTML5 File API,
                          progress:function(){}, // progress callback function,
                          complete:function(){
                            alert('complete');
                          }, // complete callback function,
                          error: function(){ alert('error')}// error callback function,                       
                        });
                        //setFile($("#file")[0].files[0], res); 
                    });
                });
                
            }
        }

    }]);


}(angular))