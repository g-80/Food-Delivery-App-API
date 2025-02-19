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

--
-- TOC entry 8 (class 2615 OID 17553)
-- Name: topology; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA topology;


ALTER SCHEMA topology OWNER TO postgres;

--
-- TOC entry 5933 (class 0 OID 0)
-- Dependencies: 8
-- Name: SCHEMA topology; Type: COMMENT; Schema: -; Owner: postgres
--

COMMENT ON SCHEMA topology IS 'PostGIS Topology schema';


--
-- TOC entry 2 (class 3079 OID 16481)
-- Name: postgis; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS postgis WITH SCHEMA public;


--
-- TOC entry 5934 (class 0 OID 0)
-- Dependencies: 2
-- Name: EXTENSION postgis; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION postgis IS 'PostGIS geometry and geography spatial types and functions';


--
-- TOC entry 3 (class 3079 OID 17554)
-- Name: postgis_topology; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS postgis_topology WITH SCHEMA topology;


--
-- TOC entry 5935 (class 0 OID 0)
-- Dependencies: 3
-- Name: EXTENSION postgis_topology; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION postgis_topology IS 'PostGIS topology spatial types and functions';


--
-- TOC entry 1032 (class 1255 OID 17729)
-- Name: update_food_places_search_vector(); Type: FUNCTION; Schema: public; Owner: postgres
--

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

--
-- TOC entry 237 (class 1259 OID 16463)
-- Name: deliveries; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.deliveries (
    id integer NOT NULL,
    address_id integer NOT NULL,
    driver_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    is_delivered boolean NOT NULL,
    delivered_at timestamp with time zone
);


ALTER TABLE public.deliveries OWNER TO postgres;

--
-- TOC entry 236 (class 1259 OID 16462)
-- Name: deliveries_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.deliveries ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.deliveries_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 235 (class 1259 OID 16455)
-- Name: drivers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.drivers (
    id integer NOT NULL,
    first_name character varying(20) NOT NULL,
    surname character varying(20) NOT NULL,
    phone_number text NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    password text NOT NULL,
    country_code character varying(3) NOT NULL
);


ALTER TABLE public.drivers OWNER TO postgres;

--
-- TOC entry 234 (class 1259 OID 16454)
-- Name: drivers_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.drivers ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.drivers_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 225 (class 1259 OID 16390)
-- Name: food_places; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.food_places (
    id integer NOT NULL,
    name character varying(100) NOT NULL,
    category character varying(100) NOT NULL,
    description text,
    search_vector tsvector,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    longitude numeric(9, 6),
    latitude numeric(8, 6),
    location public.geometry(Point,4326)
);


ALTER TABLE public.food_places OWNER TO postgres;

--
-- TOC entry 224 (class 1259 OID 16389)
-- Name: food_places_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.food_places ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.food_places_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 231 (class 1259 OID 16414)
-- Name: food_places_items; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.food_places_items (
    id integer NOT NULL,
    name character varying(20) NOT NULL,
    description text,
    food_place_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    is_available boolean NOT NULL,
    price integer NOT NULL
);


ALTER TABLE public.food_places_items OWNER TO postgres;

--
-- TOC entry 230 (class 1259 OID 16413)
-- Name: food_places_items_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.food_places_items ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.food_places_items_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 233 (class 1259 OID 16429)
-- Name: order_items; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.order_items (
    id integer NOT NULL,
    order_id integer NOT NULL,
    item_id integer NOT NULL,
    quantity integer NOT NULL,
    total_price integer NOT NULL
);


ALTER TABLE public.order_items OWNER TO postgres;

--
-- TOC entry 232 (class 1259 OID 16428)
-- Name: order_items_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.order_items ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.order_items_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 227 (class 1259 OID 16398)
-- Name: orders; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.orders (
    id integer NOT NULL,
    customer_id integer NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    total_price integer NOT NULL,
    is_cancelled boolean NOT NULL DEFAULT FALSE
);


ALTER TABLE public.orders OWNER TO postgres;

--
-- TOC entry 226 (class 1259 OID 16397)
-- Name: orders_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.orders ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.orders_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 229 (class 1259 OID 16405)
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    id integer NOT NULL,
    first_name text NOT NULL,
    surname text NOT NULL,
    phone_number text NOT NULL,
    password text NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    country_code character varying(3) NOT NULL
);


