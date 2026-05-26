SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.InventoryLocation', N'U') IS NULL
CREATE TABLE [dbo].[InventoryLocation]
(
    [LocationId] INT IDENTITY(1,1) NOT NULL,
    [LocationCode] NVARCHAR(50) NOT NULL,
    [LocationName] NVARCHAR(150) NOT NULL,
    [Description] NVARCHAR(500) NOT NULL CONSTRAINT [DF_InventoryLocation_Description] DEFAULT (N''),
    [IsDefault] BIT NOT NULL CONSTRAINT [DF_InventoryLocation_IsDefault] DEFAULT (0),
    [IsActive] BIT NOT NULL CONSTRAINT [DF_InventoryLocation_IsActive] DEFAULT (1),
    [CreatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_InventoryLocation_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [UpdatedByUserId] INT NULL,
    [UpdatedDate] DATETIME2(0) NULL,
    CONSTRAINT [PK_InventoryLocation] PRIMARY KEY CLUSTERED ([LocationId]),
    CONSTRAINT [UQ_InventoryLocation_LocationCode] UNIQUE ([LocationCode])
);
GO

IF OBJECT_ID(N'dbo.InventoryStock', N'U') IS NULL
CREATE TABLE [dbo].[InventoryStock]
(
    [InventoryStockId] BIGINT IDENTITY(1,1) NOT NULL,
    [ProductId] INT NOT NULL,
    [LocationId] INT NOT NULL,
    [CurrentStock] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_InventoryStock_CurrentStock] DEFAULT (0),
    [LastMovementDate] DATETIME2(0) NULL,
    [CreatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_InventoryStock_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [UpdatedByUserId] INT NULL,
    [UpdatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_InventoryStock_UpdatedDate] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_InventoryStock] PRIMARY KEY CLUSTERED ([InventoryStockId]),
    CONSTRAINT [UQ_InventoryStock_Product_Location] UNIQUE ([ProductId], [LocationId]),
    CONSTRAINT [FK_InventoryStock_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([ProductId]),
    CONSTRAINT [FK_InventoryStock_Location] FOREIGN KEY ([LocationId]) REFERENCES [dbo].[InventoryLocation] ([LocationId])
);
GO

IF OBJECT_ID(N'dbo.InventoryMovement', N'U') IS NULL
CREATE TABLE [dbo].[InventoryMovement]
(
    [InventoryMovementId] BIGINT IDENTITY(1,1) NOT NULL,
    [ProductId] INT NOT NULL,
    [LocationId] INT NOT NULL,
    [MovementType] NVARCHAR(30) NOT NULL,
    [Quantity] DECIMAL(18,4) NOT NULL,
    [QuantitySigned] DECIMAL(18,4) NOT NULL,
    [UnitCost] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_InventoryMovement_UnitCost] DEFAULT (0),
    [StockBefore] DECIMAL(18,4) NOT NULL,
    [StockAfter] DECIMAL(18,4) NOT NULL,
    [ReferenceType] NVARCHAR(50) NOT NULL CONSTRAINT [DF_InventoryMovement_ReferenceType] DEFAULT (N''),
    [ReferenceId] BIGINT NULL,
    [ReferenceNo] NVARCHAR(100) NOT NULL CONSTRAINT [DF_InventoryMovement_ReferenceNo] DEFAULT (N''),
    [Reason] NVARCHAR(500) NOT NULL CONSTRAINT [DF_InventoryMovement_Reason] DEFAULT (N''),
    [Remarks] NVARCHAR(1000) NOT NULL CONSTRAINT [DF_InventoryMovement_Remarks] DEFAULT (N''),
    [IsActive] BIT NOT NULL CONSTRAINT [DF_InventoryMovement_IsActive] DEFAULT (1),
    [CreatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_InventoryMovement_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_InventoryMovement] PRIMARY KEY CLUSTERED ([InventoryMovementId]),
    CONSTRAINT [FK_InventoryMovement_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([ProductId]),
    CONSTRAINT [FK_InventoryMovement_Location] FOREIGN KEY ([LocationId]) REFERENCES [dbo].[InventoryLocation] ([LocationId]),
    CONSTRAINT [CK_InventoryMovement_MovementType] CHECK ([MovementType] IN (N'StockIn',N'StockOut',N'Sale',N'Return',N'PurchaseReceive',N'AdjustmentIn',N'AdjustmentOut',N'TransferIn',N'TransferOut',N'StockCountCorrection')),
    CONSTRAINT [CK_InventoryMovement_QuantityPositive] CHECK ([Quantity] > 0)
);
GO

IF OBJECT_ID(N'dbo.StockAdjustment', N'U') IS NULL
CREATE TABLE [dbo].[StockAdjustment]
(
    [StockAdjustmentId] BIGINT IDENTITY(1,1) NOT NULL,
    [AdjustmentNo] NVARCHAR(50) NOT NULL,
    [LocationId] INT NOT NULL,
    [Status] NVARCHAR(20) NOT NULL CONSTRAINT [DF_StockAdjustment_Status] DEFAULT (N'Draft'),
    [Reason] NVARCHAR(500) NOT NULL,
    [Remarks] NVARCHAR(1000) NOT NULL CONSTRAINT [DF_StockAdjustment_Remarks] DEFAULT (N''),
    [CreatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_StockAdjustment_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [UpdatedByUserId] INT NULL,
    [UpdatedDate] DATETIME2(0) NULL,
    CONSTRAINT [PK_StockAdjustment] PRIMARY KEY CLUSTERED ([StockAdjustmentId]),
    CONSTRAINT [UQ_StockAdjustment_AdjustmentNo] UNIQUE ([AdjustmentNo]),
    CONSTRAINT [FK_StockAdjustment_Location] FOREIGN KEY ([LocationId]) REFERENCES [dbo].[InventoryLocation] ([LocationId]),
    CONSTRAINT [CK_StockAdjustment_Status] CHECK ([Status] IN (N'Draft',N'Approved',N'Rejected',N'Cancelled')),
    CONSTRAINT [CK_StockAdjustment_Reason] CHECK (LEN(LTRIM(RTRIM([Reason]))) > 0)
);
GO

IF OBJECT_ID(N'dbo.StockAdjustmentItem', N'U') IS NULL
CREATE TABLE [dbo].[StockAdjustmentItem]
(
    [StockAdjustmentItemId] BIGINT IDENTITY(1,1) NOT NULL,
    [StockAdjustmentId] BIGINT NOT NULL,
    [ProductId] INT NOT NULL,
    [Quantity] DECIMAL(18,4) NOT NULL,
    [AdjustmentType] NVARCHAR(20) NOT NULL,
    [UnitCost] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_StockAdjustmentItem_UnitCost] DEFAULT (0),
    [Reason] NVARCHAR(500) NOT NULL,
    CONSTRAINT [PK_StockAdjustmentItem] PRIMARY KEY CLUSTERED ([StockAdjustmentItemId]),
    CONSTRAINT [FK_StockAdjustmentItem_Adjustment] FOREIGN KEY ([StockAdjustmentId]) REFERENCES [dbo].[StockAdjustment] ([StockAdjustmentId]),
    CONSTRAINT [FK_StockAdjustmentItem_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([ProductId]),
    CONSTRAINT [CK_StockAdjustmentItem_AdjustmentType] CHECK ([AdjustmentType] IN (N'Increase',N'Decrease')),
    CONSTRAINT [CK_StockAdjustmentItem_QuantityPositive] CHECK ([Quantity] > 0),
    CONSTRAINT [CK_StockAdjustmentItem_Reason] CHECK (LEN(LTRIM(RTRIM([Reason]))) > 0)
);
GO

IF OBJECT_ID(N'dbo.StockCount', N'U') IS NULL
CREATE TABLE [dbo].[StockCount]
(
    [StockCountId] BIGINT IDENTITY(1,1) NOT NULL,
    [StockCountNo] NVARCHAR(50) NOT NULL,
    [LocationId] INT NOT NULL,
    [Status] NVARCHAR(20) NOT NULL CONSTRAINT [DF_StockCount_Status] DEFAULT (N'Draft'),
    [Remarks] NVARCHAR(1000) NOT NULL CONSTRAINT [DF_StockCount_Remarks] DEFAULT (N''),
    [CreatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_StockCount_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [UpdatedByUserId] INT NULL,
    [UpdatedDate] DATETIME2(0) NULL,
    CONSTRAINT [PK_StockCount] PRIMARY KEY CLUSTERED ([StockCountId]),
    CONSTRAINT [UQ_StockCount_StockCountNo] UNIQUE ([StockCountNo]),
    CONSTRAINT [FK_StockCount_Location] FOREIGN KEY ([LocationId]) REFERENCES [dbo].[InventoryLocation] ([LocationId]),
    CONSTRAINT [CK_StockCount_Status] CHECK ([Status] IN (N'Draft',N'Approved',N'Cancelled'))
);
GO

IF OBJECT_ID(N'dbo.StockCountItem', N'U') IS NULL
CREATE TABLE [dbo].[StockCountItem]
(
    [StockCountItemId] BIGINT IDENTITY(1,1) NOT NULL,
    [StockCountId] BIGINT NOT NULL,
    [ProductId] INT NOT NULL,
    [SystemQty] DECIMAL(18,4) NOT NULL,
    [CountedQty] DECIMAL(18,4) NOT NULL,
    [VarianceQty] AS ([CountedQty] - [SystemQty]) PERSISTED,
    [Remarks] NVARCHAR(1000) NOT NULL CONSTRAINT [DF_StockCountItem_Remarks] DEFAULT (N''),
    CONSTRAINT [PK_StockCountItem] PRIMARY KEY CLUSTERED ([StockCountItemId]),
    CONSTRAINT [UQ_StockCountItem_Count_Product] UNIQUE ([StockCountId], [ProductId]),
    CONSTRAINT [FK_StockCountItem_Count] FOREIGN KEY ([StockCountId]) REFERENCES [dbo].[StockCount] ([StockCountId]),
    CONSTRAINT [FK_StockCountItem_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([ProductId]),
    CONSTRAINT [CK_StockCountItem_CountedQtyNonNegative] CHECK ([CountedQty] >= 0)
);
GO

IF OBJECT_ID(N'dbo.StockTransfer', N'U') IS NULL
CREATE TABLE [dbo].[StockTransfer]
(
    [StockTransferId] BIGINT IDENTITY(1,1) NOT NULL,
    [TransferNo] NVARCHAR(50) NOT NULL,
    [SourceLocationId] INT NOT NULL,
    [DestinationLocationId] INT NOT NULL,
    [Status] NVARCHAR(20) NOT NULL CONSTRAINT [DF_StockTransfer_Status] DEFAULT (N'Draft'),
    [Remarks] NVARCHAR(1000) NOT NULL CONSTRAINT [DF_StockTransfer_Remarks] DEFAULT (N''),
    [SentDate] DATETIME2(0) NULL,
    [ReceivedDate] DATETIME2(0) NULL,
    [CreatedByUserId] INT NULL,
    [CreatedDate] DATETIME2(0) NOT NULL CONSTRAINT [DF_StockTransfer_CreatedDate] DEFAULT (SYSUTCDATETIME()),
    [UpdatedByUserId] INT NULL,
    [UpdatedDate] DATETIME2(0) NULL,
    CONSTRAINT [PK_StockTransfer] PRIMARY KEY CLUSTERED ([StockTransferId]),
    CONSTRAINT [UQ_StockTransfer_TransferNo] UNIQUE ([TransferNo]),
    CONSTRAINT [FK_StockTransfer_SourceLocation] FOREIGN KEY ([SourceLocationId]) REFERENCES [dbo].[InventoryLocation] ([LocationId]),
    CONSTRAINT [FK_StockTransfer_DestinationLocation] FOREIGN KEY ([DestinationLocationId]) REFERENCES [dbo].[InventoryLocation] ([LocationId]),
    CONSTRAINT [CK_StockTransfer_Status] CHECK ([Status] IN (N'Draft',N'Sent',N'Received',N'Cancelled')),
    CONSTRAINT [CK_StockTransfer_DifferentLocations] CHECK ([SourceLocationId] <> [DestinationLocationId])
);
GO

IF OBJECT_ID(N'dbo.StockTransferItem', N'U') IS NULL
CREATE TABLE [dbo].[StockTransferItem]
(
    [StockTransferItemId] BIGINT IDENTITY(1,1) NOT NULL,
    [StockTransferId] BIGINT NOT NULL,
    [ProductId] INT NOT NULL,
    [Quantity] DECIMAL(18,4) NOT NULL,
    [UnitCost] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_StockTransferItem_UnitCost] DEFAULT (0),
    CONSTRAINT [PK_StockTransferItem] PRIMARY KEY CLUSTERED ([StockTransferItemId]),
    CONSTRAINT [UQ_StockTransferItem_Transfer_Product] UNIQUE ([StockTransferId], [ProductId]),
    CONSTRAINT [FK_StockTransferItem_Transfer] FOREIGN KEY ([StockTransferId]) REFERENCES [dbo].[StockTransfer] ([StockTransferId]),
    CONSTRAINT [FK_StockTransferItem_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([ProductId]),
    CONSTRAINT [CK_StockTransferItem_QuantityPositive] CHECK ([Quantity] > 0)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_InventoryStock_Location' AND object_id = OBJECT_ID(N'dbo.InventoryStock')) CREATE INDEX [IX_InventoryStock_Location] ON [dbo].[InventoryStock] ([LocationId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_InventoryMovement_Product_Date' AND object_id = OBJECT_ID(N'dbo.InventoryMovement')) CREATE INDEX [IX_InventoryMovement_Product_Date] ON [dbo].[InventoryMovement] ([ProductId], [CreatedDate] DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_InventoryMovement_Location_Date' AND object_id = OBJECT_ID(N'dbo.InventoryMovement')) CREATE INDEX [IX_InventoryMovement_Location_Date] ON [dbo].[InventoryMovement] ([LocationId], [CreatedDate] DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_InventoryMovement_ReferenceNo' AND object_id = OBJECT_ID(N'dbo.InventoryMovement')) CREATE INDEX [IX_InventoryMovement_ReferenceNo] ON [dbo].[InventoryMovement] ([ReferenceNo]) WHERE [ReferenceNo] <> N'';
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StockAdjustment_Status' AND object_id = OBJECT_ID(N'dbo.StockAdjustment')) CREATE INDEX [IX_StockAdjustment_Status] ON [dbo].[StockAdjustment] ([Status], [CreatedDate] DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StockCount_Status' AND object_id = OBJECT_ID(N'dbo.StockCount')) CREATE INDEX [IX_StockCount_Status] ON [dbo].[StockCount] ([Status], [CreatedDate] DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StockTransfer_Status' AND object_id = OBJECT_ID(N'dbo.StockTransfer')) CREATE INDEX [IX_StockTransfer_Status] ON [dbo].[StockTransfer] ([Status], [CreatedDate] DESC);
GO

MERGE [dbo].[InventoryLocation] AS target
USING (VALUES (N'MAIN', N'Main Store', N'Default selling and stock location', CONVERT(bit, 1))) AS source(LocationCode, LocationName, Description, IsDefault)
ON target.LocationCode = source.LocationCode
WHEN NOT MATCHED THEN
    INSERT (LocationCode, LocationName, Description, IsDefault, IsActive)
    VALUES (source.LocationCode, source.LocationName, source.Description, source.IsDefault, 1);
GO

CREATE OR ALTER PROCEDURE [dbo].[spInventoryMovementCreate]
    @ProductId INT,
    @LocationId INT,
    @MovementType NVARCHAR(30),
    @Quantity DECIMAL(18,4),
    @UnitCost DECIMAL(18,4) = 0,
    @ReferenceType NVARCHAR(50) = N'',
    @ReferenceId BIGINT = NULL,
    @ReferenceNo NVARCHAR(100) = N'',
    @Reason NVARCHAR(500) = N'',
    @Remarks NVARCHAR(1000) = N'',
    @AllowNegativeStock BIT = 0,
    @IsDecrease BIT = 0,
    @CreatedByUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Sign INT = CASE WHEN @MovementType IN (N'StockOut',N'Sale',N'AdjustmentOut',N'TransferOut') OR (@MovementType = N'StockCountCorrection' AND @IsDecrease = 1) THEN -1 ELSE 1 END;
    DECLARE @SignedQty DECIMAL(18,4) = @Quantity * @Sign;
    DECLARE @Before DECIMAL(18,4);
    DECLARE @After DECIMAL(18,4);
    DECLARE @MovementId BIGINT;

    IF @Quantity <= 0 THROW 51000, 'Quantity must be greater than zero.', 1;
    IF @MovementType NOT IN (N'StockIn',N'StockOut',N'Sale',N'Return',N'PurchaseReceive',N'AdjustmentIn',N'AdjustmentOut',N'TransferIn',N'TransferOut',N'StockCountCorrection') THROW 51001, 'Invalid inventory movement type.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM [dbo].[InventoryStock] WITH (UPDLOCK, HOLDLOCK) WHERE ProductId = @ProductId AND LocationId = @LocationId)
        BEGIN
            INSERT INTO [dbo].[InventoryStock] (ProductId, LocationId, CurrentStock, CreatedByUserId, UpdatedByUserId)
            VALUES (@ProductId, @LocationId, 0, NULLIF(@CreatedByUserId, 0), NULLIF(@CreatedByUserId, 0));
        END

        SELECT @Before = CurrentStock
        FROM [dbo].[InventoryStock] WITH (UPDLOCK, HOLDLOCK)
        WHERE ProductId = @ProductId AND LocationId = @LocationId;

        SET @After = @Before + @SignedQty;
        IF @After < 0 AND @AllowNegativeStock = 0 THROW 51002, 'Insufficient stock for this movement.', 1;

        INSERT INTO [dbo].[InventoryMovement] (ProductId, LocationId, MovementType, Quantity, QuantitySigned, UnitCost, StockBefore, StockAfter, ReferenceType, ReferenceId, ReferenceNo, Reason, Remarks, CreatedByUserId)
        VALUES (@ProductId, @LocationId, @MovementType, @Quantity, @SignedQty, ISNULL(@UnitCost, 0), @Before, @After, ISNULL(@ReferenceType, N''), @ReferenceId, ISNULL(@ReferenceNo, N''), ISNULL(@Reason, N''), ISNULL(@Remarks, N''), NULLIF(@CreatedByUserId, 0));

        SET @MovementId = SCOPE_IDENTITY();

        UPDATE [dbo].[InventoryStock]
        SET CurrentStock = @After,
            LastMovementDate = SYSUTCDATETIME(),
            UpdatedByUserId = NULLIF(@CreatedByUserId, 0),
            UpdatedDate = SYSUTCDATETIME()
        WHERE ProductId = @ProductId AND LocationId = @LocationId;

        COMMIT TRANSACTION;
        SELECT @MovementId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spInventoryLocationGetAll] AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM [dbo].[InventoryLocation] ORDER BY IsDefault DESC, LocationName;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spInventoryLocationGetAllActive] AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM [dbo].[InventoryLocation] WHERE IsActive = 1 ORDER BY IsDefault DESC, LocationName;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spInventoryLocationGetById] @LocationId INT AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM [dbo].[InventoryLocation] WHERE LocationId = @LocationId;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spInventoryLocationCreate]
    @LocationCode NVARCHAR(50), @LocationName NVARCHAR(150), @Description NVARCHAR(500) = N'', @IsDefault BIT = 0, @CreatedByUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF @IsDefault = 1 UPDATE [dbo].[InventoryLocation] SET IsDefault = 0;
        INSERT INTO [dbo].[InventoryLocation] (LocationCode, LocationName, Description, IsDefault, CreatedByUserId)
        VALUES (@LocationCode, @LocationName, ISNULL(@Description, N''), @IsDefault, NULLIF(@CreatedByUserId, 0));
        DECLARE @LocationId INT = SCOPE_IDENTITY();
        COMMIT TRANSACTION;
        SELECT @LocationId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spInventoryLocationUpdate]
    @LocationId INT, @LocationCode NVARCHAR(50), @LocationName NVARCHAR(150), @Description NVARCHAR(500) = N'', @IsDefault BIT, @IsActive BIT, @UpdatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF @IsDefault = 1 UPDATE [dbo].[InventoryLocation] SET IsDefault = 0 WHERE LocationId <> @LocationId;
        UPDATE [dbo].[InventoryLocation]
        SET LocationCode = @LocationCode, LocationName = @LocationName, Description = ISNULL(@Description, N''), IsDefault = @IsDefault, IsActive = @IsActive, UpdatedByUserId = NULLIF(@UpdatedByUserId, 0), UpdatedDate = SYSUTCDATETIME()
        WHERE LocationId = @LocationId;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spInventoryLocationToggleActive] @LocationId INT, @IsActive BIT, @UpdatedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[InventoryLocation] SET IsActive = @IsActive, UpdatedByUserId = NULLIF(@UpdatedByUserId, 0), UpdatedDate = SYSUTCDATETIME() WHERE LocationId = @LocationId;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spInventoryStockGetPaged]
    @PageNumber INT = 1, @PageSize INT = 20, @SearchText NVARCHAR(200) = NULL, @LocationId INT = NULL, @CategoryId INT = NULL, @ActiveProductsOnly BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH q AS (
        SELECT s.InventoryStockId, p.ProductId, p.ProductCode, p.ProductName, s.LocationId, l.LocationCode, l.LocationName, s.CurrentStock, p.MinimumStockLevel,
               CAST(CASE WHEN s.CurrentStock <= p.MinimumStockLevel THEN 1 ELSE 0 END AS BIT) IsLowStock,
               CAST(CASE WHEN s.CurrentStock <= 0 THEN 1 ELSE 0 END AS BIT) IsOutOfStock,
               ISNULL(s.LastMovementDate, s.UpdatedDate) LastMovementDate, s.UpdatedDate
        FROM dbo.InventoryStock s
        JOIN dbo.Product p ON p.ProductId = s.ProductId
        JOIN dbo.InventoryLocation l ON l.LocationId = s.LocationId
        WHERE (@LocationId IS NULL OR s.LocationId = @LocationId)
          AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
          AND (@ActiveProductsOnly = 0 OR p.IsActive = 1)
          AND (@SearchText IS NULL OR p.ProductCode LIKE N'%' + @SearchText + N'%' OR p.ProductName LIKE N'%' + @SearchText + N'%' OR l.LocationName LIKE N'%' + @SearchText + N'%')
    )
    SELECT *, COUNT(1) OVER() TotalCount FROM q ORDER BY ProductName OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spInventoryStockGetByProductId] @ProductId INT AS
