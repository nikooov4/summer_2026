CREATE TABLE [CourseReport].[AppraisalScales] (
    [ID]          UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [Name]        NVARCHAR (128)   NOT NULL,
    [Description] NVARCHAR (1024)  DEFAULT ('') NOT NULL,
    [IsEnum]      BIT              NOT NULL,
    [IsLinear]    BIT              NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    CHECK ([IsEnum]=(1) AND [IsLinear]=(0) OR [IsEnum]=(0) AND [IsLinear]=(1))
);

