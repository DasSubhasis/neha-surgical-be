-- Create SubAssistantAssignments table for assigning additional assistants under main assistant
CREATE TABLE SubAssistantAssignments (
    sub_assignment_id     SERIAL PRIMARY KEY,
    assignment_id         INT NOT NULL REFERENCES AssistantAssignments(assignment_id) ON DELETE CASCADE,
    sub_assistant_id      INT NOT NULL REFERENCES SystemUsers(system_user_id) ON DELETE CASCADE,
    remarks               TEXT,
    assigned_at           TIMESTAMP NOT NULL DEFAULT NOW(),
    assigned_by           INT REFERENCES SystemUsers(system_user_id) ON DELETE SET NULL,
    is_active             CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N')),
    created_at            TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at            TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Create indexes for faster lookups
CREATE INDEX idx_sub_assistant_assignments_assignment_id ON SubAssistantAssignments(assignment_id);
CREATE INDEX idx_sub_assistant_assignments_sub_assistant_id ON SubAssistantAssignments(sub_assistant_id);
CREATE INDEX idx_sub_assistant_assignments_is_active ON SubAssistantAssignments(is_active);

-- Add comment
COMMENT ON TABLE SubAssistantAssignments IS 'Stores sub-assistant assignments under main assistant for orders';
COMMENT ON COLUMN SubAssistantAssignments.assignment_id IS 'Reference to the main assistant assignment';
COMMENT ON COLUMN SubAssistantAssignments.sub_assistant_id IS 'User ID of the sub-assistant';
