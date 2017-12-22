FROM golang

ADD . /go/src/server
RUN apt-get update
RUN apt-get -y install dnsutils
RUN go get github.com/lib/pq
RUN go install server

WORKDIR /go/src/server
CMD ["go", "run", "server.go"]

EXPOSE 8084
