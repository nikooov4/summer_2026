CREATE TABLE [CourseReport].[AppraisalListColumns] (
    [ID]                     UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [Name]                   NVARCHAR (64)    NULL,
    [Description]            NVARCHAR (1024)  DEFAULT ('') NULL,
    [AppraisalListID]        UNIQUEIDENTIFIER NOT NULL,
    [OrderIndex]             INT              NULL,
    [CourseInstanceModuleID] UNIQUEIDENTIFIER NULL,
    [ScaleID]                UNIQUEIDENTIFIER NULL,
    [MaxScore]               INT              NOT NULL,
    [MinAcceptScore]         INT              NOT NULL,
    [Comment]                NVARCHAR (MAX)   DEFAULT ('') NOT NULL,
    [IsComputed]             BIT              DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    FOREIGN KEY ([AppraisalListID]) REFERENCES [CourseReport].[AppraisalLists] ([ID]),
    FOREIGN KEY ([CourseInstanceModuleID]) REFERENCES [Course].[CourseInstanceModules] ([ID]),
    CONSTRAINT [FK_AppraisalListColumns_ScaleID] FOREIGN KEY ([ScaleID]) REFERENCES [CourseReport].[AppraisalScales] ([ID])
);

