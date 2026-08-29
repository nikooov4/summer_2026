CREATE TABLE [Course].[CourseInstanceEdPrograms] (
    [CourseID]        UNIQUEIDENTIFIER NOT NULL,
    [Year]            INT              NOT NULL,
    [CourseProgramID] UNIQUEIDENTIFIER NOT NULL,
    [IsMain]          BIT              DEFAULT ((0)) NULL,
    PRIMARY KEY CLUSTERED ([CourseID] ASC, [Year] ASC, [CourseProgramID] ASC),
    FOREIGN KEY ([CourseProgramID]) REFERENCES [Education].[CoursePrograms] ([ID]),
    CONSTRAINT [FK__CourseInstanceEd__489AC854] FOREIGN KEY ([CourseID], [Year]) REFERENCES [Course].[CourseInstances] ([CourseID], [Year])
);

