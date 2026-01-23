angular.module('VirtoCommerce.AttributeGrid')
    .controller('VirtoCommerce.AttributeGrid.attributeGridController', [
        '$scope',
        'VirtoCommerce.AttributeGrid.webApi',
        'platformWebApp.bladeNavigationService',
        function ($scope, api, bladeNavigationService) {
            var blade = $scope.blade;
            blade.title = 'Attribute Grid';

            $scope.filters = {
                keyword: '',
                catalogId: '',
                catalogName: '',
                valueType: '',
                propertyType: '',
            };

            $scope.valueTypes = [
                { value: '', title: 'All types' },
                { value: 'ShortText', title: 'Text' },
                { value: 'Number', title: 'Number' },
                { value: 'DateTime', title: 'Date' },
                { value: 'Boolean', title: 'Boolean' },
            ];

            $scope.propertyTypes = [
                { value: '', title: 'All scopes' },
                { value: 'Product', title: 'Product' },
                { value: 'Variation', title: 'Variation' },
            ];

            $scope.pageSettings = {
                currentPage: 1,
                itemsPerPageCount: 20,
                totalItems: 0,
            };

            $scope.pageSettings.pageChanged = function () {
                blade.refresh();
            };

            blade.refresh = function () {
                blade.isLoading = true;

                var criteria = {
                    keyword: $scope.filters.keyword,
                    catalogId: $scope.filters.catalogId,
                    valueType: $scope.filters.valueType,
                    propertyType: $scope.filters.propertyType,
                    skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                    take: $scope.pageSettings.itemsPerPageCount,
                };

                api.search(criteria, function (result) {
                    blade.isLoading = false;
                    $scope.pageSettings.totalItems = result.totalCount;
                    blade.currentEntities = result.results || [];
                });
            };

            $scope.toggleFilterable = function (item) {
                api.update({ id: item.id }, { isFilterable: !item.isFilterable }, function () {
                    item.isFilterable = !item.isFilterable;
                });
            };

            $scope.openDetail = function (item) {
                var detailBlade = {
                    id: 'attributeDetail',
                    title: item.name,
                    controller: 'VirtoCommerce.AttributeGrid.attributeDetailController',
                    template: 'Modules/$(VirtoCommerce.AttributeGrid)/Scripts/blades/attribute-detail.html',
                    currentEntityId: item.id,
                };
                bladeNavigationService.showBlade(detailBlade, blade);
            };

            $scope.openCatalogSelector = function () {
                var newBlade = {
                    id: 'catalogSelector',
                    title: 'Select catalog or category',
                    controller: 'virtoCommerce.catalogModule.catalogItemSelectController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/common/catalog-items-select.tpl.html',
                    breadcrumbs: [],
                    toolbarCommands: [],
                    options: {
                        showCheckingMultiple: false,
                        checkItemFn: function (listItem, isSelected) {
                            if (isSelected) {
                                $scope.filters.catalogId = listItem.id;
                                $scope.filters.catalogName = listItem.name;
                                blade.refresh();
                                bladeNavigationService.closeBlade(newBlade);
                            }
                        },
                    },
                };

                bladeNavigationService.showBlade(newBlade, blade);
            };

            $scope.clearCatalogFilter = function ($event) {
                if ($event) {
                    $event.stopPropagation();
                }

                $scope.filters.catalogId = '';
                $scope.filters.catalogName = '';
                blade.refresh();
            };

            $scope.$watchGroup([
                function () { return $scope.filters.keyword; },
                function () { return $scope.filters.catalogId; },
                function () { return $scope.filters.valueType; },
                function () { return $scope.filters.propertyType; },
            ], function () {
                $scope.pageSettings.currentPage = 1;
                blade.refresh();
            });

            blade.refresh();
        }
    ]);
