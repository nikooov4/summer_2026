CREATE TABLE [CourseReport].[AppraisalListColumnsDependencies] (
    [ColumnID]    UNIQUEIDENTIFIER NOT NULL,
    [ColumnDepID] UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY CLUSTERED ([ColumnID] ASC, [ColumnDepID] ASC),
    FOREIGN KEY ([ColumnDepID]) REFERENCES [CourseReport].[AppraisalListColumns] ([ID]),
    FOREIGN KEY ([ColumnID]) REFERENCES [CourseReport].[AppraisalListColumnsCompute] ([ColumnID])
);