BEGIN
    SET NOCOUNT ON;
    SELECT s.InventoryStockId, p.ProductId, p.ProductCode, p.ProductName, s.LocationId, l.LocationCode, l.LocationName, s.CurrentStock, p.MinimumStockLevel,
           CAST(CASE WHEN s.CurrentStock <= p.MinimumStockLevel THEN 1 ELSE 0 END AS BIT) IsLowStock,
           CAST(CASE WHEN s.CurrentStock <= 0 THEN 1 ELSE 0 END AS BIT) IsOutOfStock,
           ISNULL(s.LastMovementDate, s.UpdatedDate) LastMovementDate, s.UpdatedDate
    FROM dbo.InventoryStock s JOIN dbo.Product p ON p.ProductId = s.ProductId JOIN dbo.InventoryLocation l ON l.LocationId = s.LocationId
    WHERE s.ProductId = @ProductId ORDER BY l.LocationName;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spInventoryStockGetLowStockPaged]
    @PageNumber INT = 1, @PageSize INT = 20, @SearchText NVARCHAR(200) = NULL, @LocationId INT = NULL, @CategoryId INT = NULL, @ActiveProductsOnly BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH q AS (
        SELECT s.InventoryStockId, p.ProductId, p.ProductCode, p.ProductName, s.LocationId, l.LocationCode, l.LocationName, s.CurrentStock, p.MinimumStockLevel,
               CAST(1 AS BIT) IsLowStock, CAST(CASE WHEN s.CurrentStock <= 0 THEN 1 ELSE 0 END AS BIT) IsOutOfStock,
               ISNULL(s.LastMovementDate, s.UpdatedDate) LastMovementDate, s.UpdatedDate
        FROM dbo.InventoryStock s JOIN dbo.Product p ON p.ProductId = s.ProductId JOIN dbo.InventoryLocation l ON l.LocationId = s.LocationId
        WHERE s.CurrentStock <= p.MinimumStockLevel AND (@LocationId IS NULL OR s.LocationId = @LocationId) AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
          AND (@ActiveProductsOnly = 0 OR p.IsActive = 1)
          AND (@SearchText IS NULL OR p.ProductCode LIKE N'%' + @SearchText + N'%' OR p.ProductName LIKE N'%' + @SearchText + N'%')
    )
    SELECT *, COUNT(1) OVER() TotalCount FROM q ORDER BY CurrentStock, ProductName OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spInventoryStockGetOutOfStockPaged]
    @PageNumber INT = 1, @PageSize INT = 20, @SearchText NVARCHAR(200) = NULL, @LocationId INT = NULL, @CategoryId INT = NULL, @ActiveProductsOnly BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH q AS (
        SELECT s.InventoryStockId, p.ProductId, p.ProductCode, p.ProductName, s.LocationId, l.LocationCode, l.LocationName, s.CurrentStock, p.MinimumStockLevel,
               CAST(CASE WHEN s.CurrentStock <= p.MinimumStockLevel THEN 1 ELSE 0 END AS BIT) IsLowStock, CAST(1 AS BIT) IsOutOfStock,
               ISNULL(s.LastMovementDate, s.UpdatedDate) LastMovementDate, s.UpdatedDate
        FROM dbo.InventoryStock s JOIN dbo.Product p ON p.ProductId = s.ProductId JOIN dbo.InventoryLocation l ON l.LocationId = s.LocationId
        WHERE s.CurrentStock <= 0 AND (@LocationId IS NULL OR s.LocationId = @LocationId) AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
          AND (@ActiveProductsOnly = 0 OR p.IsActive = 1)
          AND (@SearchText IS NULL OR p.ProductCode LIKE N'%' + @SearchText + N'%' OR p.ProductName LIKE N'%' + @SearchText + N'%')
    )
    SELECT *, COUNT(1) OVER() TotalCount FROM q ORDER BY ProductName OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spInventoryStockGetSummary] @LocationId INT = NULL AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1) ProductCount,
           SUM(CASE WHEN s.CurrentStock <= p.MinimumStockLevel THEN 1 ELSE 0 END) LowStockCount,
           SUM(CASE WHEN s.CurrentStock <= 0 THEN 1 ELSE 0 END) OutOfStockCount,
           ISNULL(SUM(s.CurrentStock), 0) TotalStockQty
    FROM dbo.InventoryStock s JOIN dbo.Product p ON p.ProductId = s.ProductId
    WHERE (@LocationId IS NULL OR s.LocationId = @LocationId);
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spInventoryMovementGetPaged]
    @PageNumber INT = 1, @PageSize INT = 20, @SearchText NVARCHAR(200) = NULL, @ProductId INT = NULL, @LocationId INT = NULL, @MovementType NVARCHAR(30) = NULL, @FromDate DATETIME2(0) = NULL, @ToDate DATETIME2(0) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    ;WITH q AS (
        SELECT m.InventoryMovementId, m.ProductId, p.ProductCode, p.ProductName, m.LocationId, l.LocationCode, l.LocationName, m.MovementType, m.Quantity, m.QuantitySigned, m.UnitCost, m.StockBefore, m.StockAfter, m.ReferenceType, m.ReferenceId, m.ReferenceNo, m.Reason, m.Remarks, m.CreatedByUserId, m.CreatedDate
        FROM dbo.InventoryMovement m JOIN dbo.Product p ON p.ProductId = m.ProductId JOIN dbo.InventoryLocation l ON l.LocationId = m.LocationId
        WHERE m.IsActive = 1 AND (@ProductId IS NULL OR m.ProductId = @ProductId) AND (@LocationId IS NULL OR m.LocationId = @LocationId) AND (@MovementType IS NULL OR m.MovementType = @MovementType)
          AND (@FromDate IS NULL OR m.CreatedDate >= @FromDate) AND (@ToDate IS NULL OR m.CreatedDate < DATEADD(DAY, 1, @ToDate))
          AND (@SearchText IS NULL OR p.ProductCode LIKE N'%' + @SearchText + N'%' OR p.ProductName LIKE N'%' + @SearchText + N'%' OR m.ReferenceNo LIKE N'%' + @SearchText + N'%')
    )
    SELECT *, COUNT(1) OVER() TotalCount FROM q ORDER BY CreatedDate DESC, InventoryMovementId DESC OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spInventoryMovementGetByProductId] @ProductId INT AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.spInventoryMovementGetPaged @PageNumber = 1, @PageSize = 500, @ProductId = @ProductId;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spInventoryMovementGetByReferenceNo] @ReferenceNo NVARCHAR(100) AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.InventoryMovementId, m.ProductId, p.ProductCode, p.ProductName, m.LocationId, l.LocationCode, l.LocationName, m.MovementType, m.Quantity, m.QuantitySigned, m.UnitCost, m.StockBefore, m.StockAfter, m.ReferenceType, m.ReferenceId, m.ReferenceNo, m.Reason, m.Remarks, m.CreatedByUserId, m.CreatedDate
    FROM dbo.InventoryMovement m JOIN dbo.Product p ON p.ProductId = m.ProductId JOIN dbo.InventoryLocation l ON l.LocationId = m.LocationId
    WHERE m.ReferenceNo = @ReferenceNo AND m.IsActive = 1
    ORDER BY m.InventoryMovementId;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spInventoryMovementGetSummaryByDateRange] @FromDate DATETIME2(0), @ToDate DATETIME2(0), @LocationId INT = NULL AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(CreatedDate AS DATE) MovementDate, MovementType, SUM(Quantity) TotalQuantity, COUNT(1) MovementCount
    FROM dbo.InventoryMovement
    WHERE IsActive = 1 AND CreatedDate >= @FromDate AND CreatedDate < DATEADD(DAY, 1, @ToDate) AND (@LocationId IS NULL OR LocationId = @LocationId)
    GROUP BY CAST(CreatedDate AS DATE), MovementType
    ORDER BY MovementDate, MovementType;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spStockAdjustmentCreate] @LocationId INT, @Reason NVARCHAR(500), @Remarks NVARCHAR(1000) = N'', @CreatedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    IF LEN(LTRIM(RTRIM(ISNULL(@Reason, N'')))) = 0 THROW 51100, 'Stock adjustment reason is required.', 1;
    DECLARE @No NVARCHAR(50) = CONCAT(N'ADJ-', FORMAT(SYSUTCDATETIME(), 'yyyyMMddHHmmssfff'));
    INSERT dbo.StockAdjustment (AdjustmentNo, LocationId, Reason, Remarks, CreatedByUserId) VALUES (@No, @LocationId, @Reason, ISNULL(@Remarks, N''), NULLIF(@CreatedByUserId, 0));
    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spStockAdjustmentAddItem] @StockAdjustmentId BIGINT, @ProductId INT, @Quantity DECIMAL(18,4), @AdjustmentType NVARCHAR(20), @UnitCost DECIMAL(18,4) = 0, @Reason NVARCHAR(500) AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.StockAdjustment WHERE StockAdjustmentId = @StockAdjustmentId AND Status <> N'Draft') THROW 51101, 'Only draft stock adjustments can be changed.', 1;
    INSERT dbo.StockAdjustmentItem (StockAdjustmentId, ProductId, Quantity, AdjustmentType, UnitCost, Reason) VALUES (@StockAdjustmentId, @ProductId, @Quantity, @AdjustmentType, ISNULL(@UnitCost, 0), @Reason);
    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spStockAdjustmentApprove] @StockAdjustmentId BIGINT, @ApprovedByUserId INT AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @LocationId INT, @AdjustmentNo NVARCHAR(50), @ProductId INT, @Quantity DECIMAL(18,4), @Type NVARCHAR(20), @UnitCost DECIMAL(18,4), @Reason NVARCHAR(500), @MovementType NVARCHAR(30);
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @LocationId = LocationId, @AdjustmentNo = AdjustmentNo FROM dbo.StockAdjustment WITH (UPDLOCK) WHERE StockAdjustmentId = @StockAdjustmentId AND Status = N'Draft';
        IF @LocationId IS NULL THROW 51102, 'Draft stock adjustment was not found.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.StockAdjustmentItem WHERE StockAdjustmentId = @StockAdjustmentId) THROW 51103, 'Stock adjustment requires at least one item.', 1;
        DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT ProductId, Quantity, AdjustmentType, UnitCost, Reason FROM dbo.StockAdjustmentItem WHERE StockAdjustmentId = @StockAdjustmentId;
        OPEN c; FETCH NEXT FROM c INTO @ProductId, @Quantity, @Type, @UnitCost, @Reason;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @MovementType = CASE WHEN @Type = N'Increase' THEN N'AdjustmentIn' ELSE N'AdjustmentOut' END;
            EXEC dbo.spInventoryMovementCreate @ProductId, @LocationId, @MovementType, @Quantity, @UnitCost, N'StockAdjustment', @StockAdjustmentId, @AdjustmentNo, @Reason, N'', 0, 0, @ApprovedByUserId;
            FETCH NEXT FROM c INTO @ProductId, @Quantity, @Type, @UnitCost, @Reason;
        END
        CLOSE c; DEALLOCATE c;
        UPDATE dbo.StockAdjustment SET Status = N'Approved', UpdatedByUserId = NULLIF(@ApprovedByUserId, 0), UpdatedDate = SYSUTCDATETIME() WHERE StockAdjustmentId = @StockAdjustmentId;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF CURSOR_STATUS('local','c') >= 0 CLOSE c;
        IF CURSOR_STATUS('local','c') > -3 DEALLOCATE c;
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spStockAdjustmentReject] @StockAdjustmentId BIGINT, @Reason NVARCHAR(500), @RejectedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.StockAdjustment SET Status = N'Rejected', Remarks = CONCAT(Remarks, CASE WHEN Remarks = N'' THEN N'' ELSE N' | ' END, N'Rejected: ', @Reason), UpdatedByUserId = NULLIF(@RejectedByUserId, 0), UpdatedDate = SYSUTCDATETIME() WHERE StockAdjustmentId = @StockAdjustmentId AND Status = N'Draft';
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spStockAdjustmentCancel] @StockAdjustmentId BIGINT, @CancelledByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.StockAdjustment SET Status = N'Cancelled', UpdatedByUserId = NULLIF(@CancelledByUserId, 0), UpdatedDate = SYSUTCDATETIME() WHERE StockAdjustmentId = @StockAdjustmentId AND Status = N'Draft';
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spStockAdjustmentGetPaged] @PageNumber INT = 1, @PageSize INT = 20, @SearchText NVARCHAR(200) = NULL, @Status NVARCHAR(20) = NULL, @LocationId INT = NULL AS
BEGIN
    SET NOCOUNT ON;
    ;WITH q AS (
        SELECT a.StockAdjustmentId, a.AdjustmentNo, a.LocationId, l.LocationName, a.Status, a.Reason, a.Remarks, a.CreatedByUserId, a.CreatedDate, a.UpdatedByUserId, a.UpdatedDate
        FROM dbo.StockAdjustment a JOIN dbo.InventoryLocation l ON l.LocationId = a.LocationId
        WHERE (@Status IS NULL OR a.Status = @Status) AND (@LocationId IS NULL OR a.LocationId = @LocationId)
          AND (@SearchText IS NULL OR a.AdjustmentNo LIKE N'%' + @SearchText + N'%' OR a.Reason LIKE N'%' + @SearchText + N'%')
    )
    SELECT *, COUNT(1) OVER() TotalCount FROM q ORDER BY CreatedDate DESC OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spStockAdjustmentGetById] @StockAdjustmentId BIGINT AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.StockAdjustmentId, a.AdjustmentNo, a.LocationId, l.LocationName, a.Status, a.Reason, a.Remarks, a.CreatedByUserId, a.CreatedDate, a.UpdatedByUserId, a.UpdatedDate
    FROM dbo.StockAdjustment a JOIN dbo.InventoryLocation l ON l.LocationId = a.LocationId WHERE a.StockAdjustmentId = @StockAdjustmentId;
    SELECT i.StockAdjustmentItemId, i.StockAdjustmentId, i.ProductId, p.ProductCode, p.ProductName, i.Quantity, i.AdjustmentType, i.UnitCost, i.Reason
    FROM dbo.StockAdjustmentItem i JOIN dbo.Product p ON p.ProductId = i.ProductId WHERE i.StockAdjustmentId = @StockAdjustmentId;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spStockCountCreate] @LocationId INT, @Remarks NVARCHAR(1000) = N'', @CreatedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @No NVARCHAR(50) = CONCAT(N'CNT-', FORMAT(SYSUTCDATETIME(), 'yyyyMMddHHmmssfff'));
    INSERT dbo.StockCount (StockCountNo, LocationId, Remarks, CreatedByUserId) VALUES (@No, @LocationId, ISNULL(@Remarks, N''), NULLIF(@CreatedByUserId, 0));
    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spStockCountAddItem] @StockCountId BIGINT, @ProductId INT, @CountedQty DECIMAL(18,4), @Remarks NVARCHAR(1000) = N'' AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @LocationId INT, @SystemQty DECIMAL(18,4);
    SELECT @LocationId = LocationId FROM dbo.StockCount WHERE StockCountId = @StockCountId AND Status = N'Draft';
    IF @LocationId IS NULL THROW 51200, 'Only draft stock counts can be changed.', 1;
    SELECT @SystemQty = ISNULL(CurrentStock, 0) FROM dbo.InventoryStock WHERE ProductId = @ProductId AND LocationId = @LocationId;
    INSERT dbo.StockCountItem (StockCountId, ProductId, SystemQty, CountedQty, Remarks) VALUES (@StockCountId, @ProductId, ISNULL(@SystemQty, 0), @CountedQty, ISNULL(@Remarks, N''));
    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spStockCountUpdateCountedQty] @StockCountItemId BIGINT, @CountedQty DECIMAL(18,4), @Remarks NVARCHAR(1000) = N'', @UpdatedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE i SET CountedQty = @CountedQty, Remarks = ISNULL(@Remarks, N'')
    FROM dbo.StockCountItem i JOIN dbo.StockCount c ON c.StockCountId = i.StockCountId
    WHERE i.StockCountItemId = @StockCountItemId AND c.Status = N'Draft';
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spStockCountApprove] @StockCountId BIGINT, @ApprovedByUserId INT AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @LocationId INT, @No NVARCHAR(50), @ProductId INT, @Variance DECIMAL(18,4), @CorrectionQty DECIMAL(18,4), @IsDecrease BIT;
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @LocationId = LocationId, @No = StockCountNo FROM dbo.StockCount WITH (UPDLOCK) WHERE StockCountId = @StockCountId AND Status = N'Draft';
        IF @LocationId IS NULL THROW 51201, 'Draft stock count was not found.', 1;
        DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT ProductId, VarianceQty FROM dbo.StockCountItem WHERE StockCountId = @StockCountId AND VarianceQty <> 0;
        OPEN c; FETCH NEXT FROM c INTO @ProductId, @Variance;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @CorrectionQty = ABS(@Variance);
            SET @IsDecrease = CASE WHEN @Variance < 0 THEN 1 ELSE 0 END;
            EXEC dbo.spInventoryMovementCreate @ProductId, @LocationId, N'StockCountCorrection', @CorrectionQty, 0, N'StockCount', @StockCountId, @No, N'Stock count variance correction', N'', 0, @IsDecrease, @ApprovedByUserId;
            FETCH NEXT FROM c INTO @ProductId, @Variance;
        END
        CLOSE c; DEALLOCATE c;
        UPDATE dbo.StockCount SET Status = N'Approved', UpdatedByUserId = NULLIF(@ApprovedByUserId, 0), UpdatedDate = SYSUTCDATETIME() WHERE StockCountId = @StockCountId;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF CURSOR_STATUS('local','c') >= 0 CLOSE c;
        IF CURSOR_STATUS('local','c') > -3 DEALLOCATE c;
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spStockCountCancel] @StockCountId BIGINT, @CancelledByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.StockCount SET Status = N'Cancelled', UpdatedByUserId = NULLIF(@CancelledByUserId, 0), UpdatedDate = SYSUTCDATETIME() WHERE StockCountId = @StockCountId AND Status = N'Draft';
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spStockCountGetPaged] @PageNumber INT = 1, @PageSize INT = 20, @SearchText NVARCHAR(200) = NULL, @Status NVARCHAR(20) = NULL, @LocationId INT = NULL AS
BEGIN
    SET NOCOUNT ON;
    ;WITH q AS (
        SELECT c.StockCountId, c.StockCountNo, c.LocationId, l.LocationName, c.Status, c.Remarks, c.CreatedByUserId, c.CreatedDate, c.UpdatedByUserId, c.UpdatedDate
        FROM dbo.StockCount c JOIN dbo.InventoryLocation l ON l.LocationId = c.LocationId
        WHERE (@Status IS NULL OR c.Status = @Status) AND (@LocationId IS NULL OR c.LocationId = @LocationId)
          AND (@SearchText IS NULL OR c.StockCountNo LIKE N'%' + @SearchText + N'%')
    )
    SELECT *, COUNT(1) OVER() TotalCount FROM q ORDER BY CreatedDate DESC OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spStockCountGetById] @StockCountId BIGINT AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.StockCountId, c.StockCountNo, c.LocationId, l.LocationName, c.Status, c.Remarks, c.CreatedByUserId, c.CreatedDate, c.UpdatedByUserId, c.UpdatedDate
    FROM dbo.StockCount c JOIN dbo.InventoryLocation l ON l.LocationId = c.LocationId WHERE c.StockCountId = @StockCountId;
    SELECT i.StockCountItemId, i.StockCountId, i.ProductId, p.ProductCode, p.ProductName, i.SystemQty, i.CountedQty, i.VarianceQty, i.Remarks
    FROM dbo.StockCountItem i JOIN dbo.Product p ON p.ProductId = i.ProductId WHERE i.StockCountId = @StockCountId;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spStockTransferCreate] @SourceLocationId INT, @DestinationLocationId INT, @Remarks NVARCHAR(1000) = N'', @CreatedByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    IF @SourceLocationId = @DestinationLocationId THROW 51300, 'Source and destination locations must be different.', 1;
    DECLARE @No NVARCHAR(50) = CONCAT(N'TRF-', FORMAT(SYSUTCDATETIME(), 'yyyyMMddHHmmssfff'));
    INSERT dbo.StockTransfer (TransferNo, SourceLocationId, DestinationLocationId, Remarks, CreatedByUserId) VALUES (@No, @SourceLocationId, @DestinationLocationId, ISNULL(@Remarks, N''), NULLIF(@CreatedByUserId, 0));
    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spStockTransferAddItem] @StockTransferId BIGINT, @ProductId INT, @Quantity DECIMAL(18,4), @UnitCost DECIMAL(18,4) = 0 AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.StockTransfer WHERE StockTransferId = @StockTransferId AND Status <> N'Draft') THROW 51301, 'Only draft stock transfers can be changed.', 1;
    INSERT dbo.StockTransferItem (StockTransferId, ProductId, Quantity, UnitCost) VALUES (@StockTransferId, @ProductId, @Quantity, ISNULL(@UnitCost, 0));
    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spStockTransferSend] @StockTransferId BIGINT, @SentByUserId INT AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @SourceId INT, @No NVARCHAR(50), @ProductId INT, @Quantity DECIMAL(18,4), @UnitCost DECIMAL(18,4);
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @SourceId = SourceLocationId, @No = TransferNo FROM dbo.StockTransfer WITH (UPDLOCK) WHERE StockTransferId = @StockTransferId AND Status = N'Draft';
        IF @SourceId IS NULL THROW 51302, 'Draft stock transfer was not found.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.StockTransferItem WHERE StockTransferId = @StockTransferId) THROW 51303, 'Stock transfer requires at least one item.', 1;
        DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT ProductId, Quantity, UnitCost FROM dbo.StockTransferItem WHERE StockTransferId = @StockTransferId;
        OPEN c; FETCH NEXT FROM c INTO @ProductId, @Quantity, @UnitCost;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            EXEC dbo.spInventoryMovementCreate @ProductId, @SourceId, N'TransferOut', @Quantity, @UnitCost, N'StockTransfer', @StockTransferId, @No, N'Stock transfer sent', N'', 0, 0, @SentByUserId;
            FETCH NEXT FROM c INTO @ProductId, @Quantity, @UnitCost;
        END
        CLOSE c; DEALLOCATE c;
        UPDATE dbo.StockTransfer SET Status = N'Sent', SentDate = SYSUTCDATETIME(), UpdatedByUserId = NULLIF(@SentByUserId, 0), UpdatedDate = SYSUTCDATETIME() WHERE StockTransferId = @StockTransferId;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF CURSOR_STATUS('local','c') >= 0 CLOSE c;
        IF CURSOR_STATUS('local','c') > -3 DEALLOCATE c;
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spStockTransferReceive] @StockTransferId BIGINT, @ReceivedByUserId INT AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @DestinationId INT, @No NVARCHAR(50), @ProductId INT, @Quantity DECIMAL(18,4), @UnitCost DECIMAL(18,4);
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @DestinationId = DestinationLocationId, @No = TransferNo FROM dbo.StockTransfer WITH (UPDLOCK) WHERE StockTransferId = @StockTransferId AND Status = N'Sent';
        IF @DestinationId IS NULL THROW 51304, 'Sent stock transfer was not found.', 1;
        DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT ProductId, Quantity, UnitCost FROM dbo.StockTransferItem WHERE StockTransferId = @StockTransferId;
        OPEN c; FETCH NEXT FROM c INTO @ProductId, @Quantity, @UnitCost;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            EXEC dbo.spInventoryMovementCreate @ProductId, @DestinationId, N'TransferIn', @Quantity, @UnitCost, N'StockTransfer', @StockTransferId, @No, N'Stock transfer received', N'', 0, 0, @ReceivedByUserId;
            FETCH NEXT FROM c INTO @ProductId, @Quantity, @UnitCost;
        END
        CLOSE c; DEALLOCATE c;
        UPDATE dbo.StockTransfer SET Status = N'Received', ReceivedDate = SYSUTCDATETIME(), UpdatedByUserId = NULLIF(@ReceivedByUserId, 0), UpdatedDate = SYSUTCDATETIME() WHERE StockTransferId = @StockTransferId;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF CURSOR_STATUS('local','c') >= 0 CLOSE c;
        IF CURSOR_STATUS('local','c') > -3 DEALLOCATE c;
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spStockTransferCancel] @StockTransferId BIGINT, @CancelledByUserId INT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.StockTransfer SET Status = N'Cancelled', UpdatedByUserId = NULLIF(@CancelledByUserId, 0), UpdatedDate = SYSUTCDATETIME() WHERE StockTransferId = @StockTransferId AND Status IN (N'Draft', N'Sent');
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spStockTransferGetPaged] @PageNumber INT = 1, @PageSize INT = 20, @SearchText NVARCHAR(200) = NULL, @Status NVARCHAR(20) = NULL, @SourceLocationId INT = NULL, @DestinationLocationId INT = NULL AS
BEGIN
    SET NOCOUNT ON;
    ;WITH q AS (
        SELECT t.StockTransferId, t.TransferNo, t.SourceLocationId, sl.LocationName SourceLocationName, t.DestinationLocationId, dl.LocationName DestinationLocationName, t.Status, t.Remarks, t.SentDate, t.ReceivedDate, t.CreatedByUserId, t.CreatedDate, t.UpdatedByUserId, t.UpdatedDate
        FROM dbo.StockTransfer t JOIN dbo.InventoryLocation sl ON sl.LocationId = t.SourceLocationId JOIN dbo.InventoryLocation dl ON dl.LocationId = t.DestinationLocationId
        WHERE (@Status IS NULL OR t.Status = @Status) AND (@SourceLocationId IS NULL OR t.SourceLocationId = @SourceLocationId) AND (@DestinationLocationId IS NULL OR t.DestinationLocationId = @DestinationLocationId)
          AND (@SearchText IS NULL OR t.TransferNo LIKE N'%' + @SearchText + N'%')
    )
    SELECT *, COUNT(1) OVER() TotalCount FROM q ORDER BY CreatedDate DESC OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[spStockTransferGetById] @StockTransferId BIGINT AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.StockTransferId, t.TransferNo, t.SourceLocationId, sl.LocationName SourceLocationName, t.DestinationLocationId, dl.LocationName DestinationLocationName, t.Status, t.Remarks, t.SentDate, t.ReceivedDate, t.CreatedByUserId, t.CreatedDate, t.UpdatedByUserId, t.UpdatedDate
    FROM dbo.StockTransfer t JOIN dbo.InventoryLocation sl ON sl.LocationId = t.SourceLocationId JOIN dbo.InventoryLocation dl ON dl.LocationId = t.DestinationLocationId WHERE t.StockTransferId = @StockTransferId;
    SELECT i.StockTransferItemId, i.StockTransferId, i.ProductId, p.ProductCode, p.ProductName, i.Quantity, i.UnitCost
    FROM dbo.StockTransferItem i JOIN dbo.Product p ON p.ProductId = i.ProductId WHERE i.StockTransferId = @StockTransferId;
END;
GO
