var app = angular.module('SimpleStore', ['ui.bootstrap']);
app.run(function () { });

var catalog = {
    items: [
        {
            productName: "XBOX",
            unitPrice: 499.9,
            quantity: 0,
            imageUrl: ""
        },
        {
            productName: "PlayStation",
            unitPrice: 499.9,
            quantity: 0,
            imageUrl: ""
        }
    ]
};

app.controller('SimpleStoreController', ['$scope', '$http', function ($scope, $http) {
    $scope.refresh = function () {
        $http.get('api/Carts')
            .then(function (res, status) {
                $scope.cart = res.data;
                $scope.catalog = catalog;
            }, function (res, status) {
                $scope.cart = undefined;
            });
    }
    $http.add = function (item) {
        $http.put('api/Carts', item)
            .then(function (res, status) {
                $scope.refresh();
            });
    };
    $http.remove = function (name) {
        $http.delete('api/Carts/' + name)
            .then(function (res, status) {
                $scope.refresh();
            })
    };
    $http.update = function (item) {
        $http.post('api/Carts', item)
            .then(function (res, status) {
                $scope.refresh();
            });
    };
    $scope.addToCart = function (item){
        alert(item.quantity);
    }
    $scope.removeFromCart = function (item){
        alert(item.quantity);
    }
    
}]);
