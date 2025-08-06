# Stage 1: Download dependencies
FROM node:lts-alpine3.20 AS dependencies
WORKDIR /app

# Copy package.json and package-lock.json
COPY deeplynx.UI/deeplynx-v3/package*.json ./
RUN npm install

# Stage 2: Build the frontend
FROM node:lts-alpine3.20 AS frontend-build
WORKDIR /app

# Copy dependencies
COPY --from=dependencies /app/node_modules ./node_modules

# Copy the rest of the frontend source code
COPY deeplynx.UI/deeplynx-v3/ ./

# Build the frontend
RUN npm run build

# Stage 3: Build the C# backend
FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0-preview-alpine AS backend-build
WORKDIR /source

# Copy the solution file and restore dependencies
COPY . ./Backend
RUN dotnet restore ./Backend/deeplynx.sln

# Build the backend
WORKDIR /source/Backend
RUN dotnet build -c Release -o /app/build

# Publish the backend
FROM backend-build AS publish
RUN dotnet publish deeplynx.sln -c Release -o /app/publish /p:UseAppHost=false

# Install tools needed for entrypoint.sh
RUN apk --no-check-certificate add postgresql-client

# Copy the entrypoint script
COPY database/Dockerfiles/entrypoint.sh /usr/local/bin/
RUN chmod +x /usr/local/bin/entrypoint.sh

# Stage 4: Create the final image
FROM node:lts-alpine3.20 AS final

WORKDIR /app/backend

# Copy the published backend code
COPY --from=publish /app/publish .

WORKDIR /app/frontend

COPY --from=frontend-build /app/next.config.ts ./
COPY --from=frontend-build /app/public ./public
COPY --from=frontend-build /app/.next ./.next
COPY --from=frontend-build /app/package.json ./package.json

RUN npm install --production

# Set environment variables
ENV NEXT_PUBLIC_OKTA_CLIENT_ID=$NEXT_PUBLIC_OKTA_CLIENT_ID
ENV NEXT_PUBLIC_OKTA_ISSUER=$NEXT_PUBLIC_OKTA_ISSUER
ENV NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL

# Set the command point to run the application
# Currently overriden by entrypoint.sh
CMD [ "npm", "start" ]