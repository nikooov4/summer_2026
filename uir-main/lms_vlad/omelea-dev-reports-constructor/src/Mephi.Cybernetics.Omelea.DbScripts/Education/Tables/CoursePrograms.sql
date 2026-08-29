CREATE TABLE [Education].[CoursePrograms] (
    [ID]                 UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [EdCourseID]         UNIQUEIDENTIFIER NOT NULL,
    [CourseName]         NVARCHAR (256)   NULL,
    [EdProgramID]        UNIQUEIDENTIFIER NOT NULL,
    [LectureHours]       INT              NOT NULL,
    [SeminarHours]       INT              NOT NULL,
    [LabHours]           INT              NOT NULL,
    [OtherPresenceHours] INT              DEFAULT ((0)) NULL,
    [NonAuditorialHours] INT              NULL,
    [IsElective]         BIT              DEFAULT ((0)) NOT NULL,
    [IsOptional]         BIT              DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    FOREIGN KEY ([EdCourseID]) REFERENCES [Education].[Courses] ([ID]),
    FOREIGN KEY ([EdProgramID]) REFERENCES [Education].[EducationalPrograms] ([ID])
);

