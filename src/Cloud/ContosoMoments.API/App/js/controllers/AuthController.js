'use strict';

contosoMomentsApp
    .controller('titleController', ['$scope', 'selectedAlbum',
    function ($scope, selectedAlbum) {

        $scope.$on('userAuthenticated', function (e, authContext) {
            $scope.userEmail = authContext.currentUser.email;
        });
        $scope.curAlbum = selectedAlbum;

    }]);