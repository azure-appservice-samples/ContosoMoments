'use strict';

contosoMomentsApp
    .directive('resolvesas', ['imageService', function (imageService, $compile) {
        return function (scope, element, attrs) {

            attrs.$observe('resolvesas', function (value) {

                var size = attrs.size;
                if (!size) {
                    size = 'lg';
                }

                imageService.getImageURL(value, size)
                    .then(function (data) {

                        switch (element[0].tagName) {

                            case 'A':
                                attrs.$set('href', data);
                                break;
                            case 'IMG':
                                attrs.$set('src', data);
                                break;
                            default:
                                element.css({
                                    'background': 'transparent url(\'' + data + '\')',
                                    'background-position': 'center',
                                    'background-repeat': 'no-repeat',
                                    'zoom': 0.25
                                });
                                break;
                        }
                    });
            });
        };
    }]);