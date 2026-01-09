-- Create Specifications table
CREATE TABLE Specifications (
    specification_id SERIAL PRIMARY KEY,
    name             VARCHAR(100) NOT NULL UNIQUE,
    is_active        CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N')),
    created_at       TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Insert sample data
INSERT INTO Specifications (name, is_active)
VALUES 
    ('Stainless Steel', 'Y'),
    ('Titanium', 'Y'),
    ('Disposable', 'Y'),
    ('Reusable', 'Y'),
    ('Sterile', 'Y');
