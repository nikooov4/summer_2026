CREATE TABLE [CourseLabs].[StudentVisitDetails] (
    [StudentVisitID]      UNIQUEIDENTIFIER NOT NULL,
    [OrderIndex]          INT              NOT NULL,
    [StudentCourseTaskID] UNIQUEIDENTIFIER NULL,
    [CriterionID]         UNIQUEIDENTIFIER NULL,
    [Score]               INT              NULL,
    PRIMARY KEY CLUSTERED ([StudentVisitID] ASC, [OrderIndex] ASC),
    FOREIGN KEY ([CriterionID]) REFERENCES [CourseLabs].[TaskCriteria] ([ID]),
    FOREIGN KEY ([StudentCourseTaskID]) REFERENCES [CourseLabs].[StudentCourseTasks] ([ID]),
    FOREIGN KEY ([StudentVisitID]) REFERENCES [CourseLabs].[StudentVisits] ([ID])
);

