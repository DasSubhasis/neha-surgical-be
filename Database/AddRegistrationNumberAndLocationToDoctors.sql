-- Add registration_number and location columns to Doctors table
-- Run this script in your PostgreSQL database

ALTER TABLE doctors 
ADD COLUMN registration_number VARCHAR(100),
ADD COLUMN location VARCHAR(200);

-- Create index for registration_number for better performance
CREATE INDEX idx_doctors_registration_number ON doctors(registration_number) WHERE registration_number IS NOT NULL;

-- Add comments for documentation
COMMENT ON COLUMN doctors.registration_number IS 'Medical registration number of the doctor';
COMMENT ON COLUMN doctors.location IS 'Primary location/address of the doctor';
