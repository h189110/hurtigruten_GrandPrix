FROM node:22-alpine AS build
WORKDIR /src
COPY frontend/grand-prix-web/package.json frontend/grand-prix-web/package-lock.json ./
RUN npm ci
COPY frontend/grand-prix-web/ ./
ARG VITE_API_BASE_URL=http://localhost:5080
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm run build

FROM nginx:1.27-alpine AS final
COPY --from=build /src/dist /usr/share/nginx/html
EXPOSE 80
