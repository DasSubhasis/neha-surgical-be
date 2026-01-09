-- Create MaterialDeliveries table
CREATE TABLE MaterialDeliveries (
    delivery_id         SERIAL PRIMARY KEY,
    order_id            INTEGER NOT NULL,
    delivery_date       DATE NOT NULL,
    delivered_by        VARCHAR(100),
    delivered_by_user_id INTEGER,
    remarks             TEXT,
    delivery_status     VARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (delivery_status IN ('Pending', 'Assigned', 'Delivered')),
    created_by          VARCHAR(100) NOT NULL,
    updated_by          VARCHAR(100),
    is_active           CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N')),
    created_at          TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMP NOT NULL DEFAULT NOW(),
    FOREIGN KEY (order_id) REFERENCES Orders(order_id) ON DELETE CASCADE,
    FOREIGN KEY (delivered_by_user_id) REFERENCES SystemUsers(system_user_id)
);

-- Create indexes for better performance
CREATE INDEX idx_material_deliveries_order_id ON MaterialDeliveries(order_id);
CREATE INDEX idx_material_deliveries_delivery_status ON MaterialDeliveries(delivery_status);
CREATE INDEX idx_material_deliveries_delivery_date ON MaterialDeliveries(delivery_date);
CREATE INDEX idx_material_deliveries_delivered_by_user_id ON MaterialDeliveries(delivered_by_user_id);

-- Insert sample data (using user IDs from SystemUsers table)
-- Note: Make sure these user IDs exist in SystemUsers table before running
INSERT INTO MaterialDeliveries (order_id, delivery_date, delivered_by_user_id, remarks, delivery_status, created_by)
VALUES 
    (1, '2024-12-23', 1, 'Materials delivered successfully', 'Delivered', 'Admin'),
    (2, '2024-12-24', 2, 'Partial delivery completed', 'Assigned', 'Admin'),
    (3, '2024-12-25', 1, 'Full delivery completed on time', 'Delivered', 'Admin');
