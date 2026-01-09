-- Create Items table
CREATE TABLE Items (
    item_id      SERIAL PRIMARY KEY,
    name         VARCHAR(100) NOT NULL,
    shortname    VARCHAR(20) NOT NULL,
    group_name   VARCHAR(100) NOT NULL,
    category     VARCHAR(100) NOT NULL,
    specification VARCHAR(100),
    size         VARCHAR(100),
    material     VARCHAR(100),
    model        VARCHAR(100),
    description  VARCHAR(250),
    price        NUMERIC(12,2) NOT NULL DEFAULT 0.00,
    status       VARCHAR(20) NOT NULL DEFAULT 'Active',
    is_active    CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N')),
    created_at   TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at   TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_items_is_active ON Items(is_active);
CREATE INDEX idx_items_group_name ON Items(group_name);
CREATE INDEX idx_items_category ON Items(category);
