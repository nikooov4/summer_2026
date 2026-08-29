CREATE TABLE [Education].[EducationalPrograms] (
    [ID]            UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [ProgramName]   NVARCHAR (512)   NULL,
    [SpecialtyCode] NVARCHAR (16)    NOT NULL,
    [OrgUnitID]     UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt]     DATETIME         NULL,
    [StartDate]     DATETIME         NOT NULL,
    [EndDate]       DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [EducationalPrograms_EndDate] CHECK ([EndDate] IS NULL OR [EndDate] IS NOT NULL AND [StartDate]<[EndDate]),
    FOREIGN KEY ([OrgUnitID]) REFERENCES [Organization].[OrgUnits] ([ID]),
    FOREIGN KEY ([SpecialtyCode]) REFERENCES [Education].[Specialties] ([SpecialtyCode])
);

