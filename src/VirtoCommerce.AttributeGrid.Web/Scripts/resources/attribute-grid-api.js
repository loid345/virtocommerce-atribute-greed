angular.module('VirtoCommerce.AttributeGrid')
    .factory('VirtoCommerce.AttributeGrid.webApi', ['$resource', function ($resource) {
        return $resource('api/attribute-grid');
    }]);
