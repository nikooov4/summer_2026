CREATE TABLE [CourseLabs].[StudentCourseTasks] (
    [ID]              UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [Name]            NVARCHAR (256)   NULL,
    [TaskDescription] NVARCHAR (4000)  DEFAULT ('') NOT NULL,
    [CourseID]        UNIQUEIDENTIFIER NOT NULL,
    [Year]            INT              NOT NULL,
    [StudentPersonID] UNIQUEIDENTIFIER NOT NULL,
    [TaskTypeID]      INT              NOT NULL,
    [MaxScore]        INT              NOT NULL,
    [MinAcceptScore]  INT              NOT NULL,
    [OrderIndex]      INT              NOT NULL,
    [StartDate]       DATETIME         NOT NULL,
    [IsCancelled]     BIT              DEFAULT ((0)) NOT NULL,
    [EndDate]         DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [StudentCourseTasks_EndDate] CHECK ([EndDate] IS NULL OR [EndDate] IS NOT NULL AND [StartDate]<[EndDate]),
    FOREIGN KEY ([StudentPersonID]) REFERENCES [Person].[Persons] ([ID]),
    FOREIGN KEY ([TaskTypeID]) REFERENCES [Education].[CourseTaskTypes] ([ID]),
    FOREIGN KEY ([CourseID], [Year]) REFERENCES [Course].[CourseInstances] ([CourseID], [Year]),
    UNIQUE NONCLUSTERED ([CourseID] ASC, [Year] ASC, [StudentPersonID] ASC, [OrderIndex] ASC)
);

