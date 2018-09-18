CREATE TABLE SemaphoreTokens (
    Id          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Name]      NVARCHAR(50) NOT NULL,
    [TimeStamp] BIGINT NOT NULL,
    Usage       INT NOT NULL
);
GO

CREATE INDEX IX_SemaphoreTokens_Name_TimeStamp ON SemaphoreTokens([Name] ASC, [TimeStamp] ASC);
GO