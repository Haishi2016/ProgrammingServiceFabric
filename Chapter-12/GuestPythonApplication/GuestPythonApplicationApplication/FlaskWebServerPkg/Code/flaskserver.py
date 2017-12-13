from flask import Flask

app = Flask("myweb")

@app.route("/")
def hello():
  return "Hello from Flask!"

app.run(host='0.0.0.0', port=8183, debug = False)
