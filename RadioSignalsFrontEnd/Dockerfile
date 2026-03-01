# ── Build stage ──
FROM node:22-alpine AS build
WORKDIR /app

# Copy package files first (for layer caching)
COPY package.json package-lock.json ./
RUN npm ci

# Copy source and build
COPY . .

# VITE_API_BASE_URL is baked in at build time
ARG VITE_API_BASE_URL=http://localhost:5229/api
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

RUN npm run build

# ── Runtime stage ──
FROM nginx:stable-alpine AS runtime

# SPA fallback: unknown paths → index.html
COPY nginx.conf /etc/nginx/conf.d/default.conf

COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
