FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build -- --configuration production

FROM nginx:alpine
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html
RUN echo 'server { listen 80; root /usr/share/nginx/html; index index.html; location / { try_files $uri $uri/ /index.html; } location /api/ { proxy_pass https://comments-app-production-8f3d.up.railway.app/api/; } location /uploads/ { proxy_pass https://comments-app-production-8f3d.up.railway.app/uploads/; } }' > /etc/nginx/conf.d/default.conf
EXPOSE 80
