CREATE TABLE [CourseLabs].[CourseTaskOptions] (
    [ID]                         UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [Name]                       NVARCHAR (256)   NULL,
    [Description]                NVARCHAR (4000)  NULL,
    [TypeCode]                   INT              DEFAULT ((0)) NULL,
    [TypeText]                   AS               (case [TypeCode] when (0) then '??????? ???????' when (1) then '??????-??????????' when (2) then '??????-??????????' when (3) then '?????????' else '' end) PERSISTED NOT NULL,
    [CourseInstanceModuleTaskID] UNIQUEIDENTIFIER NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    CHECK ((0)<=[TypeCode] AND [TypeCode]<=(3)),
    FOREIGN KEY ([CourseInstanceModuleTaskID]) REFERENCES [Course].[CourseInstanceModuleTasks] ([ID])
);

