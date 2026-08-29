CREATE TABLE [CourseLabs].[StudentVisits] (
    [ID]              UNIQUEIDENTIFIER NOT NULL,
    [CourseID]        UNIQUEIDENTIFIER NOT NULL,
    [Year]            INT              NOT NULL,
    [StudentPersonID] UNIQUEIDENTIFIER NOT NULL,
    [VisitDate]       DATETIME         NOT NULL,
    [RegisterDate]    DATETIME         DEFAULT (getdate()) NOT NULL,
    [DataSourceName]  NVARCHAR (2048)  NULL,
    [TeacherPersonID] UNIQUEIDENTIFIER NOT NULL,
    [Comment]         NVARCHAR (4000)  DEFAULT ('') NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    FOREIGN KEY ([StudentPersonID]) REFERENCES [Person].[Persons] ([ID]),
    FOREIGN KEY ([TeacherPersonID]) REFERENCES [Person].[Persons] ([ID])
);

