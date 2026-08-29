CREATE TABLE [Person].[PersonPhones] (
    [Phone]          NVARCHAR (20)    NOT NULL,
    [AdditionalCode] NVARCHAR (20)    DEFAULT ('') NOT NULL,
    [PersonID]       UNIQUEIDENTIFIER NOT NULL,
    [StartDate]      DATETIME         NOT NULL,
    [EndDate]        DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([Phone] ASC, [AdditionalCode] ASC, [StartDate] ASC),
    CONSTRAINT [PersonPhones_EndDate] CHECK ([EndDate] IS NULL OR [EndDate] IS NOT NULL AND [StartDate]<[EndDate]),
    FOREIGN KEY ([PersonID]) REFERENCES [Person].[Persons] ([ID])
);

