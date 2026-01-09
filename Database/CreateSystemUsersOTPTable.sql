-- Create SystemUsers table with OTP-based authentication
-- NOTE: This table should be created AFTER CreateRolesAndMenusTable.sql
CREATE TABLE SystemUsers (
    system_user_id SERIAL PRIMARY KEY,
    email          VARCHAR(100) NOT NULL UNIQUE,
    full_name      VARCHAR(100) NOT NULL,
    role_id        INT NOT NULL REFERENCES Roles(role_id),
    phone_no       VARCHAR(20),
    employee_id    VARCHAR(50),
    identifier     VARCHAR(100),
    is_active      CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N')),
    created_at     TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at     TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Create OTP table for email-based authentication
CREATE TABLE UserOtps (
    otp_id         SERIAL PRIMARY KEY,
    system_user_id INT NOT NULL REFERENCES SystemUsers(system_user_id) ON DELETE CASCADE,
    otp_code       VARCHAR(6) NOT NULL,
    expires_at     TIMESTAMP NOT NULL,
    is_used        CHAR(1) NOT NULL DEFAULT 'N' CHECK (is_used IN ('Y', 'N')),
    created_at     TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Create indexes for faster lookups
CREATE INDEX idx_systemusers_role_id ON SystemUsers(role_id);
CREATE INDEX idx_systemusers_email ON SystemUsers(email);
CREATE INDEX idx_user_otps_user_id ON UserOtps(system_user_id);
CREATE INDEX idx_user_otps_expires_at ON UserOtps(expires_at);

-- Insert sample users (role_id values from CreateRolesAndMenusTable.sql)
-- 1: Super Admin, 2: Admin, 3: Doctor, 4: Staff, 5: Viewer
INSERT INTO SystemUsers (email, full_name, role_id, phone_no, is_active)
VALUES 
    ('admin@nehasurgical.com', 'System Administrator', 2, '9876543210', 'Y'),
    ('doctor@nehasurgical.com', 'Dr. Smith', 3, '9876543211', 'Y'),
    ('staff@nehasurgical.com', 'John Staff', 4, '9876543212', 'Y');
