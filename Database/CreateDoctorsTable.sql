-- Create Doctors table for Neha Surgical API
-- Run this script in your PostgreSQL database

CREATE TABLE doctors (
    doctor_id   SERIAL PRIMARY KEY,
    doctor_name VARCHAR(100) NOT NULL,
    contact_no  VARCHAR(20) NOT NULL UNIQUE,
    email       VARCHAR(100),
    specialization VARCHAR(100),
    dob         DATE,
    doa         DATE,
    identifier  VARCHAR(100) UNIQUE,
    registration_number VARCHAR(100),
    location    VARCHAR(200),
    remarks     VARCHAR(250),
    is_active   CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N'))
);

-- Create indexes for better performance
CREATE INDEX idx_doctors_contact_no ON doctors(contact_no);
CREATE INDEX idx_doctors_identifier ON doctors(identifier) WHERE identifier IS NOT NULL;
CREATE INDEX idx_doctors_is_active ON doctors(is_active);
