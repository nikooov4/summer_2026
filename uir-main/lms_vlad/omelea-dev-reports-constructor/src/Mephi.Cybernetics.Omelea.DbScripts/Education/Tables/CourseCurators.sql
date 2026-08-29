CREATE TABLE [Education].[CourseCurators] (
    [CourseID]  UNIQUEIDENTIFIER NOT NULL,
    [PersonID]  UNIQUEIDENTIFIER NOT NULL,
    [StartDate] DATETIME         NOT NULL,
    [EndDate]   DATETIME         NULL,
    [IsLead]    BIT              DEFAULT ((0)) NULL,
    PRIMARY KEY CLUSTERED ([CourseID] ASC, [PersonID] ASC, [StartDate] ASC),
    CONSTRAINT [EdCourseCurators_EndDate] CHECK ([EndDate] IS NULL OR [EndDate] IS NOT NULL AND [StartDate]<[EndDate]),
    FOREIGN KEY ([CourseID]) REFERENCES [Education].[Courses] ([ID]),
    FOREIGN KEY ([PersonID]) REFERENCES [Person].[Persons] ([ID])
);

