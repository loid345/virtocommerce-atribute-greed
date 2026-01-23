angular.module('VirtoCommerce.AttributeGrid')
    .controller('VirtoCommerce.AttributeGrid.attributeTrashListController', [
        '$scope',
        'VirtoCommerce.AttributeGrid.webApi',
        '$translate',
        function ($scope, api, $translate) {
            var blade = $scope.blade;
            blade.title = $translate.instant('AttributeGrid.blades.trash.title');
            blade.headIcon = 'fa fa-trash';

            blade.refresh = function () {
                blade.isLoading = true;
                api.getTrash({}, function (data) {
                    blade.currentEntities = data || [];
                    blade.isLoading = false;
                }, function () {
                    blade.isLoading = false;
                });
            };

            $scope.restore = function (item) {
                api.restore({ id: item.id }, {}, function () {
                    blade.refresh();
                    if (blade.parentBlade && blade.parentBlade.refresh) {
                        blade.parentBlade.refresh();
                    }
                });
            };

            blade.toolbarCommands = [
                {
                    name: 'platform.commands.refresh',
                    icon: 'fa fa-refresh',
                    executeMethod: blade.refresh,
                    canExecuteMethod: function () { return true; },
                },
            ];

            blade.refresh();
        }
    ]);
