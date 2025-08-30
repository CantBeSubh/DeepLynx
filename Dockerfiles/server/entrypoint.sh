#!/bin/sh
set -e

echo "Waiting for PostgreSQL to be ready..."

export PGPASSWORD="$POSTGRES_PASSWORD"
until pg_isready -h "$POSTGRES_DB_HOST" -U "$POSTGRES_USER"; do
  sleep 2
done

echo "Checking for existence of the deeplynx database"
if psql -h "$POSTGRES_DB_HOST" -U "$POSTGRES_USER" -lqt | cut -d \| -f 1 | grep -qw deeplynx; then
  echo "Database 'deeplynx' already exists, skipping creation."
else
  echo "Database 'deeplynx' does not exist, creating now."
  psql -h "$POSTGRES_DB_HOST" -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<EOSQL
    CREATE DATABASE deeplynx;
EOSQL
fi

#
#echo "PostgreSQL is ready. Applying migrations..."
##for file in ../database/*.sql; do
#  echo "Applying migration: $file"
#  psql -h "$POSTGRES_DB_HOST" -U "$POSTGRES_USER" -d "$POSTGRES_DB" -f "$file"
#done
#echo "Migrations complete."

# Execute the dotnet application
dotnet deeplynx.api.dll --urls http://*:5000
