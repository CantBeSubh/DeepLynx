FROM node:20-alpine AS frontend-build
WORKDIR /app

# Copy package.json and package-lock.json
COPY UI/deeplynx-v3/package*.json ./

# Install dependencies
RUN npm install

# Copy the rest of the frontend source code
COPY UI/deeplynx-v3/ ./

# Build the frontend
RUN npm run build

# Stage 2: Build the C# backend
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine3.20 AS backend-build
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

# Stage 3: Create the final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine3.20 AS final
WORKDIR /app

# Copy the published backend code
COPY --from=publish /app/publish .

# Copy the built frontend code
COPY --from=frontend-build /app/.next /app/wwwroot

# Set the entry point to run the backend application
ENTRYPOINT ["dotnet", "deeplynx.dll"]