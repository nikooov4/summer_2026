CREATE TABLE [CourseReport].[AppraisalListColumnsCompute] (
    [ColumnID]    UNIQUEIDENTIFIER NOT NULL,
    [Operation]   NVARCHAR (256)    NOT NULL,
    PRIMARY KEY CLUSTERED ([ColumnID] ASC),
    FOREIGN KEY ([ColumnID]) REFERENCES [CourseReport].[AppraisalListColumns] ([ID]) ON DELETE CASCADE
);

