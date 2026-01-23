angular.module('VirtoCommerce.AttributeGrid')
    .factory('VirtoCommerce.AttributeGrid.webApi', ['$resource', function ($resource) {
        return $resource('api/attribute-grid/:id', { id: '@id' }, {
            search: { method: 'POST', url: 'api/attribute-grid/search' },
            update: { method: 'PATCH' },
        });
    }]);
