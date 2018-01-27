var dns = require('dns');
var request = require('request');
var express = require('express');
var app = express();

app.get('/', function(req,res) {
	var ipAddress = '';
	var url = '';
	var port = 8082;
	dns.resolve('backend.irisapp', function (errors, ipAddresses) {
		if (errors) {
			res.send(errors.message);
		} else {
			ipAddress = ipAddresses[0];
			url = 'http://' + ipAddress + ':8082/';
			var parameters = { 
				width:req.query.width, 
				length:req.query.length
			};
			request({url:url, qs:parameters}, function (err, response, body) {
				if (err) {
					res.send(err);
				} else {
					res.send(url + ' returned: ' + body);
				}
			});
		}
	});
});

app.listen(8081);

