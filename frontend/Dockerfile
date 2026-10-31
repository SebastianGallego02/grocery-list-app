# Build context: frontend/  (docker compose sets it; manually: docker build -t grocerylist-web frontend)

FROM node:24-alpine AS build
WORKDIR /app

# Install dependencies first, using only the manifests, so the layer is cached until they change.
COPY package.json package-lock.json ./
RUN npm ci

COPY . .

# Vite inlines VITE_* variables at build time, so the API URL is a build argument, not a runtime one.
ARG VITE_API_BASE_URL=
ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}
RUN npm run build

# Unprivileged nginx: runs as a non-root user and listens on 8080.
FROM nginxinc/nginx-unprivileged:1.29-alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 8080
