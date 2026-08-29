CREATE TABLE [CourseLabs].[CourseTaskOptionRelations] (
    [TaskOption1ID] UNIQUEIDENTIFIER NOT NULL,
    [TaskOption2ID] UNIQUEIDENTIFIER NOT NULL,
    [RelationID]    INT              NOT NULL,
    PRIMARY KEY CLUSTERED ([TaskOption1ID] ASC, [TaskOption2ID] ASC),
    FOREIGN KEY ([RelationID]) REFERENCES [CourseLabs].[Relations] ([ID]),
    FOREIGN KEY ([TaskOption1ID]) REFERENCES [CourseLabs].[CourseTaskOptions] ([ID]),
    FOREIGN KEY ([TaskOption2ID]) REFERENCES [CourseLabs].[CourseTaskOptions] ([ID])
);

