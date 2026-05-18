#!/usr/bin/env bash
set -e

primary_host="${POSTGRES_PRIMARY_HOST:-postgres}"
primary_port="${POSTGRES_PRIMARY_PORT:-5432}"
replication_user="${POSTGRES_REPLICATION_USER:-replicator}"

echo "Waiting for primary PostgreSQL at ${primary_host}:${primary_port}..."
until pg_isready -h "$primary_host" -p "$primary_port" -U "$POSTGRES_USER"; do
  sleep 2
done

if [ ! -s "$PGDATA/PG_VERSION" ]; then
  echo "Initializing replica data directory from primary..."
  rm -rf "$PGDATA"/*

  PGPASSWORD="$POSTGRES_REPLICATION_PASSWORD" pg_basebackup \
    -h "$primary_host" \
    -p "$primary_port" \
    -U "$replication_user" \
    -D "$PGDATA" \
    -P \
    -Xs \
    -R

  cp /etc/postgresql/replica/postgresql.conf "$PGDATA/postgresql.conf"
  chown -R postgres:postgres "$PGDATA"
fi

echo "Starting PostgreSQL replica..."
exec docker-entrypoint.sh postgres
