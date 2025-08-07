# Stage 1: Build the C# backend
FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0-preview-alpine AS backend-build

# Define build arguments
ARG NEXT_PUBLIC_OKTA_CLIENT_ID
ARG NEXT_PUBLIC_OKTA_ISSUER
ARG NEXT_PUBLIC_API_URL
ARG NEXT_PUBLIC_REDIRECT_LINK
ARG NEXT_PUBLIC_OKTA_CLIENT_SECRET
ARG NEXT_PUBLIC_AUTH_SECRET

# Set environment variables
ENV NEXT_PUBLIC_OKTA_CLIENT_ID=${NEXT_PUBLIC_OKTA_CLIENT_ID}
ENV NEXT_PUBLIC_OKTA_ISSUER=${NEXT_PUBLIC_OKTA_ISSUER}
ENV NEXT_PUBLIC_API_URL=${NEXT_PUBLIC_API_URL}
ENV NEXT_PUBLIC_REDIRECT_LINK=${NEXT_PUBLIC_REDIRECT_LINK}
ENV NEXT_PUBLIC_OKTA_CLIENT_SECRET=${NEXT_PUBLIC_OKTA_CLIENT_SECRET}
ENV NEXT_PUBLIC_AUTH_SECRET=${NEXT_PUBLIC_AUTH_SECRET}

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



# Stage 4: Create the final image
FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0-preview-alpine AS final

# Add missing package
RUN apk --no-check-certificate add postgresql-client

# Copy the entrypoint script
COPY database/Dockerfiles/entrypoint.sh /usr/local/bin/
RUN chmod +x /usr/local/bin/entrypoint.sh


WORKDIR /app/backend

# Copy the published backend code
COPY --from=publish /app/publish .
COPY database /database
COPY deeplynx.api/moon.css /app/backend/moon.css

CMD [ "dotnet", "run" ]