angular.module('VirtoCommerce.AttributeGrid')
    .factory('VirtoCommerce.AttributeGrid.webApi', ['$resource', function ($resource) {
        return $resource('api/attribute-grid/:id', { id: '@id' }, {
            search: { method: 'POST', url: 'api/attribute-grid/search' },
            save: { method: 'PUT', url: 'api/attribute-grid' },
            update: { method: 'PATCH' },
            remove: { method: 'DELETE' },
            moveToTrash: { method: 'POST', url: 'api/attribute-grid/trash' },
            getTrash: { method: 'GET', url: 'api/attribute-grid/trash', isArray: true },
            restore: { method: 'POST', url: 'api/attribute-grid/trash/:id/restore' },
            bulkUpdate: { method: 'POST', url: 'api/attribute-grid/bulk-update' },
            bulkDelete: { method: 'POST', url: 'api/attribute-grid/bulk-delete' },
        });
    }]);
