-- Create HospitalContacts table
CREATE TABLE HospitalContacts (
    contact_id   SERIAL PRIMARY KEY,
    hospital_id  INTEGER NOT NULL REFERENCES Hospitals(hospital_id) ON DELETE CASCADE,
    name         VARCHAR(100),
    mobile       VARCHAR(20),
    email        VARCHAR(100),
    location     VARCHAR(100),
    remarks      TEXT,
    created_at   TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at   TIMESTAMP NOT NULL DEFAULT NOW()
);
