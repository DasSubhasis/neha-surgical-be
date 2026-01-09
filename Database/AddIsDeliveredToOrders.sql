-- Add is_delivered column to Orders table
ALTER TABLE Orders
ADD COLUMN is_delivered VARCHAR(20) NOT NULL DEFAULT 'Pending' 
CHECK (is_delivered IN ('Pending', 'Assigned', 'Delivered'));

-- Add index for better performance
CREATE INDEX idx_orders_is_delivered ON Orders(is_delivered);

-- Update existing orders to have delivery status based on their current status
UPDATE Orders 
SET is_delivered = CASE 
    WHEN status = 'Completed' THEN 'Delivered'
    WHEN status IN ('Assigned', 'Dispatched') THEN 'Assigned'
    ELSE 'Pending'
END;
