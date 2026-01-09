-- Test script to diagnose the issue

-- 1. Check current constraint on Orders table
SELECT conname, pg_get_constraintdef(oid) 
FROM pg_constraint 
WHERE conrelid = 'orders'::regclass AND conname LIKE '%status%';

-- 2. Try to manually update order 1 to 'In-operation' to see if constraint blocks it
UPDATE Orders 
SET status = 'In-operation' 
WHERE order_id = 1;

-- 3. Check if it worked
SELECT order_id, order_no, status FROM Orders WHERE order_id = 1;

-- 4. If above fails, run this to fix the constraint:
ALTER TABLE Orders DROP CONSTRAINT IF EXISTS orders_status_check;

ALTER TABLE Orders 
ADD CONSTRAINT orders_status_check 
CHECK (status IN ('Pending', 'Assigned', 'In-operation', 'Dispatched', 'Completed', 'Canceled'));

-- 5. Try the update again
UPDATE Orders 
SET status = 'In-operation' 
WHERE order_id = 1;

-- 6. Verify
SELECT order_id, order_no, status FROM Orders WHERE order_id = 1;
