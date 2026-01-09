-- Add 'Completed (Pre-Billing)' status to Orders table
ALTER TABLE Orders DROP CONSTRAINT IF EXISTS orders_status_check;

ALTER TABLE Orders 
ADD CONSTRAINT orders_status_check 
CHECK (status IN ('Pending', 'Assigned', 'In-operation', 'Dispatched', 'Completed', 'Completed (Pre-Billing)', 'Canceled'));
