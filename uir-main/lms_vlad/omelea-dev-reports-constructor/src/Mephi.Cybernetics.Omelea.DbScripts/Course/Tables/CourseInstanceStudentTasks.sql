CREATE TABLE [Course].[CourseInstanceStudentTasks] (
    [CourseInstanceModuleTaskID] UNIQUEIDENTIFIER NOT NULL,
    [PersonID]                   UNIQUEIDENTIFIER NOT NULL,
    [Score]                      INT              DEFAULT ((0)) NOT NULL,
    [AcceptanceDate]             DATETIME         DEFAULT (getdate()) NOT NULL,
    [Comment]                    NVARCHAR (MAX)   DEFAULT ('') NOT NULL,
    [OrderIndex]                 INT              NOT NULL,
    [StartDate]                  DATETIME         DEFAULT (getdate()) NOT NULL,
    [EndDate]                    DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([CourseInstanceModuleTaskID] ASC, [PersonID] ASC, [OrderIndex] ASC),
    CONSTRAINT [CourseInstanceStudentTasks_EndDate] CHECK ([EndDate] IS NULL OR [EndDate] IS NOT NULL AND [StartDate]<[EndDate]),
    FOREIGN KEY ([PersonID]) REFERENCES [Person].[Persons] ([ID]),
    CONSTRAINT [FK__CourseIns__Cours__625A9A57] FOREIGN KEY ([CourseInstanceModuleTaskID]) REFERENCES [Course].[CourseInstanceModuleTasks] ([ID])
);

