CREATE TABLE [Education].[Courses] (
    [ID]         UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [CourseName] NVARCHAR (256)   NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC)
);

