-- Create Roles table
CREATE TABLE Roles (
    role_id       SERIAL PRIMARY KEY,
    role_name     VARCHAR(50) NOT NULL UNIQUE,
    description   VARCHAR(250),
    is_active     CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N')),
    created_at    TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at    TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Create Menus table (sidebar menu items)
CREATE TABLE Menus (
    menu_id       SERIAL PRIMARY KEY,
    menu_name     VARCHAR(100) NOT NULL,
    menu_path     VARCHAR(200) NOT NULL,
    menu_icon     VARCHAR(50),
    parent_menu_id INT REFERENCES Menus(menu_id) ON DELETE CASCADE,
    sort_order    INT NOT NULL DEFAULT 0,
    is_active     CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N')),
    created_at    TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at    TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Create RoleMenuPermissions table (role to menu mapping)
CREATE TABLE RoleMenuPermissions (
    permission_id SERIAL PRIMARY KEY,
    role_id       INT NOT NULL REFERENCES Roles(role_id) ON DELETE CASCADE,
    menu_id       INT NOT NULL REFERENCES Menus(menu_id) ON DELETE CASCADE,
    can_view      CHAR(1) NOT NULL DEFAULT 'Y' CHECK (can_view IN ('Y', 'N')),
    can_create    CHAR(1) NOT NULL DEFAULT 'N' CHECK (can_create IN ('Y', 'N')),
    can_edit      CHAR(1) NOT NULL DEFAULT 'N' CHECK (can_edit IN ('Y', 'N')),
    can_delete    CHAR(1) NOT NULL DEFAULT 'N' CHECK (can_delete IN ('Y', 'N')),
    created_at    TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE(role_id, menu_id)
);

-- Update SystemUsers to reference Roles table
ALTER TABLE SystemUsers DROP COLUMN IF EXISTS role;
ALTER TABLE SystemUsers ADD COLUMN role_id INT REFERENCES Roles(role_id);

-- Insert default roles
INSERT INTO Roles (role_name, description, is_active)
VALUES 
    ('Super Admin', 'Full system access with all permissions', 'Y'),
    ('Admin', 'Administrative access with most permissions', 'Y'),
    ('Doctor', 'Doctor access with patient and medical records', 'Y'),
    ('Staff', 'Staff access with limited permissions', 'Y'),
    ('Viewer', 'Read-only access', 'Y');

-- Insert menu items
INSERT INTO Menus (menu_name, menu_path, menu_icon, parent_menu_id, sort_order, is_active)
VALUES 
    -- Main menus
    ('Dashboard', '/dashboard', 'dashboard', NULL, 1, 'Y'),
    ('Masters', '/masters', 'settings', NULL, 2, 'Y'),
    ('Doctors', '/doctors', 'person', NULL, 3, 'Y'),
    ('Hospitals', '/hospitals', 'local_hospital', NULL, 4, 'Y'),
    ('Inventory', '/inventory', 'inventory', NULL, 5, 'Y'),
    ('Reports', '/reports', 'assessment', NULL, 6, 'Y'),
    ('Users', '/users', 'group', NULL, 7, 'Y'),
    ('Settings', '/settings', 'settings', NULL, 8, 'Y');

-- Insert sub-menus under Masters
INSERT INTO Menus (menu_name, menu_path, menu_icon, parent_menu_id, sort_order, is_active)
VALUES 
    ('Item Groups', '/masters/item-groups', 'category', (SELECT menu_id FROM Menus WHERE menu_path = '/masters'), 1, 'Y'),
    ('Items', '/masters/items', 'inventory_2', (SELECT menu_id FROM Menus WHERE menu_path = '/masters'), 2, 'Y');

-- Assign all menus to Super Admin with full permissions
INSERT INTO RoleMenuPermissions (role_id, menu_id, can_view, can_create, can_edit, can_delete)
SELECT 
    (SELECT role_id FROM Roles WHERE role_name = 'Super Admin'),
    menu_id,
    'Y', 'Y', 'Y', 'Y'
FROM Menus;

-- Assign menus to Admin (all except Settings with full permissions)
INSERT INTO RoleMenuPermissions (role_id, menu_id, can_view, can_create, can_edit, can_delete)
SELECT 
    (SELECT role_id FROM Roles WHERE role_name = 'Admin'),
    menu_id,
    'Y', 'Y', 'Y', 'Y'
FROM Menus
WHERE menu_path != '/settings';

-- Assign menus to Doctor (Dashboard, Doctors, Hospitals, Reports - view only)
INSERT INTO RoleMenuPermissions (role_id, menu_id, can_view, can_create, can_edit, can_delete)
SELECT 
    (SELECT role_id FROM Roles WHERE role_name = 'Doctor'),
    menu_id,
    'Y', 'N', 'N', 'N'
FROM Menus
WHERE menu_path IN ('/dashboard', '/doctors', '/hospitals', '/reports');

-- Assign menus to Staff (Dashboard, Inventory - view and create)
INSERT INTO RoleMenuPermissions (role_id, menu_id, can_view, can_create, can_edit, can_delete)
SELECT 
    (SELECT role_id FROM Roles WHERE role_name = 'Staff'),
    menu_id,
    'Y', 'Y', 'N', 'N'
FROM Menus
WHERE menu_path IN ('/dashboard', '/inventory');

-- Assign menus to Viewer (Dashboard only - view only)
INSERT INTO RoleMenuPermissions (role_id, menu_id, can_view, can_create, can_edit, can_delete)
SELECT 
    (SELECT role_id FROM Roles WHERE role_name = 'Viewer'),
    menu_id,
    'Y', 'N', 'N', 'N'
FROM Menus
WHERE menu_path = '/dashboard';

-- Update existing users with role_id (map old text roles to new role_id)
UPDATE SystemUsers SET role_id = (SELECT role_id FROM Roles WHERE role_name = 'Admin') WHERE email = 'admin@nehasurgical.com';
UPDATE SystemUsers SET role_id = (SELECT role_id FROM Roles WHERE role_name = 'Doctor') WHERE email = 'doctor@nehasurgical.com';
UPDATE SystemUsers SET role_id = (SELECT role_id FROM Roles WHERE role_name = 'Staff') WHERE email = 'staff@nehasurgical.com';
