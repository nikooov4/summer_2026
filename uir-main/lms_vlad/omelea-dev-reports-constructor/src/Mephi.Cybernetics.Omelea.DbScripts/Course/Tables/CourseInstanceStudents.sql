CREATE TABLE [Course].[CourseInstanceStudents] (
    [CourseID]  UNIQUEIDENTIFIER NOT NULL,
    [Year]      INT              NOT NULL,
    [PersonID]  UNIQUEIDENTIFIER NOT NULL,
    [StartDate] DATETIME         DEFAULT (getdate()) NOT NULL,
    [EndDate]   DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([CourseID] ASC, [Year] ASC, [PersonID] ASC),
    CONSTRAINT [CourseInstanceStudents_EndDate] CHECK ([EndDate] IS NULL OR [EndDate] IS NOT NULL AND [StartDate]<[EndDate]),
    FOREIGN KEY ([PersonID]) REFERENCES [Person].[Persons] ([ID]),
    FOREIGN KEY ([CourseID], [Year]) REFERENCES [Course].[CourseInstances] ([CourseID], [Year])
);

