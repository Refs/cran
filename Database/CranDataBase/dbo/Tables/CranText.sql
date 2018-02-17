﻿CREATE TABLE [dbo].[CranText]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[Key] VARCHAR(1000) NOT NULL, 
	[ContentDe] VARCHAR(MAX) NOT NULL, 
    [ContentEn] VARCHAR(MAX) NOT NULL,
	[InsertUser]  VARCHAR (1000) NOT NULL DEFAULT SYSTEM_USER,
    [InsertDate]  DATETIME2 (7)  NOT NULL DEFAULT GETDATE(),
    [UpdateUser]  VARCHAR (1000) NOT NULL DEFAULT SYSTEM_USER,
    [UpdateDate]  DATETIME2 (7)  NOT NULL DEFAULT GETDATE(),   
)

GO

CREATE UNIQUE INDEX [IX_CranText_Key] ON [dbo].[CranText] ([Key])
