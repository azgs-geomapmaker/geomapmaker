--
-- PostgreSQL database dump
--

-- Dumped from database version 11.5
-- Dumped by pg_dump version 11.5

-- Started on 2021-04-23 17:02:17

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

DROP DATABASE geomapmaker;
--
-- TOC entry 2843 (class 1262 OID 63411)
-- Name: geomapmaker; Type: DATABASE; Schema: -; Owner: geomapmaker
--

CREATE DATABASE geomapmaker WITH TEMPLATE = template0 ENCODING = 'UTF8' LC_COLLATE = 'English_United States.1252' LC_CTYPE = 'English_United States.1252';


ALTER DATABASE geomapmaker OWNER TO geomapmaker;

\connect geomapmaker

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 4 (class 2615 OID 63438)
-- Name: geomapmaker; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA geomapmaker;


ALTER SCHEMA geomapmaker OWNER TO postgres;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- TOC entry 200 (class 1259 OID 63452)
-- Name: projects; Type: TABLE; Schema: geomapmaker; Owner: postgres
--

CREATE TABLE geomapmaker.projects (
    id integer NOT NULL,
    name text NOT NULL,
    notes text,
    connection_properties jsonb NOT NULL
);


ALTER TABLE geomapmaker.projects OWNER TO postgres;

--
-- TOC entry 199 (class 1259 OID 63450)
-- Name: projects_id_seq; Type: SEQUENCE; Schema: geomapmaker; Owner: postgres
--

CREATE SEQUENCE geomapmaker.projects_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE geomapmaker.projects_id_seq OWNER TO postgres;

--
-- TOC entry 2844 (class 0 OID 0)
-- Dependencies: 199
-- Name: projects_id_seq; Type: SEQUENCE OWNED BY; Schema: geomapmaker; Owner: postgres
--

ALTER SEQUENCE geomapmaker.projects_id_seq OWNED BY geomapmaker.projects.id;


--
-- TOC entry 202 (class 1259 OID 63463)
-- Name: user_project_links; Type: TABLE; Schema: geomapmaker; Owner: postgres
--

CREATE TABLE geomapmaker.user_project_links (
    id integer NOT NULL,
    user_id integer NOT NULL,
    project_id integer NOT NULL
);


ALTER TABLE geomapmaker.user_project_links OWNER TO postgres;

--
-- TOC entry 201 (class 1259 OID 63461)
-- Name: user_project_links_id_seq; Type: SEQUENCE; Schema: geomapmaker; Owner: postgres
--

CREATE SEQUENCE geomapmaker.user_project_links_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE geomapmaker.user_project_links_id_seq OWNER TO postgres;

--
-- TOC entry 2845 (class 0 OID 0)
-- Dependencies: 201
-- Name: user_project_links_id_seq; Type: SEQUENCE OWNED BY; Schema: geomapmaker; Owner: postgres
--

ALTER SEQUENCE geomapmaker.user_project_links_id_seq OWNED BY geomapmaker.user_project_links.id;


--
-- TOC entry 198 (class 1259 OID 63441)
-- Name: users; Type: TABLE; Schema: geomapmaker; Owner: postgres
--

CREATE TABLE geomapmaker.users (
    id integer NOT NULL,
    name text NOT NULL,
    notes text
);


ALTER TABLE geomapmaker.users OWNER TO postgres;

--
-- TOC entry 197 (class 1259 OID 63439)
-- Name: users_id_seq; Type: SEQUENCE; Schema: geomapmaker; Owner: postgres
--

CREATE SEQUENCE geomapmaker.users_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE geomapmaker.users_id_seq OWNER TO postgres;

--
-- TOC entry 2846 (class 0 OID 0)
-- Dependencies: 197
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: geomapmaker; Owner: postgres
--

ALTER SEQUENCE geomapmaker.users_id_seq OWNED BY geomapmaker.users.id;


--
-- TOC entry 2701 (class 2604 OID 63455)
-- Name: projects id; Type: DEFAULT; Schema: geomapmaker; Owner: postgres
--

ALTER TABLE ONLY geomapmaker.projects ALTER COLUMN id SET DEFAULT nextval('geomapmaker.projects_id_seq'::regclass);


--
-- TOC entry 2702 (class 2604 OID 63466)
-- Name: user_project_links id; Type: DEFAULT; Schema: geomapmaker; Owner: postgres
--

ALTER TABLE ONLY geomapmaker.user_project_links ALTER COLUMN id SET DEFAULT nextval('geomapmaker.user_project_links_id_seq'::regclass);


