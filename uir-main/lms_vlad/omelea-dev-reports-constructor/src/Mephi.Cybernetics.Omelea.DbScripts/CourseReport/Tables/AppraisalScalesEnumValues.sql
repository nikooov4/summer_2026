CREATE TABLE [CourseReport].[AppraisalScalesEnumValues] (
    [ScaleID]     UNIQUEIDENTIFIER NOT NULL,
    [Name]        NVARCHAR (32)    NOT NULL,
    [Description] NVARCHAR (1024)  DEFAULT ('') NOT NULL,
    [Code]        INT              NOT NULL,
    PRIMARY KEY CLUSTERED ([ScaleID] ASC, [Code] ASC),
    FOREIGN KEY ([ScaleID]) REFERENCES [CourseReport].[AppraisalScales] ([ID]),
    UNIQUE NONCLUSTERED ([ScaleID] ASC, [Name] ASC)
);

