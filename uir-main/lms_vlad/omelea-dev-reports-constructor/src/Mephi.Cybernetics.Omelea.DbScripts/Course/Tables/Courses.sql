CREATE TABLE [Course].[Courses] (
    [ID]          UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [CourseName]  NVARCHAR (256)   NOT NULL,
    [Description] NVARCHAR (MAX)   DEFAULT ('') NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC)
);

