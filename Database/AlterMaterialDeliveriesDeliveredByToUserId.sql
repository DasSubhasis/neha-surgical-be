-- Alter MaterialDeliveries table to use user_id instead of username for delivered_by
-- Step 1: Add new column for user_id
ALTER TABLE MaterialDeliveries 
ADD COLUMN delivered_by_user_id INTEGER;

ALTER TABLE MaterialDeliveries 
ADD COLUMN actual_delivery_by VARCHAR(100),
ADD COLUMN actual_delivery_by_userid INTEGER,
ADD COLUMN actual_delivery_time TIMESTAMP;

-- Step 2: Add foreign key constraint
ALTER TABLE MaterialDeliveries 
ADD CONSTRAINT fk_material_deliveries_delivered_by 
FOREIGN KEY (delivered_by_user_id) REFERENCES SystemUsers(system_user_id);

-- Step 3: Drop the old delivered_by column (if you want to keep it for now, comment this out)
-- ALTER TABLE MaterialDeliveries DROP COLUMN delivered_by;

-- Step 4: Rename the new column to delivered_by (optional, or keep as delivered_by_user_id)
-- ALTER TABLE MaterialDeliveries RENAME COLUMN delivered_by_user_id TO delivered_by;

-- Note: For now, keep both columns during transition. Once all data is migrated, 
-- you can drop the old varchar column and rename the new one.
-- The API will use delivered_by_user_id going forward.
