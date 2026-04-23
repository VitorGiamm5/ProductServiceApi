#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    -- Usuário de replicação
    CREATE USER repl_user WITH REPLICATION ENCRYPTED PASSWORD 'repl_XLR';

    -- Usuário somente leitura para replica 1
    CREATE USER replica1_user WITH ENCRYPTED PASSWORD 'replica1_XLR';
    GRANT CONNECT ON DATABASE dbgodelivery TO replica1_user;
    GRANT SELECT ON ALL TABLES IN SCHEMA public TO replica1_user;
    ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO replica1_user;

    -- Usuário somente leitura para replica 2
    CREATE USER replica2_user WITH ENCRYPTED PASSWORD 'replica2_XLR';
    GRANT CONNECT ON DATABASE dbgodelivery TO replica2_user;
    GRANT SELECT ON ALL TABLES IN SCHEMA public TO replica2_user;
    ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO replica2_user;
EOSQL