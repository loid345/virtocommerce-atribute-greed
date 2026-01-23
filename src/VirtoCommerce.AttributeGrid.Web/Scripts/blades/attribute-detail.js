angular.module('VirtoCommerce.AttributeGrid')
    .controller('VirtoCommerce.AttributeGrid.attributeDetailController', [
        '$scope',
        'VirtoCommerce.AttributeGrid.webApi',
        'platformWebApp.bladeNavigationService',
        '$q',
        '$translate',
        function ($scope, api, bladeNavigationService, $q, $translate) {
            var blade = $scope.blade;
            blade.headIcon = 'fa fa-tag';
            blade.updatePermission = 'attribute-grid:update';
            blade.createPermission = 'attribute-grid:create';
            blade.isNew = !blade.currentEntityId;

            blade.refresh = function () {
                if (!blade.currentEntityId) {
                    blade.title = $translate.instant('AttributeGrid.blades.detail.newTitle');
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

            function normalizeCode(code) {
                return (code || '').trim();
            }

            $scope.checkCodeUnique = function (code) {
                var normalizedCode = normalizeCode(code);
                if (!normalizedCode) {
                    return $q.resolve(true);
                }

                if (!blade.isNew && blade.origEntity && normalizedCode.toLowerCase() === normalizeCode(blade.origEntity.code).toLowerCase()) {
                    return $q.resolve(true);
                }

                return $q(function (resolve) {
                    var criteria = {
                        keyword: normalizedCode,
                        take: 50,
                    };

                    api.search(criteria, function (data) {
                        var results = data.results || data.items || [];
                        var hasDuplicate = results.some(function (item) {
                            return item.code
                                && item.code.toLowerCase() === normalizedCode.toLowerCase()
                                && (!blade.currentEntityId || item.id !== blade.currentEntityId);
                        });
                        resolve(!hasDuplicate);
                    }, function () {
                        resolve(true);
                    });
                });
            };

            $scope.saveChanges = function () {
                blade.isLoading = true;
                if (!blade.currentEntity.code) {
                    blade.currentEntity.code = blade.currentEntity.name;
                }

                if (!blade.currentEntity.id && blade.currentEntityId) {
                    blade.currentEntity.id = blade.currentEntityId;
                }

                var currentCode = blade.currentEntity.code;
                $scope.checkCodeUnique(currentCode).then(function (isUnique) {
                    if (!isUnique) {
                        blade.isLoading = false;
                        bladeNavigationService.setError(
                            $translate.instant('AttributeGrid.messages.codeExists', { code: currentCode }),
                            blade);
                        return;
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
                    }, function (error) {
                        blade.isLoading = false;
                        bladeNavigationService.setError(
                            $translate.instant('AttributeGrid.messages.saveError', { status: error.status }),
                            blade);
                    });
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
