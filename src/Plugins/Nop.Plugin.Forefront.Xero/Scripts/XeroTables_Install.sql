IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='XeroAccounting')
BEGIN
	CREATE TABLE [XeroAccounting] (
    [Id] int NOT NULL IDENTITY,
    [NopPaymentMethod] nvarchar(max) NULL,
    [XeroAccountId] nvarchar(max) NULL,
    CONSTRAINT [PK_XeroAccounting] PRIMARY KEY ([Id])
);
END

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='XeroProduct')
BEGIN
	CREATE TABLE [XeroProduct] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [XeroStatus] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
    [InTime] datetime2 NULL,
    [ActionType] nvarchar(max) NULL,
    [XeroId] nvarchar(max) NULL,
	[SyncAttemptCount] int NOT NULL,
    CONSTRAINT [PK_XeroProduct] PRIMARY KEY ([Id])
);
END

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='XeroQueue')
BEGIN
	CREATE TABLE [XeroQueue] (
    [Id] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [IsSuccess] bit NOT NULL,
    [ParentId] int NOT NULL,
    [ActionType] nvarchar(max) NULL,
    [IsPaid] bit NULL,
    [QueuedOn] datetime2 NOT NULL,
    [SyncAttemptOn] datetime2 NULL,
    [SyncAttemptCount] int NOT NULL,
    [Amount] decimal(18, 2) NOT NULL,
    [ResponseMessages] nvarchar(max) NULL,
    [ResponseData] nvarchar(max) NULL,
    [XeroId] nvarchar(max) NULL,
    CONSTRAINT [PK_XeroQueue] PRIMARY KEY ([Id])
);
END

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='XeroAccessRefreshToken')
BEGIN
CREATE TABLE [dbo].[XeroAccessRefreshToken](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AccessToken] [nvarchar](max) NOT NULL,
	[RefreshToken] [nvarchar](max) NOT NULL,
	[IdentityToken] [nvarchar](max) NOT NULL,
	[ExpiresIn] [datetime2](7) NOT NULL,
	[Tenant_Id] [uniqueidentifier] NULL,
	[Tenant_TenantId] [uniqueidentifier] NULL,
	[Tenant_TenantType] [nvarchar](max) NULL,
	[Tenant_CreatedDateUtc] [datetime2](7) NULL,
	[Tenant_UpdatedDateUtc] [datetime2](7) NULL,
 CONSTRAINT [PK_XeroAccessRefreshToken] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END;


--TRUNCATE TABLE XeroProduct;

INSERT INTO XeroProduct (ProductId, XeroStatus, IsDeleted, InTime, ActionType,SyncAttemptCount) SELECT Id, 0, Deleted, GETDATE(), 'Create',0 FROM [Product]