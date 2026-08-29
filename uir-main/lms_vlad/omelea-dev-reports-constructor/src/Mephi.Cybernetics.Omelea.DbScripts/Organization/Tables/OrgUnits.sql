CREATE TABLE [Organization].[OrgUnits] (
    [ID]                   UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [UNitName]             NVARCHAR (256)   NOT NULL,
    [UnitCode]             NVARCHAR (16)    NOT NULL,
    [ShortName]            NVARCHAR (128)   NULL,
    [ShortestName]         NVARCHAR (64)    NULL,
    [IsDepartmentGeneral]  BIT              NOT NULL,
    [IsDepartmentGraduate] BIT              NOT NULL,
    [IsDepartment]         AS               ([IsDepartmentGeneral]|[IsDepartmentGraduate]) PERSISTED NOT NULL,
    [IsFaculty]            BIT              DEFAULT ((0)) NOT NULL,
    [IsBranch]             BIT              DEFAULT ((0)) NOT NULL,
    [IsTechnical]          BIT              DEFAULT ((0)) NOT NULL,
    [IsHousehold]          BIT              DEFAULT ((0)) NOT NULL,
    [IsNotEducational]     AS               (~(([IsDepartmentGeneral]|[IsDepartmentGraduate])|[IsFaculty])) PERSISTED NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC)
);

