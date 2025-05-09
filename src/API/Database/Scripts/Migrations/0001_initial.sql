--
-- PostgreSQL database dump
--

-- Dumped from database version 17.0
-- Dumped by pg_dump version 17.0

-- Started on 2024-12-18 14:16:34

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;


CREATE SCHEMA topology;
ALTER SCHEMA topology OWNER TO postgres;
COMMENT ON SCHEMA topology IS 'PostGIS Topology schema';

CREATE EXTENSION IF NOT EXISTS postgis WITH SCHEMA public;
COMMENT ON EXTENSION postgis IS 'PostGIS geometry and geography spatial types and functions';

CREATE EXTENSION IF NOT EXISTS postgis_topology WITH SCHEMA topology;
COMMENT ON EXTENSION postgis_topology IS 'PostGIS topology spatial types and functions';


CREATE FUNCTION public.update_food_places_search_vector() RETURNS trigger
    LANGUAGE plpgsql
    AS $$BEGIN
  NEW.search_vector = to_tsvector('english', NEW.name || ' ' || NEW.category || ' ' || COALESCE(NEW.description, ''));
  RETURN NEW;
END;$$;


ALTER FUNCTION public.update_food_places_search_vector() OWNER TO postgres;

CREATE FUNCTION public.create_geometry_object_from_latlong() RETURNS trigger
    LANGUAGE plpgsql
AS $$
BEGIN
	NEW.location = ST_SetSRID(ST_POINT(NEW.longitude, NEW.latitude), 4326);
	RETURN NEW;
END;
$$;

ALTER FUNCTION public.create_geometry_object_from_latlong() OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;


CREATE TABLE public.users (
    id integer primary key generated by default as identity,
    first_name text NOT NULL,
    surname text NOT NULL,
    phone_number text NOT NULL UNIQUE,
    password text NOT NULL,
    user_type text NOT NULL CHECK (user_type IN ('food_place', 'customer')),
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE public.drivers (
    id integer primary key generated by default as identity,
    first_name character varying(20) NOT NULL,
    surname character varying(20) NOT NULL,
    phone_number text NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    password text NOT NULL
);

CREATE TABLE public.deliveries (
    id integer primary key generated by default as identity,
    address_id integer NOT NULL,
    driver_id integer REFERENCES drivers(id) ON DELETE RESTRICT,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    is_delivered boolean NOT NULL,
    delivered_at timestamp with time zone
);

CREATE TABLE public.food_places (
    id integer primary key generated by default as identity,
    name character varying(100) NOT NULL,
    category character varying(100) NOT NULL,
    description text,
    search_vector tsvector,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    longitude numeric(9, 6),
    latitude numeric(8, 6),
    location public.geometry(Point,4326)
);

CREATE TABLE public.food_places_items (
    id integer primary key generated by default as identity,
    name character varying(20) NOT NULL,
    description text,
    food_place_id integer NOT NULL REFERENCES food_places(id) ON DELETE CASCADE,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    is_available boolean NOT NULL,
    price integer NOT NULL
);

CREATE TABLE public.orders (
    id integer primary key generated by default as identity,
    customer_id integer NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    total_price integer NOT NULL,
    is_cancelled boolean NOT NULL DEFAULT FALSE
);

CREATE TABLE public.order_items (
    id integer primary key generated by default as identity,
    order_id integer NOT NULL REFERENCES orders(id) ON DELETE RESTRICT,
    item_id integer NOT NULL REFERENCES food_places_items(id) ON DELETE RESTRICT,
    quantity integer NOT NULL,
    subtotal integer NOT NULL
);


CREATE INDEX idx_search_vector ON public.food_places USING gin (search_vector) WITH (fastupdate='true');


CREATE TRIGGER trg_update_search_vector BEFORE INSERT OR UPDATE OF name, category ON public.food_places FOR EACH ROW EXECUTE FUNCTION public.update_food_places_search_vector();

CREATE OR REPLACE TRIGGER trg_create_location_object
    BEFORE INSERT OR UPDATE OF longitude, latitude
    ON public.food_places
    FOR EACH ROW
    EXECUTE FUNCTION public.create_geometry_object_from_latlong();


-- Completed on 2024-12-18 14:16:34

--
-- PostgreSQL database dump complete
--

