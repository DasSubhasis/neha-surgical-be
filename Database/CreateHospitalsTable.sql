-- Create Hospitals table
CREATE TABLE Hospitals (
    hospital_id   SERIAL PRIMARY KEY,
    name          VARCHAR(150) NOT NULL UNIQUE,
    address       TEXT,
    contact_person VARCHAR(100),
    contact_no    VARCHAR(20),
    email         VARCHAR(100),
    is_active     CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N')),
    created_at    TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at    TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Insert sample data
INSERT INTO Hospitals (name, address, contact_person, contact_no, email, is_active)
VALUES 
    ('City General Hospital', '123 Main Street, New York, NY 10001', 'John Smith', '555-0101', 'admin@citygeneralhospital.com', 'Y'),
    ('Medical Center East', '456 Oak Avenue, Boston, MA 02101', 'Sarah Johnson', '555-0102', 'contact@medcentereast.com', 'Y'),
    ('Wellness Clinic', '789 Pine Road, Chicago, IL 60601', 'Michael Brown', '555-0103', 'info@wellnessclinic.com', 'Y');
