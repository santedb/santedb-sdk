angular.module('layout').controller('LayoutController', ['$scope', function ($scope) {

    $scope.someVariable = "Hello!";
    $scope.someAction = function () { alert("Hi from layout!"); }
}]);