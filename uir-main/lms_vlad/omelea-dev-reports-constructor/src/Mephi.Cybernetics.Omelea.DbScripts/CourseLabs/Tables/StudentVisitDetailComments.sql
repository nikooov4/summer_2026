CREATE TABLE [CourseLabs].[StudentVisitDetailComments] (
    [ID]             UNIQUEIDENTIFIER NOT NULL,
    [StudentVisitID] UNIQUEIDENTIFIER NOT NULL,
    [OrderIndex]     INT              NOT NULL,
    [Comment]        VARCHAR (2048)   NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    FOREIGN KEY ([StudentVisitID]) REFERENCES [CourseLabs].[StudentVisits] ([ID]),
    UNIQUE NONCLUSTERED ([StudentVisitID] ASC, [OrderIndex] ASC)
);

