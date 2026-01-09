-- Create AssistantOperations table for tracking check-in/check-out
CREATE TABLE AssistantOperations (
    operation_record_id   SERIAL PRIMARY KEY,
    order_id              INT NOT NULL REFERENCES Orders(order_id) ON DELETE CASCADE,
    assistant_id          INT NOT NULL REFERENCES SystemUsers(system_user_id) ON DELETE CASCADE,
    gps_latitude          DECIMAL(10, 8),
    gps_longitude         DECIMAL(11, 8),
    gps_location          TEXT,
    checkin_time          TIMESTAMP,
    checkout_time         TIMESTAMP,
    notes                 TEXT,
    is_active             CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N')),
    created_at            TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at            TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Create indexes for faster lookups
CREATE INDEX idx_assistant_operations_order_id ON AssistantOperations(order_id);
CREATE INDEX idx_assistant_operations_assistant_id ON AssistantOperations(assistant_id);
CREATE INDEX idx_assistant_operations_is_active ON AssistantOperations(is_active);
CREATE INDEX idx_assistant_operations_checkin_time ON AssistantOperations(checkin_time);

-- Add comments
COMMENT ON TABLE AssistantOperations IS 'Stores assistant operation tracking information including check-in/check-out times and GPS coordinates';
COMMENT ON COLUMN AssistantOperations.gps_latitude IS 'GPS latitude coordinate (-90 to 90)';
COMMENT ON COLUMN AssistantOperations.gps_longitude IS 'GPS longitude coordinate (-180 to 180)';
COMMENT ON COLUMN AssistantOperations.gps_location IS 'Full GPS location address';
COMMENT ON COLUMN AssistantOperations.checkin_time IS 'Time when assistant checked in at the operation location';
COMMENT ON COLUMN AssistantOperations.checkout_time IS 'Time when assistant checked out from the operation location';
