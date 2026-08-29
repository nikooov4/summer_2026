CREATE TABLE [Education].[CourseAuthors] (
    [CourseProgramID] UNIQUEIDENTIFIER NOT NULL,
    [PersonID]        UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY CLUSTERED ([CourseProgramID] ASC, [PersonID] ASC),
    FOREIGN KEY ([CourseProgramID]) REFERENCES [Education].[CoursePrograms] ([ID]),
    FOREIGN KEY ([PersonID]) REFERENCES [Person].[Persons] ([ID])
);

