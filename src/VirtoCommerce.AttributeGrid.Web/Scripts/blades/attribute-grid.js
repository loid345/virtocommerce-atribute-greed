angular.module('VirtoCommerce.AttributeGrid')
    .controller('VirtoCommerce.AttributeGrid.attributeGridController', ['$scope', function ($scope) {
        var blade = $scope.blade;
        blade.title = 'Attribute Grid';
        blade.isLoading = false;

        blade.filters = {
            keyword: '',
            catalog: 'All',
            type: 'All',
        };

        blade.catalogs = [
            'All',
            'Electronics',
            'Appliances',
            'Fashion',
        ];

        blade.types = [
            'All',
            'Text',
            'Number',
            'Dictionary',
            'Date',
            'Boolean',
        ];

        blade.items = [
            {
                name: 'Brand',
                code: 'brand',
                type: 'Dictionary',
                scope: 'Product',
                ownerPath: 'Electronics',
                catalog: 'Electronics',
                isFilterable: true,
                usage: 410,
            },
            {
                name: 'Screen Size',
                code: 'screen',
                type: 'Number',
                scope: 'Product',
                ownerPath: 'Electronics / Phones',
                catalog: 'Electronics',
                isFilterable: true,
                usage: 128,
            },
            {
                name: 'Color',
                code: 'color',
                type: 'Dictionary',
                scope: 'Variant',
                ownerPath: 'Fashion',
                catalog: 'Fashion',
                isFilterable: true,
                usage: 850,
            },
            {
                name: 'Weight',
                code: 'weight',
                type: 'Number',
                scope: 'Product',
                ownerPath: 'Appliances / Kitchen',
                catalog: 'Appliances',
                isFilterable: false,
                usage: 12,
            },
        ];

        blade.updateFiltered = function () {
            var keyword = (blade.filters.keyword || '').toLowerCase();
            var selectedCatalog = blade.filters.catalog;
            var selectedType = blade.filters.type;

            blade.filteredItems = blade.items.filter(function (item) {
                var matchesKeyword = !keyword ||
                    item.name.toLowerCase().indexOf(keyword) !== -1 ||
                    item.code.toLowerCase().indexOf(keyword) !== -1;
                var matchesCatalog = selectedCatalog === 'All' || item.catalog === selectedCatalog;
                var matchesType = selectedType === 'All' || item.type === selectedType;
                return matchesKeyword && matchesCatalog && matchesType;
            });
        };

        $scope.$watchGroup([
            function () { return blade.filters.keyword; },
            function () { return blade.filters.catalog; },
            function () { return blade.filters.type; },
        ], blade.updateFiltered);

        blade.updateFiltered();
    }]);
