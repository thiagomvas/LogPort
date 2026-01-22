CREATE OR REPLACE FUNCTION drop_old_log_partitions(
    cutoff_date date
)
RETURNS INT
LANGUAGE plpgsql
AS $$
DECLARE
r RECORD;
    dropped INT := 0;
    partition_date date;
BEGIN
FOR r IN
SELECT c.relname AS partition_name
FROM pg_class c
         JOIN pg_inherits i ON i.inhrelid = c.oid
         JOIN pg_class p ON p.oid = i.inhparent
WHERE p.relname = 'logs'
  AND c.relname LIKE 'logs\_%\_%\_%\_1d' ESCAPE '\'
    LOOP
        /*
          Extract YYYY_MM_DD from logs_YYYY_MM_DD_1d
        */
        partition_date :=
            to_date(
                substring(r.partition_name FROM 'logs_(\d{4}_\d{2}_\d{2})_1d'),
                'YYYY_MM_DD'
            );

IF partition_date + INTERVAL '1 day' <= cutoff_date THEN
            EXECUTE format('DROP TABLE IF EXISTS %I', r.partition_name);
            dropped := dropped + 1;
END IF;
END LOOP;

RETURN dropped;
END;
$$;
