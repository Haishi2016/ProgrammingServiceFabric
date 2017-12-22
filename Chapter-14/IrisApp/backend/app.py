import numpy as np
from sklearn.datasets import load_iris
from sklearn.linear_model import Perceptron
from flask import Flask, request

app = Flask(__name__)

iris = load_iris()
X = iris.data[:, (2,3)] 
y = (iris.target == 0).astype(np.int)
per_clf = Perceptron(random_state=42)
per_clf.fit(X,y)

@app.route('/')
def predict():
	return str(per_clf.predict([[float(request.args.get('length')), float(request.args.get('width'))]])[0])

app.run(host='0.0.0.0', port=8082)
