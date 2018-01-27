FROM node:6
WORKDIR /app
ADD ./server.js /app
RUN npm install express
RUN npm install request
EXPOSE 8081
CMD ["node", "server.js"]
