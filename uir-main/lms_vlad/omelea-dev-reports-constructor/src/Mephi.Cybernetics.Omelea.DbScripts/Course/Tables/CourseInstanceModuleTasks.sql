CREATE TABLE [Course].[CourseInstanceModuleTasks] (
    [ID]                     UNIQUEIDENTIFIER CONSTRAINT [DF__CourseInstan__ID__503BEA1C] DEFAULT (newid()) NOT NULL,
    [TaskName]               NVARCHAR (256)   NOT NULL,
    [TaskDescription]        NVARCHAR (4000)  CONSTRAINT [DF_CourseInstanceModuleTasks_TaskDescription] DEFAULT ('') NOT NULL,
    [CourseInstanceModuleID] UNIQUEIDENTIFIER NOT NULL,
    [TaskTypeID]             INT              NOT NULL,
    [TaskID]                 UNIQUEIDENTIFIER NULL,
    [MaxScore]               INT              NOT NULL,
    [MinAcceptScore]         INT              NOT NULL,
    [OrderIndex]             INT              NOT NULL,
    [StartDate]              DATETIME         NOT NULL,
    [EndDate]                DATETIME         NULL,
    CONSTRAINT [PK__CourseIn__3214EC27937BBAF7] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [CourseInstanceModuleTask_EndDate] CHECK ([EndDate] IS NULL OR [EndDate] IS NOT NULL AND [StartDate]<[EndDate]),
    CONSTRAINT [FK__CourseIns__Cours__51300E55] FOREIGN KEY ([CourseInstanceModuleID]) REFERENCES [Course].[CourseInstanceModules] ([ID]),
    CONSTRAINT [FK__CourseIns__TaskI__531856C7] FOREIGN KEY ([TaskID]) REFERENCES [Education].[CourseTasks] ([ID]),
    CONSTRAINT [FK__CourseIns__TaskT__5224328E] FOREIGN KEY ([TaskTypeID]) REFERENCES [Education].[CourseTaskTypes] ([ID])
);

