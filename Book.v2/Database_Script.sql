IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Books] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(300) NOT NULL,
    [Author] nvarchar(200) NOT NULL,
    [Description] nvarchar(2000) NULL,
    [CoverImageUrl] nvarchar(500) NULL,
    [Genre] nvarchar(100) NOT NULL,
    [Tags] nvarchar(500) NULL,
    [TotalPages] int NOT NULL,
    [ContentFileUrl] nvarchar(500) NULL,
    [PublishedDate] datetime2 NOT NULL,
    [Rating] float NOT NULL,
    CONSTRAINT [PK_Books] PRIMARY KEY ([Id])
);

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [Username] nvarchar(100) NOT NULL,
    [Email] nvarchar(200) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

CREATE TABLE [BookPages] (
    [Id] uniqueidentifier NOT NULL,
    [BookId] uniqueidentifier NOT NULL,
    [PageNumber] int NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_BookPages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BookPages_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ReadingListEntries] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [BookId] uniqueidentifier NOT NULL,
    [AddedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ReadingListEntries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ReadingListEntries_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ReadingListEntries_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ReadingProgresses] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [BookId] uniqueidentifier NOT NULL,
    [CurrentPage] int NOT NULL,
    [ProgressPercentage] float NOT NULL,
    [LastReadAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ReadingProgresses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ReadingProgresses_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ReadingProgresses_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [UserPreferences] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [FavoriteGenres] nvarchar(1000) NOT NULL,
    [FavoriteTags] nvarchar(1000) NOT NULL,
    CONSTRAINT [PK_UserPreferences] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserPreferences_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_BookPages_BookId_PageNumber] ON [BookPages] ([BookId], [PageNumber]);

CREATE INDEX [IX_ReadingListEntries_BookId] ON [ReadingListEntries] ([BookId]);

CREATE UNIQUE INDEX [IX_ReadingListEntries_UserId_BookId] ON [ReadingListEntries] ([UserId], [BookId]);

CREATE INDEX [IX_ReadingProgresses_BookId] ON [ReadingProgresses] ([BookId]);

CREATE UNIQUE INDEX [IX_ReadingProgresses_UserId_BookId] ON [ReadingProgresses] ([UserId], [BookId]);

CREATE UNIQUE INDEX [IX_UserPreferences_UserId] ON [UserPreferences] ([UserId]);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260602214007_InitialSqlServer', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Username');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Users] ALTER COLUMN [Username] nvarchar(max) NOT NULL;

DROP INDEX [IX_Users_Email] ON [Users];
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Email');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Users] ALTER COLUMN [Email] nvarchar(450) NOT NULL;
CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UserPreferences]') AND [c].[name] = N'FavoriteTags');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [UserPreferences] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [UserPreferences] ALTER COLUMN [FavoriteTags] nvarchar(max) NOT NULL;

DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UserPreferences]') AND [c].[name] = N'FavoriteGenres');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [UserPreferences] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [UserPreferences] ALTER COLUMN [FavoriteGenres] nvarchar(max) NOT NULL;

DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Books]') AND [c].[name] = N'Title');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Books] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [Books] ALTER COLUMN [Title] nvarchar(max) NOT NULL;

DECLARE @var5 nvarchar(max);
SELECT @var5 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Books]') AND [c].[name] = N'Tags');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Books] DROP CONSTRAINT ' + @var5 + ';');
ALTER TABLE [Books] ALTER COLUMN [Tags] nvarchar(max) NULL;

DECLARE @var6 nvarchar(max);
SELECT @var6 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Books]') AND [c].[name] = N'Genre');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Books] DROP CONSTRAINT ' + @var6 + ';');
ALTER TABLE [Books] ALTER COLUMN [Genre] nvarchar(max) NOT NULL;

DECLARE @var7 nvarchar(max);
SELECT @var7 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Books]') AND [c].[name] = N'Description');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Books] DROP CONSTRAINT ' + @var7 + ';');
ALTER TABLE [Books] ALTER COLUMN [Description] nvarchar(max) NULL;

DECLARE @var8 nvarchar(max);
SELECT @var8 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Books]') AND [c].[name] = N'CoverImageUrl');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Books] DROP CONSTRAINT ' + @var8 + ';');
ALTER TABLE [Books] ALTER COLUMN [CoverImageUrl] nvarchar(max) NULL;

DECLARE @var9 nvarchar(max);
SELECT @var9 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Books]') AND [c].[name] = N'ContentFileUrl');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Books] DROP CONSTRAINT ' + @var9 + ';');
ALTER TABLE [Books] ALTER COLUMN [ContentFileUrl] nvarchar(max) NULL;

DECLARE @var10 nvarchar(max);
SELECT @var10 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Books]') AND [c].[name] = N'Author');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Books] DROP CONSTRAINT ' + @var10 + ';');
ALTER TABLE [Books] ALTER COLUMN [Author] nvarchar(max) NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260602215147_TurkishCommentsAndRefactor', N'10.0.8');

COMMIT;
GO

