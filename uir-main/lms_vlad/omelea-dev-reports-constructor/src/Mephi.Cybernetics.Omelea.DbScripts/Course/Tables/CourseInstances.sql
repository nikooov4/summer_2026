CREATE TABLE [Course].[CourseInstances] (
    [CourseID]       UNIQUEIDENTIFIER NOT NULL,
    [Year]           INT              NOT NULL,
    [ContentScopeID] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt]      DATETIME         CONSTRAINT [DF__CourseIns__Creat__43D61337] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK__CourseIn__8466A7827F5AA9E6] PRIMARY KEY CLUSTERED ([CourseID] ASC, [Year] ASC),
    CONSTRAINT [FK__CourseIns__Cours__42E1EEFE] FOREIGN KEY ([CourseID]) REFERENCES [Course].[Courses] ([ID])
);

