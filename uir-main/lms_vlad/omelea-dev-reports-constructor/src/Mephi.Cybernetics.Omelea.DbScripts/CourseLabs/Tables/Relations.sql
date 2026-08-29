CREATE TABLE [CourseLabs].[Relations] (
    [ID]          INT             NOT NULL,
    [Name]        NVARCHAR (32)   NULL,
    [Description] NVARCHAR (1024) DEFAULT ('') NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC)
);

