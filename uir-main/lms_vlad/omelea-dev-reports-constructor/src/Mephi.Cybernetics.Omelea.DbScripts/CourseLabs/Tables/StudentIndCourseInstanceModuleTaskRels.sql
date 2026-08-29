CREATE TABLE [CourseLabs].[StudentIndCourseInstanceModuleTaskRels] (
    [StudentCourseTaskID]        UNIQUEIDENTIFIER NOT NULL,
    [CourseInstanceModuleTaskID] UNIQUEIDENTIFIER NOT NULL,
    [RelationID]                 INT              NOT NULL,
    FOREIGN KEY ([CourseInstanceModuleTaskID]) REFERENCES [Course].[CourseInstanceModuleTasks] ([ID]),
    FOREIGN KEY ([RelationID]) REFERENCES [CourseLabs].[Relations] ([ID]),
    FOREIGN KEY ([StudentCourseTaskID]) REFERENCES [CourseLabs].[StudentCourseTasks] ([ID])
);

