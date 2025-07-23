#!/bin/sh
set -e

echo "Waiting for PostgreSQL to be ready..."

until pg_isready -h "$POSTGRES_DBHOST" -U "$POSTGRES_USER"; do
  sleep 2
done

echo "PostgreSQL is ready. Applying migrations..."

for file in /database/*.sql; do
  echo "Applying migration: $file"
  PGPASSWORD=$POSTGRES_PASSWORD psql -h "$POSTGRES_DBHOST" -U "$POSTGRES_USER" -d "$POSTGRES_DB" -f "$file"
done

echo "Migrations complete."

npm start
