CREATE TABLE [CourseReport].[AppraisalListColumnsCompute] (
    [ColumnID]    UNIQUEIDENTIFIER NOT NULL,
    [OperationID] NVARCHAR (32)    NOT NULL,
    PRIMARY KEY CLUSTERED ([ColumnID] ASC),
    FOREIGN KEY ([ColumnID]) REFERENCES [CourseReport].[AppraisalListColumns] ([ID]),
    CONSTRAINT [FK_AppraisalListColumnsCompute_ColumnOperations] FOREIGN KEY ([OperationID]) REFERENCES [CourseReport].[ColumnOperations] ([ID])
);

