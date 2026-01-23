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
            blade.updatePermission = 'attribute-grid:update';

            $scope.filter = {
                keyword: '',
                catalogId: null,
                valueType: null,
            };

            $scope.selectedItems = [];
            $scope.selectAll = false;

            function updateSelectedItems() {
                $scope.selectedItems = (blade.currentEntities || []).filter(function (item) { return item.$selected; });
                $scope.selectAll = $scope.selectedItems.length > 0
                    && $scope.selectedItems.length === (blade.currentEntities || []).length;
            }

            $scope.toggleSelectAll = function () {
                $scope.selectAll = !$scope.selectAll;
                angular.forEach(blade.currentEntities, function (item) {
                    item.$selected = $scope.selectAll;
                });
                updateSelectedItems();
            };

            $scope.toggleSelect = function (item, $event) {
                if ($event) {
                    $event.stopPropagation();
                }

                item.$selected = !item.$selected;
                updateSelectedItems();
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
                $scope.selectedItems = [];
                $scope.selectAll = false;

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
                    updateSelectedItems();
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
                            api.moveToTrash([item.id], function () {
                                blade.refresh();
                            }, function (error) {
                                blade.isLoading = false;
                                bladeNavigationService.setError('Ошибка удаления: ' + error.status, blade);
                            });
                        }
                    },
                };
                dialogService.showConfirmationDialog(dialog);
            };

            function getSelectedIds() {
                return $scope.selectedItems.map(function (item) { return item.id; });
            }

            $scope.bulkUpdate = function (updatePayload) {
                var ids = getSelectedIds();
                if (!ids.length) {
                    return;
                }

                var dialog = {
                    id: 'confirmBulkUpdate',
                    title: 'Массовое обновление',
                    message: 'Применить изменения к выбранным атрибутам?',
                    callback: function (confirmed) {
                        if (!confirmed) {
                            return;
                        }

                        blade.isLoading = true;
                        api.bulkUpdate({
                            ids: ids,
                            isFilterable: updatePayload.isFilterable,
                            isRequired: updatePayload.isRequired,
                        }, function () {
                            blade.refresh();
                        });
                    },
                };

                dialogService.showConfirmationDialog(dialog);
            };

            $scope.bulkDelete = function () {
                var ids = getSelectedIds();
                if (!ids.length) {
                    return;
                }

                var dialog = {
                    id: 'confirmBulkDelete',
                    title: 'Массовое удаление',
                    message: 'Удалить выбранные атрибуты в корзину?',
                    callback: function (confirmed) {
                        if (!confirmed) {
                            return;
                        }

                        blade.isLoading = true;
                        api.moveToTrash(ids, function () {
                            blade.refresh();
                        }, function (error) {
                            blade.isLoading = false;
                            bladeNavigationService.setError('Ошибка удаления: ' + error.status, blade);
                        });
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
                    name: 'platform.commands.add',
                    icon: 'fa fa-plus',
                    executeMethod: function () {
                        var newBlade = {
                            id: 'attributeDetailNew',
                            title: 'Новый атрибут',
                            controller: 'VirtoCommerce.AttributeGrid.attributeDetailController',
                            template: 'Modules/$(VirtoCommerce.AttributeGrid)/Scripts/blades/attribute-detail.html',
                            currentEntityId: null,
                        };
                        bladeNavigationService.showBlade(newBlade, blade);
                    },
                    canExecuteMethod: function () { return true; },
                    permission: 'attribute-grid:create',
                },
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
