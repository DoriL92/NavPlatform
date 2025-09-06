FROM node:20-alpine
WORKDIR /usr/src/app

COPY src/FE-NAV/package*.json ./
RUN npm ci

COPY src/FE-NAV/ .

EXPOSE 4200
# use the docker configuration we added
CMD ["npm", "start", "--", "--host", "0.0.0.0", "--port", "4200", "--configuration", "docker"]
