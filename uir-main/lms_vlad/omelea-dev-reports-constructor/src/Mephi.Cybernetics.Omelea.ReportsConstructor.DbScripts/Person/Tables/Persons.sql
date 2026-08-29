CREATE TABLE [Person].[Persons] (
    [ID]           UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [FirstName]    NVARCHAR (128)   NULL,
    [MiddleName]   NVARCHAR (128)   NULL,
    [LastName]     NVARCHAR (128)   NULL,
    [RegisterDate] DATETIME         NULL,
    [BirthYear]    INT              NULL,
    [BirthMonth]   TINYINT          NULL,
    [BirthDay]     TINYINT          NULL,
    [BirthDate]    DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    CHECK ([BirthDay]>=(1) AND [BirthDay]<=(31)),
    CHECK ([BirthMonth]>=(1) AND [BirthMonth]<=(12))
);

