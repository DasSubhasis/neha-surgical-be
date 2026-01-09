-- Create Brands table
CREATE TABLE Brands (
    brand_id   SERIAL PRIMARY KEY,
    name       VARCHAR(100) NOT NULL UNIQUE,
    is_active  CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N')),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Insert sample data
INSERT INTO Brands (name, is_active)
VALUES 
    ('Johnson & Johnson', 'Y'),
    ('Medtronic', 'Y'),
    ('Abbott', 'Y'),
    ('Stryker', 'Y'),
    ('Becton Dickinson', 'Y');
