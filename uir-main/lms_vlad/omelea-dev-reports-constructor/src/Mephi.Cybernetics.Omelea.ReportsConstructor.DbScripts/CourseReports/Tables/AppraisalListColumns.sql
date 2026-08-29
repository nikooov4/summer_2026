CREATE TABLE [CourseReport].[AppraisalListColumns] (
    [ID]                     UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [SourceIdentifier]       NVARCHAR(64)  NOT NULL,
    [AppraisalListID]        UNIQUEIDENTIFIER NOT NULL,
    -- [Name]                   NVARCHAR (64)    UNIQUE NOT NULL,
    -- [OrderIndex]             INT              NULL,
    -- [ScaleID]                UNIQUEIDENTIFIER NULL,
    [IsComputed]             BIT              DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    FOREIGN KEY ([AppraisalListID]) REFERENCES [CourseReport].[AppraisalLists] ([ID]),
    -- CONSTRAINT [FK_AppraisalListColumns_ScaleID] FOREIGN KEY ([ScaleID]) REFERENCES [CourseReport].[AppraisalScales] ([ID])
);

