'use strict';

contosoMomentsApp
    .directive('thumbnail', ['imageService', function (imageService, $compile) {
        return function (scope, element, attrs) {
            attrs.$observe('thumbnail', function (value) {
                var url = imageService.getImageURL(value, 'lg');
                console.log(url);
                element.css({
                    'background': 'transparent url(' + url + ')'
                });
            });
        };
    }]);