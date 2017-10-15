var app = angular.module('SimpleStore', ['ui.bootstrap']);
app.run(function () { });

var catalog = {
    items: [
        {
            productName: "Xbox One X",
            unitPrice: 499.99,
            quantity: 0,
            imageUrl: "images/xbox-x.png"
        },
        {
            productName: "Xbox One S",
            unitPrice: 349.00,
            quantity: 0,
            imageUrl: "images/xbox-s.png"
        },
        {
            productName: "Xbox Wireless Controller",
            unitPrice: 74.99,
            quantity: 0,
            imageUrl: "images/xbox-joy.png"
        },
        {
            productName: "Kinect for Xbox One",
            unitPrice: 74.99,
            quantity: 0,
            imageUrl: "images/xbox-kinect.png"
        },
        {
            productName: "Xbox Stereo Headset",
            unitPrice: 74.99,
            quantity: 0,
            imageUrl: "images/xbox-head.png"
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
    $http.remove = function (name) {
        $http.delete('api/Carts/' + name)
            .then(function (res, status) {
                $scope.refresh();
            })
    };
    $http.update = function (item) {
        $http.post('api/Carts', item)
            .then(function (res, status) {
                item.quantity = 0;
                $scope.refresh();
            });
    };
    $scope.addToCart = function (item){
        $http.update(item);
    }
    $scope.removeFromCart = function (item) {
        $http.remove(item.productName);
    }
}]);
