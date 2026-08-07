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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806224128_InitialCreate'
)
BEGIN
    CREATE TABLE [ItemTypes] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_ItemTypes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806224128_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [Username] nvarchar(50) NOT NULL,
        [FullName] nvarchar(100) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [IsAdmin] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806224128_InitialCreate'
)
BEGIN
    CREATE TABLE [Items] (
        [Id] int NOT NULL IDENTITY,
        [AssetNumber] float NULL,
        [PhotoUrl] nvarchar(max) NULL,
        [ItemTypeId] int NOT NULL,
        [ModelBrand] nvarchar(100) NOT NULL,
        [AdditionalInfo] nvarchar(200) NULL,
        [AvailabilityStatus] int NOT NULL,
        [Condition] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedByUserId] int NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Items] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Items_ItemTypes_ItemTypeId] FOREIGN KEY ([ItemTypeId]) REFERENCES [ItemTypes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Items_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806224128_InitialCreate'
)
BEGIN
    CREATE TABLE [Loans] (
        [Id] int NOT NULL IDENTITY,
        [ItemId] int NOT NULL,
        [CheckoutDate] datetime2 NOT NULL,
        [ReturnDueDate] datetime2 NULL,
        [Technician] nvarchar(100) NOT NULL,
        [BorrowedBy] nvarchar(100) NOT NULL,
        [ReturnedAt] datetime2 NULL,
        [RegisteredByUserId] int NOT NULL,
        CONSTRAINT [PK_Loans] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Loans_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Loans_Users_RegisteredByUserId] FOREIGN KEY ([RegisteredByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806224128_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[ItemTypes]'))
        SET IDENTITY_INSERT [ItemTypes] ON;
    EXEC(N'INSERT INTO [ItemTypes] ([Id], [Name])
    VALUES (1, N''Phone''),
    (2, N''Phone Base''),
    (3, N''Mono Headset''),
    (4, N''Stereo Headset''),
    (5, N''Mobile Phone''),
    (6, N''HDMI Cables''),
    (7, N''VGA Cables''),
    (8, N''Wired Keyboard''),
    (9, N''Wireless Keyboard''),
    (10, N''Wired Mouse''),
    (11, N''Wireless Mouse''),
    (12, N''Wired Earphones''),
    (13, N''Wireless Earphones''),
    (14, N''HDMI-to-VGA Adapters''),
    (15, N''DisplayPort Adapters''),
    (16, N''Desktop Workstation''),
    (17, N''Monitors''),
    (18, N''Monitor Stand''),
    (19, N''Laptop Stand''),
    (20, N''Headphone Foam Pads''),
    (21, N''Laptops'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[ItemTypes]'))
        SET IDENTITY_INSERT [ItemTypes] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806224128_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Items_CreatedByUserId] ON [Items] ([CreatedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806224128_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Items_ItemTypeId] ON [Items] ([ItemTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806224128_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ItemTypes_Name] ON [ItemTypes] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806224128_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Loans_ItemId] ON [Loans] ([ItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806224128_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Loans_RegisteredByUserId] ON [Loans] ([RegisteredByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806224128_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260806224128_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260806224128_InitialCreate', N'10.0.10');
END;

COMMIT;
GO

