-- Create Categories table
CREATE TABLE Categories (
    category_id SERIAL PRIMARY KEY,
    name        VARCHAR(100) NOT NULL UNIQUE,
    is_active   CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N')),
    created_at  TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Insert sample data
INSERT INTO Categories (name, is_active)
VALUES 
    ('Surgical Instruments', 'Y'),
    ('Diagnostic Equipment', 'Y'),
    ('Patient Monitoring', 'Y'),
    ('Consumables', 'Y'),
    ('Implants', 'Y');
