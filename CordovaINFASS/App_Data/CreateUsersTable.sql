-- Run against (localdb)\MSSQLLocalDB in Visual Studio SQL Server Object Explorer
-- if you prefer to create the schema by hand. EF Core also creates this table on first run.

IF DB_ID(N'CordovaINFASS') IS NULL
BEGIN
    CREATE DATABASE [CordovaINFASS];
END
GO

USE [CordovaINFASS];
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id]           INT            IDENTITY (1, 1) NOT NULL,
        [FirstName]    NVARCHAR (50)  NOT NULL,
        [LastName]     NVARCHAR (50)  NOT NULL,
        [Email]        NVARCHAR (256) NOT NULL,
        [PasswordHash] NVARCHAR (256) NOT NULL,
        [Phone]        NVARCHAR (30)  NULL,
        [Role]         NVARCHAR (50)  NOT NULL CONSTRAINT [DF_Users_Role] DEFAULT (N'User'),
        [IsActive]     BIT            NOT NULL CONSTRAINT [DF_Users_IsActive] DEFAULT (1),
        [CreatedAt]    DATETIME2      NOT NULL,
        [UpdatedAt]    DATETIME2      NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [IX_Users_Email] ON [dbo].[Users] ([Email]);
END
GO
