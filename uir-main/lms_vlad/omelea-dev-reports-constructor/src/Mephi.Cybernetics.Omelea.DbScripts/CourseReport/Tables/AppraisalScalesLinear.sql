CREATE TABLE [CourseReport].[AppraisalScalesLinear] (
    [ID]          UNIQUEIDENTIFIER NOT NULL,
    [MinValue]    INT              NOT NULL,
    [MaxValue]    INT              NOT NULL,
    [ValueFormat] NVARCHAR (32)    NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    FOREIGN KEY ([ID]) REFERENCES [CourseReport].[AppraisalScales] ([ID]),
    FOREIGN KEY ([ValueFormat]) REFERENCES [CourseReport].[ValueFormats] ([ID])
);

