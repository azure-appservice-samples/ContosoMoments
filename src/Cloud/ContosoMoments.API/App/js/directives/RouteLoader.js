'use strict';

contosoMomentsApp
    .directive('routeLoader', function ($rootScope, $timeout) {

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