'use strict';

contosoMomentsApp
    .directive('fileChange', [function () {
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