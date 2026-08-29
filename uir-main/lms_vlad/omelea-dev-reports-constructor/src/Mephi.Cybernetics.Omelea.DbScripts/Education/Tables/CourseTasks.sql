CREATE TABLE [Education].[CourseTasks] (
    [ID]             UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [CourseModuleID] UNIQUEIDENTIFIER NOT NULL,
    [TaskName]       NVARCHAR (256)   NULL,
    [Description]    NVARCHAR (MAX)   DEFAULT ('') NOT NULL,
    [TaskTypeID]     INT              NOT NULL,
    [MaxScore]       INT              NOT NULL,
    [MinAcceptScore] INT              NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    FOREIGN KEY ([CourseModuleID]) REFERENCES [Education].[CourseProgramModules] ([ID]),
    FOREIGN KEY ([TaskTypeID]) REFERENCES [Education].[CourseTaskTypes] ([ID])
);

