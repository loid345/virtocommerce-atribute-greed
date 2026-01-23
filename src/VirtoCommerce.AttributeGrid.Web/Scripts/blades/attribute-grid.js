angular.module('VirtoCommerce.AttributeGrid')
    .controller('VirtoCommerce.AttributeGrid.attributeGridController', [
        '$scope',
        'VirtoCommerce.AttributeGrid.webApi',
        'platformWebApp.bladeNavigationService',
        'platformWebApp.dialogService',
        'virtoCommerce.catalogModule.catalogs',
        function ($scope, api, bladeNavigationService, dialogService, catalogsResource) {
            var blade = $scope.blade;
            blade.title = 'Менеджер Атрибутов';
            blade.headIcon = 'fa fa-tags';
            blade.updatePermission = 'propertiesManager:update';

            $scope.filter = {
                keyword: '',
                catalogId: null,
                valueType: null,
            };

            $scope.catalogs = [];

            function loadCatalogs() {
                catalogsResource.getCatalogs({}, function (data) {
                    $scope.catalogs = data;
                });
            }

            $scope.pageSettings = {
                currentPage: 1,
                itemsPerPageCount: 20,
                totalItems: 0,
            };

            blade.refresh = function () {
                blade.isLoading = true;

                var criteria = {
                    keyword: $scope.filter.keyword,
                    catalogId: $scope.filter.catalogId,
                    valueType: $scope.filter.valueType,
                    skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                    take: $scope.pageSettings.itemsPerPageCount,
                };

                api.search(criteria, function (result) {
                    blade.isLoading = false;
                    blade.currentEntities = result.results || result.items || [];
                    $scope.pageSettings.totalItems = result.totalCount;
                }, function (error) {
                    blade.isLoading = false;
                    bladeNavigationService.setError('Ошибка загрузки: ' + error.status, blade);
                });
            };

            blade.selectNode = function (item) {
                $scope.selectedNodeId = item.id;

                var detailBlade = {
                    id: 'attributeDetail',
                    title: item.name,
                    subtitle: 'Редактирование атрибута',
                    controller: 'VirtoCommerce.AttributeGrid.attributeDetailController',
                    template: 'Modules/$(VirtoCommerce.AttributeGrid)/Scripts/blades/attribute-detail.html',
                    currentEntityId: item.id,
                };
                bladeNavigationService.showBlade(detailBlade, blade);
            };

            function openTrash() {
                var trashBlade = {
                    id: 'attributeTrash',
                    title: 'Корзина',
                    subtitle: 'Удалённые атрибуты',
                    controller: 'VirtoCommerce.AttributeGrid.attributeTrashListController',
                    template: 'Modules/$(VirtoCommerce.AttributeGrid)/Scripts/blades/attribute-trash-list.html',
                };

                bladeNavigationService.showBlade(trashBlade, blade);
            }

            $scope.deleteItem = function (item) {
                var dialog = {
                    id: 'confirmDelete',
                    title: 'Удаление атрибута',
                    message: 'Удалить атрибут "' + item.name + '" в корзину?',
                    callback: function (confirmed) {
                        if (confirmed) {
                            blade.isLoading = true;
                            api.remove({ id: item.id }, function () {
                                blade.refresh();
                            });
                        }
                    },
                };
                dialogService.showConfirmationDialog(dialog);
            };

            $scope.toggleFilterable = function (item, $event) {
                if ($event) {
                    $event.stopPropagation();
                }

                api.update({ id: item.id }, { isFilterable: !item.isFilterable }, function () {
                    item.isFilterable = !item.isFilterable;
                });
            };

            blade.toolbarCommands = [
                {
                    name: 'platform.commands.refresh',
                    icon: 'fa fa-refresh',
                    executeMethod: blade.refresh,
                    canExecuteMethod: function () { return !blade.isLoading; },
                },
                {
                    name: 'Корзина',
                    icon: 'fa fa-trash-o',
                    executeMethod: openTrash,
                    canExecuteMethod: function () { return true; },
                },
            ];

            loadCatalogs();
            blade.refresh();
        },
    ]);
