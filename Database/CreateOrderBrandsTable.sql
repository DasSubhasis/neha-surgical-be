-- Create OrderBrands table to store brands associated with orders
CREATE TABLE IF NOT EXISTS OrderBrands (
    order_brand_id      SERIAL PRIMARY KEY,
    order_id            INTEGER NOT NULL,
    brand_id            INTEGER NOT NULL,
    brand_name          VARCHAR(255) NOT NULL,
    created_at          TIMESTAMP NOT NULL DEFAULT NOW(),
    FOREIGN KEY (order_id) REFERENCES Orders(order_id) ON DELETE CASCADE,
    FOREIGN KEY (brand_id) REFERENCES Brands(brand_id)
);

-- Create index for faster lookups
CREATE INDEX IF NOT EXISTS idx_order_brands_order_id ON OrderBrands(order_id);
CREATE INDEX IF NOT EXISTS idx_order_brands_brand_id ON OrderBrands(brand_id);
