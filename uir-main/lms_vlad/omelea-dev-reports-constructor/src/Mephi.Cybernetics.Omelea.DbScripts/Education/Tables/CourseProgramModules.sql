CREATE TABLE [Education].[CourseProgramModules] (
    [ID]              UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [CourseProgramID] UNIQUEIDENTIFIER NOT NULL,
    [ModuleName]      NVARCHAR (256)   NOT NULL,
    [MaxScore]        INT              NOT NULL,
    [MinAcceptScore]  INT              NOT NULL,
    [IsAppraisal]     BIT              DEFAULT ((0)) NOT NULL,
    [OrderIndex]      INT              NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    FOREIGN KEY ([CourseProgramID]) REFERENCES [Education].[CoursePrograms] ([ID])
);

