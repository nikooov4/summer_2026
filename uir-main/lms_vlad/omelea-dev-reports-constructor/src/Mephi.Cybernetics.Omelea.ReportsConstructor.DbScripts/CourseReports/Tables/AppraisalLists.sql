CREATE TABLE [CourseReport].[AppraisalLists] (
    [ID]                UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    -- [Name]            NVARCHAR (256)   NOT NULL,
    [SourceIdentifier]  NVARCHAR(64)     NOT NULL UNIQUE, 
    [DateCreated]       DATETIME         DEFAULT (getdate()) NOT NULL,
    [DateClosed]        DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
);

