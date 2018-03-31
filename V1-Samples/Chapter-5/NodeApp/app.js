var express = require('express');
var app = express();
var os = require('os');
app.get('/', function (req, res) {
    res.send('Hello World from ' + os.hostname() + '!');
});
var server = app.listen(80);