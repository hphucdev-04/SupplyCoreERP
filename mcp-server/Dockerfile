# Stage 1: Build stage
FROM node:20-alpine AS builder

WORKDIR /app

# Copy package configuration files
COPY mcp-server/package*.json ./

# Install ALL dependencies (including devDependencies for TypeScript compilation)
RUN npm install

# Copy source code
COPY mcp-server/ .

# Compile TypeScript to JavaScript
RUN npm run build

# Stage 2: Runtime stage
FROM node:20-alpine AS runner

WORKDIR /app

ENV NODE_ENV=production
ENV PORT=3000

# Copy package config files
COPY mcp-server/package*.json ./

# Install only production dependencies
RUN npm install --only=production

# Copy compiled JavaScript output from the builder stage
COPY --from=builder /app/dist ./dist
COPY --from=builder /app/resources ./resources
COPY --from=builder /app/query_blacklist.json ./query_blacklist.json

# Expose port
EXPOSE 3000

# Start the MCP Server
CMD ["npm", "start"]