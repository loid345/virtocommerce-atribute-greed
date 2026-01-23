angular.module('VirtoCommerce.AttributeGrid')
    .controller('VirtoCommerce.AttributeGrid.trashWidgetController', [
        '$scope',
        'VirtoCommerce.AttributeGrid.webApi',
        'platformWebApp.bladeNavigationService',
        '$translate',
        function ($scope, api, bladeNavigationService, $translate) {
            $scope.trashCount = 0;

            function refresh() {
                api.getTrash({}, function (data) {
                    $scope.trashCount = (data || []).length;
                });
            }

            $scope.openTrash = function () {
                var trashBlade = {
                    id: 'attributeTrash',
                    title: $translate.instant('AttributeGrid.blades.trash.title'),
                    subtitle: $translate.instant('AttributeGrid.blades.trash.subtitle'),
                    controller: 'VirtoCommerce.AttributeGrid.attributeTrashListController',
                    template: 'Modules/$(VirtoCommerce.AttributeGrid)/Scripts/blades/attribute-trash-list.html',
                };

                bladeNavigationService.showBlade(trashBlade);
            };

            refresh();
        },
    ]);
