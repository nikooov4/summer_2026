CREATE TABLE [Education].[CourseTaskTypes] (
    [ID]           INT            NOT NULL,
    [Name]         NVARCHAR (128) NOT NULL,
    [ShortName]    NVARCHAR (20)  NOT NULL,
    [AltShortName] NVARCHAR (20)  NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC)
);

