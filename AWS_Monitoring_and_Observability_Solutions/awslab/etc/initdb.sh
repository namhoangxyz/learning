#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL

	
    CREATE DATABASE "VoteDB";
	\c VoteDB
	
    CREATE TABLE IF NOT EXISTS "Counts" (
        "ID" INT GENERATED BY DEFAULT AS IDENTITY,
        "Candidate" VARCHAR(20) NOT NULL,	  
        "Count" INT	  
    );

    INSERT INTO "Counts" 
    ("Candidate", "Count")
    VALUES
        ('Spain', 0);
    

    INSERT INTO "Counts" 
        ("Candidate", "Count")
    VALUES
        ('England', 0);

        INSERT INTO "Counts" 
        ("Candidate", "Count")
    VALUES
        ('Portugal', 0);

        INSERT INTO "Counts" 
        ("Candidate", "Count")
    VALUES
        ('France', 0);

        INSERT INTO "Counts" 
        ("Candidate", "Count")
    VALUES
        ('Germany', 0);
    
    INSERT INTO "Counts" 
        ("Candidate", "Count")
    VALUES
        ('Netherlands', 0);

        INSERT INTO "Counts" 
        ("Candidate", "Count")
    VALUES
        ('Denmark', 0);

EOSQL