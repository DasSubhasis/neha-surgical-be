-- Create ItemGroups table
CREATE TABLE ItemGroups (
    item_group_id SERIAL PRIMARY KEY,
    name          VARCHAR(50) NOT NULL UNIQUE,
    description   VARCHAR(250),
    is_active     CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N')),
    created_at    TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at    TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Insert sample data
INSERT INTO ItemGroups (name, description, is_active)
VALUES 
    ('Surgical Instruments', 'Tools and instruments used in surgical procedures', 'Y'),
    ('Medical Supplies', 'General medical supplies and consumables', 'Y'),
    ('Diagnostic Equipment', 'Equipment used for medical diagnosis', 'Y'),
    ('Patient Care', 'Items for patient care and comfort', 'Y'),
    ('Laboratory Supplies', 'Supplies and equipment for laboratory use', 'Y');
