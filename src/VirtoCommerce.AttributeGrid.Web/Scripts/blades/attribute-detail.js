angular.module('VirtoCommerce.AttributeGrid')
    .controller('VirtoCommerce.AttributeGrid.attributeDetailController', [
        '$scope',
        'VirtoCommerce.AttributeGrid.webApi',
        'platformWebApp.bladeNavigationService',
        function ($scope, api, bladeNavigationService) {
            var blade = $scope.blade;
            blade.headIcon = 'fa fa-tag';
            blade.updatePermission = 'attribute-grid:update';
            blade.createPermission = 'attribute-grid:create';
            blade.isNew = !blade.currentEntityId;

            blade.refresh = function () {
                if (!blade.currentEntityId) {
                    blade.title = 'Новый атрибут';
                    blade.currentEntity = {
                        name: '',
                        code: '',
                        valueType: 'ShortText',
                        propertyType: 'Product',
                        isFilterable: false,
                        isRequired: false,
                        isDictionary: false,
                        isMultivalue: false,
                    };
                    blade.origEntity = angular.copy(blade.currentEntity);
                    return;
                }

                blade.isLoading = true;
                api.get({ id: blade.currentEntityId }, function (data) {
                    blade.currentEntity = data;
                    blade.origEntity = angular.copy(data);
                    blade.title = data.name;
                    blade.isNew = false;
                    blade.isLoading = false;
                }, function () {
                    blade.isLoading = false;
                });
            };

            $scope.isDirty = function () {
                return !angular.equals(blade.currentEntity, blade.origEntity);
            };

            $scope.saveChanges = function () {
                blade.isLoading = true;
                if (!blade.currentEntity.code) {
                    blade.currentEntity.code = blade.currentEntity.name;
                }

                if (!blade.currentEntity.id && blade.currentEntityId) {
                    blade.currentEntity.id = blade.currentEntityId;
                }

                api.save(blade.currentEntity, function (data) {
                    blade.currentEntity = data;
                    blade.currentEntityId = data.id;
                    blade.isNew = false;
                    blade.title = data.name;
                    blade.origEntity = angular.copy(blade.currentEntity);
                    blade.isLoading = false;
                    if (blade.parentBlade && blade.parentBlade.refresh) {
                        blade.parentBlade.refresh();
                    }
                }, function () {
                    blade.isLoading = false;
                });
            };

            $scope.resetChanges = function () {
                blade.currentEntity = angular.copy(blade.origEntity);
            };

            blade.toolbarCommands = [
                {
                    name: 'platform.commands.save',
                    icon: 'fa fa-save',
                    executeMethod: $scope.saveChanges,
                    canExecuteMethod: $scope.isDirty,
                    permission: blade.isNew ? blade.createPermission : blade.updatePermission,
                },
                {
                    name: 'platform.commands.reset',
                    icon: 'fa fa-undo',
                    executeMethod: $scope.resetChanges,
                    canExecuteMethod: $scope.isDirty,
                    permission: blade.updatePermission,
                },
                {
                    name: 'platform.commands.close',
                    icon: 'fa fa-times',
                    executeMethod: function () { bladeNavigationService.closeBlade(blade); },
                }
            ];

            blade.refresh();
        }
    ]);
