-- Table: PaymentCollections
-- Stores payment collection records from doctors/hospitals

CREATE TABLE IF NOT EXISTS PaymentCollections (
    collection_id SERIAL PRIMARY KEY,
    collection_date DATE NOT NULL,
    collected_by VARCHAR(100) NOT NULL,
    doctor_id INTEGER NOT NULL,
    hospital_id INTEGER NOT NULL,
    amount DECIMAL(15, 2) NOT NULL CHECK (amount >= 0),
    remarks TEXT,
    created_by VARCHAR(100),
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    is_active CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N')),
    
    CONSTRAINT fk_doctor
        FOREIGN KEY (doctor_id)
        REFERENCES Doctors(doctor_id),
    
    CONSTRAINT fk_hospital
        FOREIGN KEY (hospital_id)
        REFERENCES Hospitals(hospital_id)
);

-- Indexes for faster queries
CREATE INDEX idx_payment_collections_collection_date ON PaymentCollections(collection_date);
CREATE INDEX idx_payment_collections_doctor_id ON PaymentCollections(doctor_id);
CREATE INDEX idx_payment_collections_hospital_id ON PaymentCollections(hospital_id);
CREATE INDEX idx_payment_collections_is_active ON PaymentCollections(is_active);

-- Comments
COMMENT ON TABLE PaymentCollections IS 'Stores payment collection records from doctors and hospitals';
COMMENT ON COLUMN PaymentCollections.collection_id IS 'Primary key';
COMMENT ON COLUMN PaymentCollections.collection_date IS 'Date when payment was collected';
COMMENT ON COLUMN PaymentCollections.collected_by IS 'Name or ID of the person who collected the payment';
COMMENT ON COLUMN PaymentCollections.doctor_id IS 'Foreign key to Doctors table';
COMMENT ON COLUMN PaymentCollections.hospital_id IS 'Foreign key to Hospitals table';
COMMENT ON COLUMN PaymentCollections.amount IS 'Payment amount collected';
COMMENT ON COLUMN PaymentCollections.remarks IS 'Additional notes or comments';
COMMENT ON COLUMN PaymentCollections.created_by IS 'User who created this record';
COMMENT ON COLUMN PaymentCollections.created_at IS 'Timestamp when record was created';
COMMENT ON COLUMN PaymentCollections.updated_at IS 'Timestamp when record was last updated';
COMMENT ON COLUMN PaymentCollections.is_active IS 'Soft delete flag: Y = active, N = deleted';
