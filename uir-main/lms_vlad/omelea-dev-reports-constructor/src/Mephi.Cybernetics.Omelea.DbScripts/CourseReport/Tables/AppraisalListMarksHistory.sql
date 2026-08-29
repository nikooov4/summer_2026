CREATE TABLE [CourseReport].[AppraisalListMarksHistory] (
    [AppraisalListColumnID] UNIQUEIDENTIFIER NOT NULL,
    [StudentPersonID]       UNIQUEIDENTIFIER NOT NULL,
    [TeacherPersonID]       UNIQUEIDENTIFIER NOT NULL,
    [Score]                 INT              NOT NULL,
    [ScoreSourceName]       NVARCHAR (2048)  NULL,
    [StartDate]             DATETIME         DEFAULT (getdate()) NOT NULL,
    [EndDate]               DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([AppraisalListColumnID] ASC, [StudentPersonID] ASC, [StartDate] ASC),
    CONSTRAINT [AppraisalListMarksHistory_EndDate] CHECK ([EndDate] IS NULL OR [EndDate] IS NOT NULL AND [StartDate]<[EndDate]),
    FOREIGN KEY ([AppraisalListColumnID]) REFERENCES [CourseReport].[AppraisalListColumns] ([ID]),
    FOREIGN KEY ([StudentPersonID]) REFERENCES [Person].[Persons] ([ID]),
    FOREIGN KEY ([TeacherPersonID]) REFERENCES [Person].[Persons] ([ID])
);

