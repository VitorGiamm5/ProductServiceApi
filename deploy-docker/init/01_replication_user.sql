-- Creates the replication user used by the replica to sync WAL from the primary
CREATE USER replicator WITH REPLICATION ENCRYPTED PASSWORD 'replicator_password';