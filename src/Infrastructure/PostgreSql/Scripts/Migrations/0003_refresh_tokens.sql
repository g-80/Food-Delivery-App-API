CREATE TABLE refresh_tokens (
    user_id integer primary key REFERENCES users(id) ON DELETE CASCADE,
    token text NOT NULL,
    expires_at timestamp with time zone NOT NULL
);