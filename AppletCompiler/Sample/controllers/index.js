angular.module('layout').controller('IndexController', ['$scope', function ($scope) {
    $scope.someLocalVariable = "Hello Index!";
    $scope.someAction = function () { alert("Hi from Index!"); }
}]);