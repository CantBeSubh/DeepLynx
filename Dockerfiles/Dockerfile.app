# Stage 1: Build the frontend
FROM node:lts-alpine3.20 AS frontend-build

# Set working directory
WORKDIR /app

# Define build arguments
ARG NEXT_PUBLIC_OKTA_CLIENT_ID
ARG NEXT_PUBLIC_OKTA_ISSUER
ARG NEXT_PUBLIC_API_URL
ARG NEXT_PUBLIC_REDIRECT_LINK
ARG OKTA_CLIENT_SECRET
ARG AUTH_SECRET
ARG SERVICE_TOKEN
ARG BACKEND_BASE_URL

# Set environment variables
ENV NEXT_PUBLIC_OKTA_CLIENT_ID=${NEXT_PUBLIC_OKTA_CLIENT_ID}
ENV NEXT_PUBLIC_OKTA_ISSUER=${NEXT_PUBLIC_OKTA_ISSUER}
ENV NEXT_PUBLIC_API_URL=${NEXT_PUBLIC_API_URL}
ENV NEXT_PUBLIC_REDIRECT_LINK=${NEXT_PUBLIC_REDIRECT_LINK}
ENV OKTA_CLIENT_SECRET=${OKTA_CLIENT_SECRET}
ENV AUTH_SECRET=${AUTH_SECRET}
ENV SERVICE_TOKEN=${SERVICE_TOKEN}
ENV BACKEND_BASE_URL=${BACKEND_BASE_URL}

# Print out the value of the environment variables
RUN echo "NEXT_PUBLIC_OKTA_CLIENT_ID=${NEXT_PUBLIC_OKTA_CLIENT_ID}" \
    && echo "NEXT_PUBLIC_OKTA_ISSUER=${NEXT_PUBLIC_OKTA_ISSUER}" \
    && echo "NEXT_PUBLIC_API_URL=${NEXT_PUBLIC_API_URL}" \
    && echo "NEXT_PUBLIC_REDIRECT_LINK=${NEXT_PUBLIC_REDIRECT_LINK}" \
    && echo "OKTA_CLIENT_SECRET=${OKTA_CLIENT_SECRET}" \
    && echo "AUTH_SECRET=${AUTH_SECRET}" \
    && echo "BACKEND_BASE_URL=${BACKEND_BASE_URL}"

# Copy package.json and package-lock.json
COPY deeplynx.UI/package*.json ./
RUN npm install

# Copy the rest of the frontend source code
COPY deeplynx.UI/ ./

# Build the frontend
RUN npm run build

# Stage 2: Create the final image
FROM node:lts-alpine3.20 AS final

# Set working directory
WORKDIR /app/frontend

# Copy the built frontend code
COPY --from=frontend-build /app/next.config.ts ./
COPY --from=frontend-build /app/public ./public
COPY --from=frontend-build /app/.next ./.next
COPY --from=frontend-build /app/package.json ./package.json

# Install production dependencies
RUN npm install --production

# Set the command point to run the application
CMD ["npm", "start"]
