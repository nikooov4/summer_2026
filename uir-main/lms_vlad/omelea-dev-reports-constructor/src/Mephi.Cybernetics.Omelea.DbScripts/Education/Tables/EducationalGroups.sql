CREATE TABLE [Education].[EducationalGroups] (
    [GroupName]     NVARCHAR (16)    NOT NULL,
    [GroupCaption]  NVARCHAR (32)    NULL,
    [EdProgramID]   UNIQUEIDENTIFIER NOT NULL,
    [AdmissionYear] INT              NOT NULL,
    [StartDate]     DATETIME         NOT NULL,
    [EndDate]       DATETIME         NULL,
    PRIMARY KEY CLUSTERED ([GroupName] ASC),
    CONSTRAINT [EducationalGroups_EndDate] CHECK ([EndDate] IS NULL OR [EndDate] IS NOT NULL AND [StartDate]<[EndDate]),
    FOREIGN KEY ([EdProgramID]) REFERENCES [Education].[EducationalPrograms] ([ID])
);


GO
CREATE TRIGGER Education.EducationalGroups_AfterInsert
ON Education.EducationalGroups
AFTER INSERT
AS
BEGIN
	UPDATE g
	SET GroupCaption = GroupName
	FROM Education.EducationalGroup g
		INNER JOIN inserted
			on g.GroupName = inserted.GroupName AND i.GroupCaption IS NULL
	;
END