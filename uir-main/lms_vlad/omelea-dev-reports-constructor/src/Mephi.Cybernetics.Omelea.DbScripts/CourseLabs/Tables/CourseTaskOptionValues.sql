CREATE TABLE [CourseLabs].[CourseTaskOptionValues] (
    [ID]                  UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [Name]                NVARCHAR (128)   NOT NULL,
    [Description]         NVARCHAR (2048)  DEFAULT ('') NOT NULL,
    [Value]               INT              NOT NULL,
    [CourseTaskOptionsID] UNIQUEIDENTIFIER NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    FOREIGN KEY ([CourseTaskOptionsID]) REFERENCES [CourseLabs].[CourseTaskOptions] ([ID])
);

