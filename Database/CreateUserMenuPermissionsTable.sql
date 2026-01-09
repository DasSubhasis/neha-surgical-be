-- Create UserMenuPermissions table for user-specific menu access
-- This allows individual users to have custom menu permissions that override role-based permissions
CREATE TABLE UserMenuPermissions (
    user_permission_id SERIAL PRIMARY KEY,
    system_user_id     INT NOT NULL REFERENCES SystemUsers(system_user_id) ON DELETE CASCADE,
    menu_id            INT NOT NULL REFERENCES Menus(menu_id) ON DELETE CASCADE,
    can_view           CHAR(1) NOT NULL DEFAULT 'Y' CHECK (can_view IN ('Y', 'N')),
    can_create         CHAR(1) NOT NULL DEFAULT 'N' CHECK (can_create IN ('Y', 'N')),
    can_edit           CHAR(1) NOT NULL DEFAULT 'N' CHECK (can_edit IN ('Y', 'N')),
    can_delete         CHAR(1) NOT NULL DEFAULT 'N' CHECK (can_delete IN ('Y', 'N')),
    created_at         TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at         TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE(system_user_id, menu_id)
);

-- Create index for faster lookups
CREATE INDEX idx_usermenu_user_id ON UserMenuPermissions(system_user_id);
CREATE INDEX idx_usermenu_menu_id ON UserMenuPermissions(menu_id);

-- Comments
COMMENT ON TABLE UserMenuPermissions IS 'User-specific menu permissions that override role-based permissions';
COMMENT ON COLUMN UserMenuPermissions.system_user_id IS 'Reference to the user';
COMMENT ON COLUMN UserMenuPermissions.menu_id IS 'Reference to the menu';
