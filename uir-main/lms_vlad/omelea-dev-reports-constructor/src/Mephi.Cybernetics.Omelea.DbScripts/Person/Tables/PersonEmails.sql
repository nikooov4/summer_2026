CREATE TABLE [Person].[PersonEmails] (
    [Email]       NVARCHAR (512)   NOT NULL,
    [PersonID]    UNIQUEIDENTIFIER NOT NULL,
    [IsActive]    BIT              DEFAULT ((1)) NOT NULL,
    [IsPreferred] BIT              DEFAULT ((1)) NOT NULL,
    [StartDate]   DATETIME         NOT NULL,
    [EndDate]     DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([Email] ASC),
    CONSTRAINT [PersonEmails_EndDate] CHECK ([EndDate] IS NULL OR [EndDate] IS NOT NULL AND [StartDate]<[EndDate]),
    FOREIGN KEY ([PersonID]) REFERENCES [Person].[Persons] ([ID])
);

