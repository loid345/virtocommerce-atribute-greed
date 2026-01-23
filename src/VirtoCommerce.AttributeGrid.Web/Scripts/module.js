// Call this to register your module to main application
var moduleName = 'VirtoCommerce.AttributeGrid';

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, ['virtoCommerce.catalogModule'])
    .config(['$stateProvider',
        function ($stateProvider) {
            $stateProvider
                .state('workspace.AttributeGridState', {
                    url: '/attribute-grid',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: [
                        'platformWebApp.bladeNavigationService',
                        function (bladeNavigationService) {
                            var newBlade = {
                                id: 'blade1',
                                controller: 'VirtoCommerce.AttributeGrid.attributeGridController',
                                template: 'Modules/$(VirtoCommerce.AttributeGrid)/Scripts/blades/attribute-grid.html',
                                isClosingDisabled: true,
                            };
                            bladeNavigationService.showBlade(newBlade);
                        }
                    ]
                });
        }
    ])
    .run(['platformWebApp.mainMenuService', '$state', '$translate',
        function (mainMenuService, $state, $translate) {
            //Register module in main menu
            var menuItem = {
                path: 'browse/attribute-grid',
                icon: 'fa fa-cube',
                title: $translate.instant('AttributeGrid.mainMenu.title'),
                priority: 100,
                action: function () { $state.go('workspace.AttributeGridState'); },
                permission: 'attribute-grid:access',
            };
            mainMenuService.addMenuItem(menuItem);
        }
    ]);
