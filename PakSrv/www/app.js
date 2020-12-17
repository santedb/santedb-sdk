'use strict';

angular.module('pakman', [])
    .config(['$compileProvider', '$httpProvider', function ($compileProvider, $httpProvider) {
        $compileProvider.aHrefSanitizationWhitelist(/^\s*(http|https|tel|mailto):/);
        $compileProvider.imgSrcSanitizationWhitelist(/^\s*(http|https):/);

    }]).controller("IndexController", ["$scope", "$http", function ($scope, $http) {

        var offset = 0;
        var lastScroll = 0;
        $scope.filter = "";

        function fetch(offset, filter, replace) {
            $scope.isLoading = true;     
            $http.get(`./pak?_count=10&_offset=${offset}&name.value=${filter}`)
                .then(
                    (data) => {
                        if(replace || !$scope.packageList) 
                            $scope.packageList = data.data;
                        else 
                            data.data.forEach(d=>$scope.packageList.push(d));
                        $scope.isLoading = false;     
                    },
                    (err) => {
                        $scope.error = err;
                        $scope.isLoading = false;     
                    }
                );
        }

        fetch(offset, "", true);

        var preventScroll = false;

        $scope.searchPackages = function(form) {
            if(!form.$valid) return;
            $('html,body').animate({
                scrollTop: 0
            },
                'fast');
                delete($scope.packageList);
            fetch(0, `:(nocase)~${$scope.filter}`, true);
        }
        document.addEventListener('scroll', function (e) {
            if (!preventScroll && window.innerHeight + window.scrollY >= document.body.offsetHeight && window.scrollY + window.innerHeight > lastScroll) {
                preventScroll = true;
                if ($scope.isLoading) return;
                lastScroll = window.scrollY + window.innerHeight;
                offset += 10;
                fetch(offset, $scope.filter || "", false);
                preventScroll = false;
            }
        });
    }])
    .run(['$rootScope', '$interval', function ($rootScope, $interval) {

        
    }]);