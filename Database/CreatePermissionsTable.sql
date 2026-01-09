-- Create Permissions table (predefined permissions)
CREATE TABLE Permissions (
    permission_id   SERIAL PRIMARY KEY,
    code            VARCHAR(50) NOT NULL UNIQUE,
    name            VARCHAR(100) NOT NULL,
    module          VARCHAR(50) NOT NULL,
    description     VARCHAR(250),
    is_active       CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N')),
    created_at      TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Create RolePermissions junction table (many-to-many relationship)
CREATE TABLE RolePermissions (
    role_permission_id SERIAL PRIMARY KEY,
    role_id            INT NOT NULL REFERENCES Roles(role_id) ON DELETE CASCADE,
    permission_id      INT NOT NULL REFERENCES Permissions(permission_id) ON DELETE CASCADE,
    created_at         TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE(role_id, permission_id)
);

-- Insert predefined permissions
INSERT INTO Permissions (code, name, module, description, is_active)
VALUES 
    -- Dashboard
    ('DASHBOARD_VIEW', 'View Dashboard', 'Dashboard', 'Can view dashboard', 'Y'),
    
    -- User Management
    ('USER_VIEW', 'View Users', 'User Management', 'Can view users list', 'Y'),
    ('USER_CREATE', 'Create Users', 'User Management', 'Can create new users', 'Y'),
    ('USER_EDIT', 'Edit Users', 'User Management', 'Can edit users', 'Y'),
    ('USER_DELETE', 'Delete Users', 'User Management', 'Can delete users', 'Y'),
    
    -- Role Management
    ('ROLE_VIEW', 'View Roles', 'Role Management', 'Can view roles list', 'Y'),
    ('ROLE_CREATE', 'Create Roles', 'Role Management', 'Can create new roles', 'Y'),
    ('ROLE_EDIT', 'Edit Roles', 'Role Management', 'Can edit roles', 'Y'),
    ('ROLE_DELETE', 'Delete Roles', 'Role Management', 'Can delete roles', 'Y'),
    
    -- Master Data
    ('MASTER_VIEW', 'View Master Data', 'Master Data', 'Can view master data', 'Y'),
    ('MASTER_CREATE', 'Create Master Data', 'Master Data', 'Can create master data', 'Y'),
    ('MASTER_EDIT', 'Edit Master Data', 'Master Data', 'Can edit master data', 'Y'),
    ('MASTER_DELETE', 'Delete Master Data', 'Master Data', 'Can delete master data', 'Y'),
    
    -- Orders
    ('ORDER_VIEW', 'View Orders', 'Orders', 'Can view orders', 'Y'),
    ('ORDER_CREATE', 'Create Orders', 'Orders', 'Can create orders', 'Y'),
    ('ORDER_EDIT', 'Edit Orders', 'Orders', 'Can edit orders', 'Y'),
    ('ORDER_DELETE', 'Delete Orders', 'Orders', 'Can delete orders', 'Y'),
    
    -- Reports
    ('REPORT_VIEW', 'View Reports', 'Reports', 'Can view reports', 'Y'),
    ('REPORT_EXPORT', 'Export Reports', 'Reports', 'Can export reports', 'Y'),
    
    -- Billing
    ('BILLING_VIEW', 'View Billing', 'Billing', 'Can view billing', 'Y'),
    ('BILLING_CREATE', 'Create Bills', 'Billing', 'Can create bills', 'Y'),
    ('BILLING_EDIT', 'Edit Bills', 'Billing', 'Can edit bills', 'Y'),
    
    -- Payments
    ('PAYMENT_VIEW', 'View Payments', 'Payments', 'Can view payments', 'Y'),
    ('PAYMENT_COLLECT', 'Collect Payments', 'Payments', 'Can collect payments', 'Y');

-- Assign all permissions to Super Admin
INSERT INTO RolePermissions (role_id, permission_id)
SELECT 
    (SELECT role_id FROM Roles WHERE role_name = 'Super Admin'),
    permission_id
FROM Permissions;

-- Assign most permissions to Admin (except Role management)
INSERT INTO RolePermissions (role_id, permission_id)
SELECT 
    (SELECT role_id FROM Roles WHERE role_name = 'Admin'),
    permission_id
FROM Permissions
WHERE code NOT LIKE 'ROLE_%';

-- Assign view permissions to Doctor
INSERT INTO RolePermissions (role_id, permission_id)
SELECT 
    (SELECT role_id FROM Roles WHERE role_name = 'Doctor'),
    permission_id
FROM Permissions
WHERE code IN ('DASHBOARD_VIEW', 'ORDER_VIEW', 'REPORT_VIEW');

-- Assign limited permissions to Staff
INSERT INTO RolePermissions (role_id, permission_id)
SELECT 
    (SELECT role_id FROM Roles WHERE role_name = 'Staff'),
    permission_id
FROM Permissions
WHERE code IN ('DASHBOARD_VIEW', 'MASTER_VIEW', 'ORDER_VIEW', 'ORDER_CREATE');

-- Assign view-only permissions to Viewer
INSERT INTO RolePermissions (role_id, permission_id)
SELECT 
    (SELECT role_id FROM Roles WHERE role_name = 'Viewer'),
    permission_id
FROM Permissions
WHERE code LIKE '%_VIEW';
