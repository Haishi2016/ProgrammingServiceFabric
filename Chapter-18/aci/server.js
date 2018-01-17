var express = require('express');
var app = express();
var bodyParser = require('body-parser');

app.use(bodyParser.urlencoded( {extended:true }));
app.use(bodyParser.json());

var port = process.env.PORT || 8080;

var router = express.Router();

var events = [];

router.get('/', function (req,res) {
  res.json(events);
});

router.post('/', function (req,res) {
  events.push(req.body);
  res.json({});
});

app.use('/api', router);

app.listen(port);
console.log('Server is listening at port ' + port);
