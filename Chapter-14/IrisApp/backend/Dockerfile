FROM python:2.7-slim
WORKDIR /app
ADD . /app
RUN pip install flask
RUN pip install numpy
RUN pip install scipy
RUN pip install sklearn

EXPOSE 8082

CMD ["python", "app.py"]
