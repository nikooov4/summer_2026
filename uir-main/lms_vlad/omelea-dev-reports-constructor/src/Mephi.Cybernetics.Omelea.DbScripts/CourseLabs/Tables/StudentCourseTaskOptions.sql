CREATE TABLE [CourseLabs].[StudentCourseTaskOptions] (
    [StudentCourseTaskID] UNIQUEIDENTIFIER NOT NULL,
    [TaskOptionID]        UNIQUEIDENTIFIER NOT NULL,
    [IsStudentSelection]  BIT              DEFAULT ((1)) NULL,
    PRIMARY KEY CLUSTERED ([StudentCourseTaskID] ASC, [TaskOptionID] ASC),
    FOREIGN KEY ([StudentCourseTaskID]) REFERENCES [CourseLabs].[StudentCourseTasks] ([ID]),
    FOREIGN KEY ([TaskOptionID]) REFERENCES [CourseLabs].[CourseTaskOptions] ([ID])
);

