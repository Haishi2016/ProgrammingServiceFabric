'use strict';

const express = require('express');

const PORT = 8180;

const app = express();
app.get('/', function (req, res) {
  res.send('Hello world from Node.JS on Service Fabric \n');
});

app.listen(PORT);
console.log('Running on http://localhost:' + PORT);