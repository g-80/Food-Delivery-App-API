-- Migration: Drop driver real-time status and location tables
-- These are now handled by Redis for better performance

BEGIN;

-- Drop the tables (CASCADE will handle foreign key constraints)
DROP TABLE IF EXISTS drivers_locations CASCADE;
DROP TABLE IF EXISTS drivers_statuses CASCADE;

-- Note: driver_location_history table remains for historical tracking

COMMIT;
