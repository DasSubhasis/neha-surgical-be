-- Create AssistantAssignments table
CREATE TABLE AssistantAssignments (
    assignment_id      SERIAL PRIMARY KEY,
    order_id           INT NOT NULL REFERENCES Orders(order_id) ON DELETE CASCADE,
    assistant_id       INT NOT NULL REFERENCES SystemUsers(system_user_id) ON DELETE CASCADE,
    reporting_date     DATE NOT NULL,
    reporting_time     TIME NOT NULL,
    remarks            TEXT,
    assigned_at        TIMESTAMP NOT NULL DEFAULT NOW(),
    assigned_by        INT REFERENCES SystemUsers(system_user_id) ON DELETE SET NULL,
    is_active          CHAR(1) NOT NULL DEFAULT 'Y' CHECK (is_active IN ('Y', 'N')),
    created_at         TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at         TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE(order_id)  -- One assistant per order
);

-- Create indexes for faster lookups
CREATE INDEX idx_assistant_assignments_order_id ON AssistantAssignments(order_id);
CREATE INDEX idx_assistant_assignments_assistant_id ON AssistantAssignments(assistant_id);
CREATE INDEX idx_assistant_assignments_is_active ON AssistantAssignments(is_active);

-- Add comment
COMMENT ON TABLE AssistantAssignments IS 'Stores assignment of assistant users to orders';
COMMENT ON COLUMN AssistantAssignments.reporting_time IS 'Time the assistant should report for the operation';
