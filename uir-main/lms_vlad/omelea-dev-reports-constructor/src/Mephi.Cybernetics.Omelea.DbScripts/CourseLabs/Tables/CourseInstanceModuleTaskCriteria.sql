CREATE TABLE [CourseLabs].[CourseInstanceModuleTaskCriteria] (
    [CourseInstanceModuleTaskID] UNIQUEIDENTIFIER NOT NULL,
    [TaskCriteriaID]             UNIQUEIDENTIFIER NOT NULL,
    [IsMandatory]                BIT              DEFAULT ((1)) NOT NULL,
    PRIMARY KEY CLUSTERED ([CourseInstanceModuleTaskID] ASC, [TaskCriteriaID] ASC),
    FOREIGN KEY ([CourseInstanceModuleTaskID]) REFERENCES [Course].[CourseInstanceModuleTasks] ([ID]),
    FOREIGN KEY ([TaskCriteriaID]) REFERENCES [CourseLabs].[TaskCriteria] ([ID])
);

