-- Create Sizes table
CREATE TABLE Sizes (
    size_id    SERIAL PRIMARY KEY,
    name       VARCHAR(100) NOT NULL UNIQUE,
    is_active  CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N')),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Insert sample data
INSERT INTO Sizes (name, is_active)
VALUES 
    ('Small', 'Y'),
    ('Medium', 'Y'),
    ('Large', 'Y'),
    ('Extra Large', 'Y'),
    ('XXL', 'Y');
