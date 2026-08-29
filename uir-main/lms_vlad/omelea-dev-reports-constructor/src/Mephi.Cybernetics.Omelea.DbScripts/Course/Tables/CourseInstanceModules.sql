CREATE TABLE [Course].[CourseInstanceModules] (
    [ID]             UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [CourseID]       UNIQUEIDENTIFIER NOT NULL,
    [Year]           INT              NOT NULL,
    [CourseModuleID] UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    FOREIGN KEY ([CourseModuleID]) REFERENCES [Education].[CourseProgramModules] ([ID]),
    CONSTRAINT [FK__CourseInstanceMo__4D5F7D71] FOREIGN KEY ([CourseID], [Year]) REFERENCES [Course].[CourseInstances] ([CourseID], [Year])
);

