FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build -- --configuration production

FROM nginx:alpine
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html
RUN echo 'server { listen 80; root /usr/share/nginx/html; index index.html; location / { try_files $uri $uri/ /index.html; } location /api/ { proxy_pass http://comments-app.railway.internal:8080/api/; } location /uploads/ { proxy_pass http://comments-app.railway.internal:8080/uploads/; } }' > /etc/nginx/conf.d/default.conf
EXPOSE 80