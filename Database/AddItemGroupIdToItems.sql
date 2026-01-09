-- Add item_group_id column to Items table

-- Step 1: Add the column
ALTER TABLE Items 
    ADD COLUMN IF NOT EXISTS item_group_id INTEGER;

-- Step 2: Add foreign key constraint
ALTER TABLE Items
    ADD CONSTRAINT fk_items_item_group FOREIGN KEY (item_group_id) REFERENCES ItemGroups(item_group_id);

-- Step 3: Create index for better performance
CREATE INDEX IF NOT EXISTS idx_items_item_group_id ON Items(item_group_id);
