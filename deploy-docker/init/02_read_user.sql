-- Creates the read-only user used by the API to query the replica
CREATE USER read_randandan WITH ENCRYPTED PASSWORD 'read_randandan_XLR';
GRANT CONNECT ON DATABASE dbproducts TO read_randandan;
GRANT USAGE ON SCHEMA public TO read_randandan;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO read_randandan;

-- Ensure future tables are also accessible
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO read_randandan;