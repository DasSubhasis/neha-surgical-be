-- Create Consumptions table for tracking consumed items
CREATE TABLE Consumptions (
    consumption_id        SERIAL PRIMARY KEY,
    order_id              INT NOT NULL REFERENCES Orders(order_id) ON DELETE CASCADE,
    item_group_id         INT REFERENCES ItemGroups(item_group_id) ON DELETE SET NULL,
    item_group_name       VARCHAR(255),
    consumed_items        JSONB NOT NULL,
    created_by            VARCHAR(255) NOT NULL,
    created_at            TIMESTAMP NOT NULL DEFAULT NOW(),
    is_active             CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N')),
    updated_at            TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Create indexes for faster lookups
CREATE INDEX idx_consumptions_order_id ON Consumptions(order_id);
CREATE INDEX idx_consumptions_item_group_id ON Consumptions(item_group_id);
CREATE INDEX idx_consumptions_created_at ON Consumptions(created_at);
CREATE INDEX idx_consumptions_is_active ON Consumptions(is_active);

-- Add comments
COMMENT ON TABLE Consumptions IS 'Stores consumption records for items used in operations';
COMMENT ON COLUMN Consumptions.consumed_items IS 'JSON array of consumed item details with id, name, quantity, and type (Auto/Manual)';