--
-- TOC entry 2700 (class 2604 OID 63444)
-- Name: users id; Type: DEFAULT; Schema: geomapmaker; Owner: postgres
--

ALTER TABLE ONLY geomapmaker.users ALTER COLUMN id SET DEFAULT nextval('geomapmaker.users_id_seq'::regclass);


--
-- TOC entry 2835 (class 0 OID 63452)
-- Dependencies: 200
-- Data for Name: projects; Type: TABLE DATA; Schema: geomapmaker; Owner: postgres
--

INSERT INTO geomapmaker.projects VALUES (1, 'Project 01', 'This are notes for Project 01', '{"user": "sde", "database": "gems01", "instance": "127.0.0.1", "password": "password"}');
INSERT INTO geomapmaker.projects VALUES (2, 'Project 02', 'This are notes for Project 02', '{"user": "sde", "database": "gems02", "instance": "127.0.0.1", "password": "password"}');


--
-- TOC entry 2837 (class 0 OID 63463)
-- Dependencies: 202
-- Data for Name: user_project_links; Type: TABLE DATA; Schema: geomapmaker; Owner: postgres
--

INSERT INTO geomapmaker.user_project_links VALUES (1, 1, 1);
INSERT INTO geomapmaker.user_project_links VALUES (2, 2, 2);
INSERT INTO geomapmaker.user_project_links VALUES (3, 1, 2);


--
-- TOC entry 2833 (class 0 OID 63441)
-- Dependencies: 198
-- Data for Name: users; Type: TABLE DATA; Schema: geomapmaker; Owner: postgres
--

INSERT INTO geomapmaker.users VALUES (1, 'douglas', 'Doug''s notes');
INSERT INTO geomapmaker.users VALUES (2, 'wilson', 'Wilson''s notes');


--
-- TOC entry 2847 (class 0 OID 0)
-- Dependencies: 199
-- Name: projects_id_seq; Type: SEQUENCE SET; Schema: geomapmaker; Owner: postgres
--

SELECT pg_catalog.setval('geomapmaker.projects_id_seq', 2, true);


--
-- TOC entry 2848 (class 0 OID 0)
-- Dependencies: 201
-- Name: user_project_links_id_seq; Type: SEQUENCE SET; Schema: geomapmaker; Owner: postgres
--

SELECT pg_catalog.setval('geomapmaker.user_project_links_id_seq', 3, true);


--
-- TOC entry 2849 (class 0 OID 0)
-- Dependencies: 197
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: geomapmaker; Owner: postgres
--

SELECT pg_catalog.setval('geomapmaker.users_id_seq', 2, true);


--
-- TOC entry 2706 (class 2606 OID 63460)
-- Name: projects projects_id_key; Type: CONSTRAINT; Schema: geomapmaker; Owner: postgres
--

ALTER TABLE ONLY geomapmaker.projects
    ADD CONSTRAINT projects_id_key UNIQUE (id);


--
-- TOC entry 2708 (class 2606 OID 63468)
-- Name: user_project_links user_project_links_id_key; Type: CONSTRAINT; Schema: geomapmaker; Owner: postgres
--

ALTER TABLE ONLY geomapmaker.user_project_links
    ADD CONSTRAINT user_project_links_id_key UNIQUE (id);


--
-- TOC entry 2704 (class 2606 OID 63449)
-- Name: users users_id_key; Type: CONSTRAINT; Schema: geomapmaker; Owner: postgres
--

ALTER TABLE ONLY geomapmaker.users
    ADD CONSTRAINT users_id_key UNIQUE (id);


--
-- TOC entry 2710 (class 2606 OID 63474)
-- Name: user_project_links user_project_links_project_id_fkey; Type: FK CONSTRAINT; Schema: geomapmaker; Owner: postgres
--

ALTER TABLE ONLY geomapmaker.user_project_links
    ADD CONSTRAINT user_project_links_project_id_fkey FOREIGN KEY (project_id) REFERENCES geomapmaker.projects(id);


--
-- TOC entry 2709 (class 2606 OID 63469)
-- Name: user_project_links user_project_links_user_id_fkey; Type: FK CONSTRAINT; Schema: geomapmaker; Owner: postgres
--

ALTER TABLE ONLY geomapmaker.user_project_links
    ADD CONSTRAINT user_project_links_user_id_fkey FOREIGN KEY (user_id) REFERENCES geomapmaker.users(id);


-- Completed on 2021-04-23 17:02:17

--
-- PostgreSQL database dump complete
--

