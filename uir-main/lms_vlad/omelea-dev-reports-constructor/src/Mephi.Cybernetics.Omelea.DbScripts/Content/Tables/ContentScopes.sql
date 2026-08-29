CREATE TABLE [Content].[ContentScopes] (
    [ID]       UNIQUEIDENTIFIER NOT NULL,
    [ParentID] UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    FOREIGN KEY ([ParentID]) REFERENCES [Content].[ContentScopes] ([ID])
);

