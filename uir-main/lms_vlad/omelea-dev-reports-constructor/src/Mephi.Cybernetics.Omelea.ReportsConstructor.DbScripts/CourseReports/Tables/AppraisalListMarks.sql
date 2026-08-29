CREATE TABLE [CourseReport].[AppraisalListMarks] (
    [AppraisalListColumnID] UNIQUEIDENTIFIER NOT NULL,
    [StudentPersonID]       UNIQUEIDENTIFIER NOT NULL,
    [Score]                 INT              NULL, --считаем что NULL это отсутствие оценки, иначе я сойду с ума
    [UpdatedAt]             DATETIME         DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([AppraisalListColumnID] ASC, [StudentPersonID] ASC),
    FOREIGN KEY ([AppraisalListColumnID]) REFERENCES [CourseReport].[AppraisalListColumns] ([ID]) ON DELETE CASCADE,
    FOREIGN KEY ([StudentPersonID]) REFERENCES [Person].[Persons] ([ID]) ON DELETE CASCADE,
);

