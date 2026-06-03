-- Migration: Add payment_mode and payment_reference to PaymentCollections
-- Run this script on an existing database that already has the PaymentCollections table.

ALTER TABLE PaymentCollections
    ADD COLUMN IF NOT EXISTS payment_mode VARCHAR(50) NULL,
    ADD COLUMN IF NOT EXISTS payment_reference VARCHAR(100) NULL;

COMMENT ON COLUMN PaymentCollections.payment_mode IS 'Mode of payment: Cash, Cheque, UPI, NEFT, RTGS, DD, Bank Transfer';
COMMENT ON COLUMN PaymentCollections.payment_reference IS 'Reference number: cheque no, UPI/UTR no, transaction ref, DD no, etc.';
