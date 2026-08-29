CREATE TABLE [Person].[PersonEmployments] (
    [PersonID]   UNIQUEIDENTIFIER NOT NULL,
    [PositionID] UNIQUEIDENTIFIER NOT NULL,
    [OrgUnitID]  UNIQUEIDENTIFIER NOT NULL,
    [IsMain]     BIT              DEFAULT ((1)) NOT NULL,
    [StartDate]  DATETIME         NOT NULL,
    [EndDate]    DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([PersonID] ASC, [PositionID] ASC, [OrgUnitID] ASC, [StartDate] ASC),
    CONSTRAINT [PersonEmployments_EndDate] CHECK ([EndDate] IS NULL OR [EndDate] IS NOT NULL AND [StartDate]<[EndDate]),
    FOREIGN KEY ([PersonID]) REFERENCES [Person].[Persons] ([ID])
);

