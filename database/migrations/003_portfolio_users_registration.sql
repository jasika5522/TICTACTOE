-- 003_portfolio_users_registration.sql
-- Portfolio Service: users table and register_user function
-- Target database: portfolio
--
-- Example:
--   psql -U postgres -d portfolio -f database/migrations/003_portfolio_users_registration.sql

BEGIN;

CREATE TABLE IF NOT EXISTS public.users (
    id BIGSERIAL PRIMARY KEY,
    full_name VARCHAR(150) NOT NULL,
    email VARCHAR(255) NOT NULL,
    mobile_number VARCHAR(15) NOT NULL,
    password_hash TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    updated_at TIMESTAMP NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT uq_users_email UNIQUE (email)
);

CREATE INDEX IF NOT EXISTS ix_users_email ON public.users (email);
CREATE INDEX IF NOT EXISTS ix_users_is_active ON public.users (is_active);

DROP FUNCTION IF EXISTS public.email_exists(text);
DROP FUNCTION IF EXISTS public.register_user(text, text, text, text);

CREATE OR REPLACE FUNCTION public.email_exists(p_email text)
RETURNS boolean
LANGUAGE plpgsql
STABLE
AS $function$
BEGIN
    RETURN EXISTS (
        SELECT 1
        FROM public.users u
        WHERE u.email = lower(trim(p_email))
    );
END;
$function$;

CREATE OR REPLACE FUNCTION public.register_user(
    p_full_name text,
    p_email text,
    p_mobile_number text,
    p_password_hash text
)
RETURNS TABLE(
    success boolean,
    message text,
    user_id bigint
)
LANGUAGE plpgsql
AS $function$
DECLARE
    v_email text;
    v_user_id bigint;
BEGIN
    v_email := lower(trim(p_email));

    IF public.email_exists(p_email) THEN
        RETURN QUERY SELECT false, 'Email is already registered.'::text, NULL::bigint;
        RETURN;
    END IF;

    INSERT INTO public.users (full_name, email, mobile_number, password_hash)
    VALUES (trim(p_full_name), v_email, trim(p_mobile_number), p_password_hash)
    RETURNING id INTO v_user_id;

    RETURN QUERY SELECT true, 'User registered successfully.'::text, v_user_id;
END;
$function$;

COMMIT;
