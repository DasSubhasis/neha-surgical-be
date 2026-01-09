-- Add username column to SystemUsers table if not exists
ALTER TABLE SystemUsers ADD COLUMN IF NOT EXISTS username VARCHAR(50) UNIQUE;

-- Create index on username
CREATE INDEX IF NOT EXISTS idx_system_users_username ON SystemUsers(username);

-- Update existing users with default username (email prefix)
UPDATE SystemUsers 
SET username = SPLIT_PART(email, '@', 1) 
WHERE username IS NULL;

-- Make username NOT NULL after updating existing records
-- ALTER TABLE SystemUsers ALTER COLUMN username SET NOT NULL;
