-- Table: ConsumptionImages
-- Stores multiple images for each consumption entry

CREATE TABLE IF NOT EXISTS ConsumptionImages (
    image_id SERIAL PRIMARY KEY,
    consumption_id INTEGER NOT NULL,
    image_path VARCHAR(500) NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    file_size BIGINT,
    content_type VARCHAR(100),
    uploaded_by VARCHAR(100),
    uploaded_at TIMESTAMP DEFAULT NOW(),
    is_active CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N')),
    
    CONSTRAINT fk_consumption
        FOREIGN KEY (consumption_id)
        REFERENCES Consumptions(consumption_id)
        ON DELETE CASCADE
);

-- Index for faster queries
CREATE INDEX idx_consumption_images_consumption_id ON ConsumptionImages(consumption_id);
CREATE INDEX idx_consumption_images_is_active ON ConsumptionImages(is_active);

-- Comments
COMMENT ON TABLE ConsumptionImages IS 'Stores multiple images associated with consumption entries';
COMMENT ON COLUMN ConsumptionImages.image_id IS 'Primary key';
COMMENT ON COLUMN ConsumptionImages.consumption_id IS 'Foreign key to Consumptions table';
COMMENT ON COLUMN ConsumptionImages.image_path IS 'Relative or absolute path to the uploaded image';
COMMENT ON COLUMN ConsumptionImages.file_name IS 'Original filename of the uploaded image';
COMMENT ON COLUMN ConsumptionImages.file_size IS 'Size of the image file in bytes';
COMMENT ON COLUMN ConsumptionImages.content_type IS 'MIME type of the image (e.g., image/jpeg, image/png)';
COMMENT ON COLUMN ConsumptionImages.uploaded_by IS 'Username or ID of the person who uploaded the image';
COMMENT ON COLUMN ConsumptionImages.uploaded_at IS 'Timestamp when the image was uploaded';
COMMENT ON COLUMN ConsumptionImages.is_active IS 'Soft delete flag: Y = active, N = deleted';
