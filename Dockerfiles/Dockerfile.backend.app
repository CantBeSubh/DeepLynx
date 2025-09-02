# Stage 1: Build the C# backend
FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0-preview-alpine AS backend-build

# Define build arguments
ARG OKTA_CLIENT_ID
ARG OKTA_ISSUER
ARG NEXT_PUBLIC_API_URL
ARG NEXT_PUBLIC_REDIRECT_LINK
ARG OKTA_CLIENT_SECRET
ARG AUTH_SECRET
ARG NEXTAUTH_SECRET
ARG SERVICE_TOKEN
ARG BACKEND_BASE_URL
ARG NEXTAUTH_URL

# Set environment variables
ENV OKTA_CLIENT_ID=${OKTA_CLIENT_ID}
ENV OKTA_ISSUER=${OKTA_ISSUER}
ENV NEXT_PUBLIC_API_URL=${NEXT_PUBLIC_API_URL}
ENV NEXT_PUBLIC_REDIRECT_LINK=${NEXT_PUBLIC_REDIRECT_LINK}
ENV OKTA_CLIENT_SECRET=${OKTA_CLIENT_SECRET}
ENV AUTH_SECRET=${AUTH_SECRET}
ENV NEXTAUTH_SECRET=${NEXTAUTH_SECRET}
ENV SERVICE_TOKEN=${SERVICE_TOKEN}
ENV BACKEND_BASE_URL=${BACKEND_BASE_URL}
ENV NEXTAUTH_URL=${NEXTAUTH_URL}

# Print out the value of the environment variables
RUN echo "OKTA_CLIENT_ID=${OKTA_CLIENT_ID}" \
    && echo "OKTA_ISSUER=${OKTA_ISSUER}" \
    && echo "NEXT_PUBLIC_API_URL=${NEXT_PUBLIC_API_URL}" \
    && echo "NEXT_PUBLIC_REDIRECT_LINK=${NEXT_PUBLIC_REDIRECT_LINK}" \
    && echo "OKTA_CLIENT_SECRET=${OKTA_CLIENT_SECRET}" \
    && echo "AUTH_SECRET=${AUTH_SECRET}" \
    && echo "NEXTAUTH_SECRET=${NEXTAUTH_SECRET}" \
    && echo "BACKEND_BASE_URL=${BACKEND_BASE_URL}" \
    && echo "NEXTAUTH_URL=${NEXTAUTH_URL}"

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