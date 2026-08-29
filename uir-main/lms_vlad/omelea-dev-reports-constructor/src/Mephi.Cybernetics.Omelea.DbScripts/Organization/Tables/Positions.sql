CREATE TABLE [Organization].[Positions] (
    [ID]               UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [PositionName]     NVARCHAR (128)   NOT NULL,
    [ShortName]        NVARCHAR (64)    NULL,
    [ShortestName]     NVARCHAR (64)    NULL,
    [IsTeacher]        BIT              NOT NULL,
    [IsAdministrative] BIT              DEFAULT ((0)) NOT NULL,
    [IsTeachingSuppl]  BIT              DEFAULT ((0)) NOT NULL,
    [IsTechnical]      BIT              DEFAULT ((0)) NOT NULL,
    [IsHousehold]      BIT              DEFAULT ((0)) NOT NULL,
    [IsHead]           BIT              DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC)
);

