BEGIN;
    
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

CREATE TABLE IF NOT EXISTS templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    file_name TEXT NOT NULL,
    mime_type TEXT NOT NULL,
    binary_file BYTEA NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS unique_file_name ON templates (file_name);

COMMIT;
