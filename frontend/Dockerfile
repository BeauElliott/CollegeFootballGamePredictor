# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Build the application
RUN npm run build

# Production stage
FROM node:20-alpine
WORKDIR /app

# Install serve to run the built application
RUN npm install -g serve

# Copy built files from build stage
COPY --from=build /app/build ./build

EXPOSE 3000

# Serve the static files
CMD ["serve", "-s", "build", "-l", "3000"]