ALTER TABLE public.users OWNER TO postgres;

--
-- TOC entry 228 (class 1259 OID 16404)
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.users ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.users_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 5747 (class 2606 OID 16468)
-- Name: deliveries deliveries_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.deliveries
    ADD CONSTRAINT deliveries_pkey PRIMARY KEY (id);


--
-- TOC entry 5731 (class 2606 OID 16477)
-- Name: drivers drivers_country_code_check; Type: CHECK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE public.drivers
    ADD CONSTRAINT drivers_country_code_check CHECK ((length((country_code)::text) >= 1)) NOT VALID;


--
-- TOC entry 5745 (class 2606 OID 16461)
-- Name: drivers drivers_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.drivers
    ADD CONSTRAINT drivers_pkey PRIMARY KEY (id);


--
-- TOC entry 5741 (class 2606 OID 16421)
-- Name: food_places_items food_places_items_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.food_places_items
    ADD CONSTRAINT food_places_items_pkey PRIMARY KEY (id);


--
-- TOC entry 5734 (class 2606 OID 16396)
-- Name: food_places food_places_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.food_places
    ADD CONSTRAINT food_places_pkey PRIMARY KEY (id);


--
-- TOC entry 5743 (class 2606 OID 16433)
-- Name: order_items order_items_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.order_items
    ADD CONSTRAINT order_items_pkey PRIMARY KEY (id);


--
-- TOC entry 5737 (class 2606 OID 16403)
-- Name: orders orders_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT orders_pkey PRIMARY KEY (id);


--
-- TOC entry 5730 (class 2606 OID 16476)
-- Name: users users_country_code_check; Type: CHECK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE public.users
    ADD CONSTRAINT users_country_code_check CHECK ((length((country_code)::text) >= 1)) NOT VALID;


--
-- TOC entry 5739 (class 2606 OID 16412)
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- TOC entry 5735 (class 1259 OID 17734)
-- Name: idx_search_vector; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_search_vector ON public.food_places USING gin (search_vector) WITH (fastupdate='true');


--
-- TOC entry 5763 (class 2620 OID 17730)
-- Name: food_places trg_update_search_vector; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trg_update_search_vector BEFORE INSERT OR UPDATE OF name, category ON public.food_places FOR EACH ROW EXECUTE FUNCTION public.update_food_places_search_vector();

CREATE OR REPLACE TRIGGER trg_create_location_object
    BEFORE INSERT OR UPDATE OF longitude, latitude
    ON public.food_places
    FOR EACH ROW
    EXECUTE FUNCTION public.create_geometry_object_from_latlong();
--
-- TOC entry 5762 (class 2606 OID 16469)
-- Name: deliveries deliveries_driver_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.deliveries
    ADD CONSTRAINT deliveries_driver_id_fkey FOREIGN KEY (driver_id) REFERENCES public.drivers(id) NOT VALID;


--
-- TOC entry 5759 (class 2606 OID 16444)
-- Name: food_places_items food_places_items_food_place_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.food_places_items
    ADD CONSTRAINT food_places_items_food_place_id_fkey FOREIGN KEY (food_place_id) REFERENCES public.food_places(id) NOT VALID;


--
-- TOC entry 5760 (class 2606 OID 16439)
-- Name: order_items order_items_item_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.order_items
    ADD CONSTRAINT order_items_item_id_fkey FOREIGN KEY (item_id) REFERENCES public.food_places_items(id);


--
-- TOC entry 5761 (class 2606 OID 16434)
-- Name: order_items order_items_order_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.order_items
    ADD CONSTRAINT order_items_order_id_fkey FOREIGN KEY (order_id) REFERENCES public.orders(id);


--
-- TOC entry 5758 (class 2606 OID 16449)
-- Name: orders orders_customer_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

-- ALTER TABLE ONLY public.orders
--     ADD CONSTRAINT orders_customer_id_fkey FOREIGN KEY (customer_id) REFERENCES public.users(id) NOT VALID;


-- Completed on 2024-12-18 14:16:34

--
-- PostgreSQL database dump complete
--

