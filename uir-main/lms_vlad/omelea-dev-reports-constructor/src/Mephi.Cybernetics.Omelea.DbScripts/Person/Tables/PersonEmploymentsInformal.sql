CREATE TABLE [Person].[PersonEmploymentsInformal] (
    [PersonID]  UNIQUEIDENTIFIER NOT NULL,
    [OrgUnitID] UNIQUEIDENTIFIER NOT NULL,
    [StartDate] DATETIME         NOT NULL,
    [EndDate]   DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([PersonID] ASC, [OrgUnitID] ASC, [StartDate] ASC),
    CONSTRAINT [PersonEmploymentsInformal_EndDate] CHECK ([EndDate] IS NULL OR [EndDate] IS NOT NULL AND [StartDate]<[EndDate]),
    FOREIGN KEY ([PersonID]) REFERENCES [Person].[Persons] ([ID])
);

