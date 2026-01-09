-- Add 'In-operation' status to Orders table
-- First, drop the existing CHECK constraint
ALTER TABLE Orders DROP CONSTRAINT IF EXISTS orders_status_check;

-- Add the new CHECK constraint with 'In-operation' included
ALTER TABLE Orders 
ADD CONSTRAINT orders_status_check 
CHECK (status IN ('Pending', 'Assigned', 'In-operation', 'Dispatched', 'Completed', 'Canceled'));
