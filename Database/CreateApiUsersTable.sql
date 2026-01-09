-- Create API Users table for frontend application authentication
CREATE TABLE api_users (
    user_id SERIAL PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    api_key VARCHAR(100) NOT NULL UNIQUE,
    role VARCHAR(50) NOT NULL DEFAULT 'User',
    is_active CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N')),
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Insert default frontend applications
INSERT INTO api_users (username, api_key, role, is_active) 
VALUES 
    ('admin_portal', 'NS-2025-ADMIN-PORTAL-KEY', 'Admin', 'Y'),
    ('web_app', 'NS-2025-WEB-APP-KEY', 'User', 'Y'),
    ('mobile_app', 'NS-2025-MOBILE-APP-KEY', 'User', 'Y'),
    ('doctor_portal', 'NS-2025-DOCTOR-PORTAL-KEY', 'User', 'Y');

-- Create indexes
CREATE INDEX idx_api_users_api_key ON api_users(api_key);
CREATE INDEX idx_api_users_is_active ON api_users(is_active);
