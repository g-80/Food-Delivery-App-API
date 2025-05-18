CREATE OR REPLACE FUNCTION set_updated_at_now()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = CURRENT_TIMESTAMP;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DO $$
DECLARE
    r RECORD;
BEGIN
    FOR r IN
        SELECT table_schema, table_name
        FROM information_schema.columns
        WHERE column_name = 'updated_at'
        AND table_schema NOT IN ('pg_catalog', 'information_schema')
        AND table_name NOT IN ('carts')
    LOOP
        EXECUTE format('
            DROP TRIGGER IF EXISTS set_updated_at_trigger ON %I.%I;
            CREATE TRIGGER set_updated_at_trigger
            BEFORE INSERT OR UPDATE
            ON %I.%I
            FOR EACH ROW
            EXECUTE FUNCTION set_updated_at_now();
        ', r.table_schema, r.table_name, r.table_schema, r.table_name);
    END LOOP;
END $$;