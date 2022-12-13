CREATE TABLE [dbo].[MonkeyRecords]
(
	[recordID] INT NOT NULL PRIMARY KEY IDENTITY(1,1), 
    [monkeyID] INT NOT NULL, 
    [monkeyName] NVARCHAR(50) NOT NULL, 
    [woodID] INT NOT NULL, 
    [seqnr] INT NOT NULL, 
    [treeID] INT NOT NULL, 
    [x] INT NOT NULL, 
    [y] INT NOT NULL
)
