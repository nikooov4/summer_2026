CREATE TABLE [CourseLabs].[StudentCourseTaskCriteria] (
    [StudentCourseTaskID] UNIQUEIDENTIFIER NOT NULL,
    [TaskCriteriaID]      UNIQUEIDENTIFIER NOT NULL,
    [IsMandatory]         BIT              DEFAULT ((1)) NOT NULL,
    PRIMARY KEY CLUSTERED ([StudentCourseTaskID] ASC, [TaskCriteriaID] ASC),
    FOREIGN KEY ([StudentCourseTaskID]) REFERENCES [CourseLabs].[StudentCourseTasks] ([ID]),
    FOREIGN KEY ([TaskCriteriaID]) REFERENCES [CourseLabs].[TaskCriteria] ([ID])
);

