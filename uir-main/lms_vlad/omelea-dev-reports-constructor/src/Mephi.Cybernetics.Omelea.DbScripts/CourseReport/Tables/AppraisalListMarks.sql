CREATE TABLE [CourseReport].[AppraisalListMarks] (
    [AppraisalListColumnID] UNIQUEIDENTIFIER NOT NULL,
    [StudentPersonID]       UNIQUEIDENTIFIER NOT NULL,
    [TeacherPersonID]       UNIQUEIDENTIFIER NOT NULL,
    [Score]                 INT              NOT NULL,
    [ScoreSourceName]       NVARCHAR (2048)  NULL,
    [UpdatedAt]             DATETIME         DEFAULT (getdate()) NOT NULL,
    [OldScore]              INT              NULL,
    [OldTeacherPersonID]    UNIQUEIDENTIFIER NULL,
    [OldScoreSourceName]    NVARCHAR (2048)  NULL,
    PRIMARY KEY CLUSTERED ([AppraisalListColumnID] ASC, [StudentPersonID] ASC),
    FOREIGN KEY ([AppraisalListColumnID]) REFERENCES [CourseReport].[AppraisalListColumns] ([ID]),
    FOREIGN KEY ([OldTeacherPersonID]) REFERENCES [Person].[Persons] ([ID]),
    FOREIGN KEY ([StudentPersonID]) REFERENCES [Person].[Persons] ([ID]),
    FOREIGN KEY ([TeacherPersonID]) REFERENCES [Person].[Persons] ([ID])
);

