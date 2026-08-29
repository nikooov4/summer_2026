CREATE TABLE [CourseLabs].[TaskCriteria] (
    [ID]                        UNIQUEIDENTIFIER NOT NULL,
    [Name]                      NVARCHAR (256)   NOT NULL,
    [Description]               NVARCHAR (4000)  DEFAULT ('') NOT NULL,
    [OrderIndex]                INT              NOT NULL,
    [MaxScore]                  INT              NOT NULL,
    [MinAcceptScore]            INT              NOT NULL,
    [IsMoreThanMaxScoreAllowed] BIT              DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC)
);

