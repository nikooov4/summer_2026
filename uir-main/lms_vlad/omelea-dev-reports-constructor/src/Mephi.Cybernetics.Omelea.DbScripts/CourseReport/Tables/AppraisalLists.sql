CREATE TABLE [CourseReport].[AppraisalLists] (
    [ID]              UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [Name]            NVARCHAR (256)   NOT NULL,
    [Description]     NVARCHAR (2048)  DEFAULT ('') NOT NULL,
    [CourseID]        UNIQUEIDENTIFIER NOT NULL,
    [Year]            INT              NOT NULL,
    [CourseProgramID] UNIQUEIDENTIFIER NULL,
    [DateCreated]     DATETIME         DEFAULT (getdate()) NOT NULL,
    [DateClosed]      DATETIME         NULL,
    [Comment]         NVARCHAR (MAX)   DEFAULT ('') NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    FOREIGN KEY ([CourseProgramID]) REFERENCES [Education].[CoursePrograms] ([ID]),
    FOREIGN KEY ([CourseID], [Year]) REFERENCES [Course].[CourseInstances] ([CourseID], [Year])
);

