CREATE TABLE [Organization].[Students] (
    [PersonID]    UNIQUEIDENTIFIER NOT NULL,
    [EdGroupName] NVARCHAR (16)    NOT NULL,
    [StartDate]   DATETIME         NOT NULL,
    [EndDate]     DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([PersonID] ASC, [EdGroupName] ASC, [StartDate] ASC),
    FOREIGN KEY ([EdGroupName]) REFERENCES [Education].[EducationalGroups] ([GroupName]),
    FOREIGN KEY ([PersonID]) REFERENCES [Person].[Persons] ([ID])
);

