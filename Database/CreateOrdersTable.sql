-- Create Orders table
CREATE TABLE Orders (
    order_id            SERIAL PRIMARY KEY,
    order_no            VARCHAR(50) NOT NULL UNIQUE,
    order_date          DATE NOT NULL,
    doctor_id           INTEGER NOT NULL,
    hospital_id         INTEGER NOT NULL,
    operation_date      DATE NOT NULL,
    operation_time      TIME NOT NULL,
    material_send_date  DATE NOT NULL,
    remarks             TEXT,
    created_by          VARCHAR(100) NOT NULL,
    status              VARCHAR(50) NOT NULL DEFAULT 'Pending' CHECK (status IN ('Pending', 'Assigned', 'In-operation', 'Completed', 'Canceled')),
    is_delivered        VARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (is_delivered IN ('Pending', 'Assigned', 'Delivered')),
    is_active           CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N')),
    created_at          TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMP NOT NULL DEFAULT NOW(),
    FOREIGN KEY (doctor_id) REFERENCES Doctors(doctor_id),
    FOREIGN KEY (hospital_id) REFERENCES Hospitals(hospital_id)
);

-- Create OrderItems table (for items associated with an order)
CREATE TABLE OrderItems (
    order_item_id       SERIAL PRIMARY KEY,
    order_id            INTEGER NOT NULL,
    item_id             INTEGER NULL,
    item_group_id       INTEGER NULL,
    item_name           VARCHAR(255) NOT NULL,
    is_manual           CHAR(1) NOT NULL DEFAULT 'N' CHECK (is_manual IN ('Y', 'N')),
    is_group            CHAR(1) NOT NULL DEFAULT 'N' CHECK (is_group IN ('Y', 'N')),
    quantity            INTEGER NOT NULL DEFAULT 1,
    created_at          TIMESTAMP NOT NULL DEFAULT NOW(),
    FOREIGN KEY (order_id) REFERENCES Orders(order_id) ON DELETE CASCADE,
    FOREIGN KEY (item_id) REFERENCES Items(item_id),
    FOREIGN KEY (item_group_id) REFERENCES ItemGroups(item_group_id)
);

-- Create OrderItemGroups table (for item groups associated with an order)
CREATE TABLE OrderItemGroups (
    order_item_group_id SERIAL PRIMARY KEY,
    order_id            INTEGER NOT NULL,
    item_group_id       INTEGER NOT NULL,
    item_group_name     VARCHAR(255) NOT NULL,
    created_at          TIMESTAMP NOT NULL DEFAULT NOW(),
    FOREIGN KEY (order_id) REFERENCES Orders(order_id) ON DELETE CASCADE,
    FOREIGN KEY (item_group_id) REFERENCES ItemGroups(item_group_id)
);

-- Create OrderAudits table
CREATE TABLE OrderAudits (
    order_audit_id      SERIAL PRIMARY KEY,
    order_id            INTEGER NOT NULL,
    action              VARCHAR(100) NOT NULL,
    performed_by        VARCHAR(100) NOT NULL,
    performed_at        TIMESTAMP NOT NULL DEFAULT NOW(),
    remarks             TEXT,
    FOREIGN KEY (order_id) REFERENCES Orders(order_id) ON DELETE CASCADE
);

-- Create indexes for better performance
CREATE INDEX idx_orders_order_no ON Orders(order_no);
CREATE INDEX idx_orders_doctor_id ON Orders(doctor_id);
CREATE INDEX idx_orders_hospital_id ON Orders(hospital_id);
CREATE INDEX idx_orders_status ON Orders(status);
CREATE INDEX idx_orders_is_delivered ON Orders(is_delivered);
CREATE INDEX idx_orders_order_date ON Orders(order_date);
CREATE INDEX idx_order_items_order_id ON OrderItems(order_id);
CREATE INDEX idx_order_item_groups_order_id ON OrderItemGroups(order_id);
CREATE INDEX idx_order_audits_order_id ON OrderAudits(order_id);

-- Insert sample data
INSERT INTO Orders (order_no, order_date, doctor_id, hospital_id, operation_date, operation_time, material_send_date, remarks, created_by, status, is_delivered)
VALUES 
    ('ORD-2024-001', '2024-12-20', 1, 1, '2024-12-25', '10:00:00', '2024-12-23', 'Urgent surgery - patient is critical', 'Admin', 'Pending', 'Pending'),
    ('ORD-2024-002', '2024-12-21', 2, 2, '2024-12-26', '14:30:00', '2024-12-24', 'Standard cardiac procedure', 'Admin', 'Assigned', 'Assigned'),
    ('ORD-2024-003', '2024-12-22', 3, 1, '2024-12-27', '09:00:00', '2024-12-25', 'Brain tumor surgery', 'Admin', 'Dispatched', 'Assigned'),
    ('ORD-2024-004', '2024-12-23', 1, 3, '2024-12-28', '11:00:00', '2024-12-26', 'Minimally invasive surgery', 'Admin', 'Completed', 'Delivered');

-- Insert sample order audits
INSERT INTO OrderAudits (order_id, action, performed_by, performed_at)
VALUES 
    (1, 'Created', 'Admin', '2024-12-20 09:00:00'),
    (2, 'Created', 'Admin', '2024-12-21 11:30:00'),
    (2, 'Assigned', 'Manager', '2024-12-21 15:00:00'),
    (3, 'Created', 'Admin', '2024-12-22 08:00:00'),
    (3, 'Assigned', 'Manager', '2024-12-22 10:00:00'),
    (3, 'Dispatched', 'Warehouse', '2024-12-23 14:00:00'),
    (4, 'Created', 'Admin', '2024-12-23 09:00:00'),
    (4, 'Assigned', 'Manager', '2024-12-23 12:00:00'),
    (4, 'Dispatched', 'Warehouse', '2024-12-24 10:00:00'),
    (4, 'Completed', 'System', '2024-12-28 16:00:00');
