(function (angular) {
    'use strict';
    var app = angular.module('app');
    app.directive('fileChange', [function () {
        return {
            restrict: 'A',
            scope: {
                fileChange: '&'
            },
            link: function (scope, elem, attr) {

                if (elem[0].tagName === 'INPUT' && attr.type === 'file') {

                    elem.on('change', function (ev) {

                        scope.$apply(function () {
                            scope.fileChange({ files: ev.target.files, event: ev });

                        })

                    });


                }

            }
        }

    }]);

    app.directive('routeLoader', function ($rootScope, $timeout) {

        return {
            restrict: 'E',
            replace: true,
            template: '<div><div class="loader-backdrop"></div><div class="loader"><div class="text-center"><strong>Loading<span class="icon-spin3 animate-spin"></span></strong><div></div></div>',
            link: function (scope, element) {
                element.addClass('ng-hide');

                var unregister = $rootScope.$on('$stateChangeStart', function () {
                    element.removeClass('ng-hide');            
                });
                $rootScope.$on('$stateChangeSuccess', function () {
                    element.addClass('ng-hide');
                });

                scope.$on('$destroy', unregister);
            }
        };
    });


})(angular)