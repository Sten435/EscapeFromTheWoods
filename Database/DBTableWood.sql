CREATE TABLE [dbo].[WoodRecords]
(
	[recordId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [woodID] INT NOT NULL, 
    [treeID] INT NOT NULL, 
    [x] INT NOT NULL, 
    [y] INT NOT NULL
)
