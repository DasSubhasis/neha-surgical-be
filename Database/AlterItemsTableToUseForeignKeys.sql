-- Alter Items table to use foreign keys instead of text values
-- Run this script to migrate the existing Items table

-- Step 1: Add new columns for foreign keys
ALTER TABLE Items 
    ADD COLUMN brand_id INTEGER,
    ADD COLUMN category_id INTEGER,
    ADD COLUMN specification_id INTEGER,
    ADD COLUMN size_id INTEGER;

-- Step 2: Migrate existing data (match names to IDs)
-- Update brand_id based on group_name
UPDATE Items i
SET brand_id = b.brand_id
FROM Brands b
WHERE LOWER(TRIM(i.group_name)) = LOWER(TRIM(b.name));

-- Update category_id based on category name
UPDATE Items i
SET category_id = c.category_id
FROM Categories c
WHERE LOWER(TRIM(i.category)) = LOWER(TRIM(c.name));

-- Update specification_id based on specification name
UPDATE Items i
SET specification_id = s.specification_id
FROM Specifications s
WHERE LOWER(TRIM(i.specification)) = LOWER(TRIM(s.name));

-- Update size_id based on size name
UPDATE Items i
SET size_id = sz.size_id
FROM Sizes sz
WHERE LOWER(TRIM(i.size)) = LOWER(TRIM(sz.name));

-- Step 3: Drop old text columns
ALTER TABLE Items 
    DROP COLUMN group_name,
    DROP COLUMN category,
    DROP COLUMN specification,
    DROP COLUMN size;

-- Step 4: Make brand_id and category_id NOT NULL (required fields)
ALTER TABLE Items 
    ALTER COLUMN brand_id SET NOT NULL,
    ALTER COLUMN category_id SET NOT NULL;

-- Step 5: Add foreign key constraints
ALTER TABLE Items
    ADD CONSTRAINT fk_items_brand FOREIGN KEY (brand_id) REFERENCES Brands(brand_id),
    ADD CONSTRAINT fk_items_category FOREIGN KEY (category_id) REFERENCES Categories(category_id),
    ADD CONSTRAINT fk_items_specification FOREIGN KEY (specification_id) REFERENCES Specifications(specification_id),
    ADD CONSTRAINT fk_items_size FOREIGN KEY (size_id) REFERENCES Sizes(size_id);

-- Step 6: Create indexes on foreign key columns
CREATE INDEX idx_items_brand_id ON Items(brand_id);
CREATE INDEX idx_items_category_id ON Items(category_id);
CREATE INDEX idx_items_specification_id ON Items(specification_id);
CREATE INDEX idx_items_size_id ON Items(size_id);

-- Drop old indexes if they exist
DROP INDEX IF EXISTS idx_items_group_name;
DROP INDEX IF EXISTS idx_items_category;
