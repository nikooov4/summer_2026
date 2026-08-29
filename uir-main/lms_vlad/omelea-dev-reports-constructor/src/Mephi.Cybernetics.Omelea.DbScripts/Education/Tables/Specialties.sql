CREATE TABLE [Education].[Specialties] (
    [SpecialtyCode]           NVARCHAR (16)  NOT NULL,
    [SpecialtyCaption]        NVARCHAR (512) NULL,
    [EducationLevel]          INT            NULL,
    [EducationLevelText]      AS             (case [EducationLevel] when (1) then N'бакалавриат' when (2) then N'магистратура' when (3) then N'специалитет' when (4) then N'аспирантура' else '' end) PERSISTED NOT NULL,
    [EducationLevelTextShort] AS             (case [EducationLevel] when (1) then N'бак' when (2) then N'маг' when (3) then N'спец' when (4) then N'асп' else '' end) PERSISTED NOT NULL,
    [StartDate]               DATETIME       NOT NULL,
    [EndDate]                 DATETIME       NULL,
    PRIMARY KEY CLUSTERED ([SpecialtyCode] ASC),
    CHECK ([EducationLevel]>=(1) AND [EducationLevel]<=(4)),
    CONSTRAINT [Specialties_EndDate] CHECK ([EndDate] IS NULL OR [EndDate] IS NOT NULL AND [StartDate]<[EndDate])
);

