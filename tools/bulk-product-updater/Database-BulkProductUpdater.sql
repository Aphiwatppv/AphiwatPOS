SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.ProductBulkUpdateAudit', N'U') IS NULL
CREATE TABLE dbo.ProductBulkUpdateAudit
(
    ProductAuditId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductBulkUpdateAudit PRIMARY KEY,
    ProductId INT NOT NULL,
    FieldName NVARCHAR(100) NOT NULL,
    OldValue NVARCHAR(MAX) NOT NULL,
    NewValue NVARCHAR(MAX) NOT NULL,
    UpdatedByEmployeeId INT NULL,
    UpdatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_ProductBulkUpdateAudit_UpdatedDate DEFAULT SYSUTCDATETIME()
);
GO

IF OBJECT_ID(N'dbo.ProductStockImportBatch', N'U') IS NULL
CREATE TABLE dbo.ProductStockImportBatch
(
    ImportBatchId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductStockImportBatch PRIMARY KEY,
    Filename NVARCHAR(260) NOT NULL,
    FileHash NVARCHAR(128) NOT NULL,
    ImportedByEmployeeId INT NOT NULL,
    ImportedDate DATETIME2(0) NOT NULL CONSTRAINT DF_ProductStockImportBatch_ImportedDate DEFAULT SYSUTCDATETIME(),
    TotalRows INT NOT NULL,
    UpdatedRows INT NOT NULL,
    SkippedRows INT NOT NULL,
    ErrorRows INT NOT NULL,
    Status NVARCHAR(30) NOT NULL,
    Remark NVARCHAR(500) NOT NULL CONSTRAINT DF_ProductStockImportBatch_Remark DEFAULT N''
);
GO

IF OBJECT_ID(N'dbo.ProductStockImportItem', N'U') IS NULL
CREATE TABLE dbo.ProductStockImportItem
(
    ImportItemId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductStockImportItem PRIMARY KEY,
    ImportBatchId BIGINT NOT NULL,
    ProductId INT NOT NULL,
    ProductCode NVARCHAR(50) NOT NULL,
    Barcode NVARCHAR(100) NOT NULL,
    OldStock DECIMAL(18,4) NOT NULL,
    NewStock DECIMAL(18,4) NOT NULL,
    Variance DECIMAL(18,4) NOT NULL,
    InventoryMovementId BIGINT NULL,
    Status NVARCHAR(30) NOT NULL,
    Message NVARCHAR(500) NOT NULL CONSTRAINT DF_ProductStockImportItem_Message DEFAULT N'',
    CONSTRAINT FK_ProductStockImportItem_Batch FOREIGN KEY (ImportBatchId) REFERENCES dbo.ProductStockImportBatch(ImportBatchId)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_Product_Barcode_NotNull' AND object_id=OBJECT_ID(N'dbo.Product'))
CREATE UNIQUE INDEX IX_Product_Barcode_NotNull ON dbo.Product(Barcode) WHERE Barcode IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_ProductStockImportBatch_FileHash' AND object_id=OBJECT_ID(N'dbo.ProductStockImportBatch'))
CREATE UNIQUE INDEX IX_ProductStockImportBatch_FileHash ON dbo.ProductStockImportBatch(FileHash);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_ProductStockImportItem_Batch' AND object_id=OBJECT_ID(N'dbo.ProductStockImportItem'))
CREATE INDEX IX_ProductStockImportItem_Batch ON dbo.ProductStockImportItem(ImportBatchId);
GO

IF OBJECT_ID(N'dbo.ProductImageSync', N'U') IS NULL
CREATE TABLE dbo.ProductImageSync
(
    ProductImageSyncId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductImageSync PRIMARY KEY,
    ProductId INT NOT NULL,
    LocalImagePath NVARCHAR(500) NOT NULL,
    ImageHash NVARCHAR(128) NOT NULL,
    SyncStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_ProductImageSync_SyncStatus DEFAULT N'Pending',
    UploadedPath NVARCHAR(500) NOT NULL CONSTRAINT DF_ProductImageSync_UploadedPath DEFAULT N'',
    UploadedDate DATETIME2(0) NULL,
    UploadedByEmployeeId INT NULL,
    CreatedByEmployeeId INT NULL,
    CreatedDate DATETIME2(0) NOT NULL CONSTRAINT DF_ProductImageSync_CreatedDate DEFAULT SYSUTCDATETIME(),
    LastError NVARCHAR(500) NOT NULL CONSTRAINT DF_ProductImageSync_LastError DEFAULT N''
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_ProductImageSync_Pending' AND object_id=OBJECT_ID(N'dbo.ProductImageSync'))
CREATE INDEX IX_ProductImageSync_Pending ON dbo.ProductImageSync(SyncStatus, ProductId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_ProductImageSync_ProductHashSynced' AND object_id=OBJECT_ID(N'dbo.ProductImageSync'))
CREATE INDEX IX_ProductImageSync_ProductHashSynced ON dbo.ProductImageSync(ProductId, ImageHash, SyncStatus);
GO

DECLARE @Permissions TABLE(PermissionCode NVARCHAR(100), PermissionName NVARCHAR(100), ModuleName NVARCHAR(100), Description NVARCHAR(250));
INSERT @Permissions(PermissionCode, PermissionName, ModuleName, Description)
VALUES
(N'Product.View', N'ดูสินค้า', N'Product', N'View products in desktop product and stock management'),
(N'Product.Create', N'เพิ่มสินค้า', N'Product', N'Create products in desktop product and stock management'),
(N'Product.Edit', N'แก้ไขสินค้า', N'Product', N'Edit products in desktop product and stock management'),
(N'Product.GenerateBarcode', N'สร้างบาร์โค้ด', N'Product', N'Generate internal product barcodes'),
(N'Product.PrintBarcode', N'พิมพ์บาร์โค้ด', N'Product', N'Print product barcode labels'),
(N'Inventory.ExportStockCount', N'ส่งออกไฟล์ตรวจนับสต็อก', N'Inventory', N'Export stock-count Excel templates'),
(N'Inventory.ImportStockCount', N'นำเข้าไฟล์ตรวจนับสต็อก', N'Inventory', N'Import stock-count Excel files and update stock'),
(N'Inventory.ViewImportHistory', N'ดูประวัตินำเข้าไฟล์สต็อก', N'Inventory', N'View stock-count import history');

INSERT dbo.AccessPermission(PermissionCode, PermissionName, ModuleName, Description, CreatedByUserId)
SELECT p.PermissionCode, p.PermissionName, p.ModuleName, p.Description, NULL
FROM @Permissions p
WHERE OBJECT_ID(N'dbo.AccessPermission', N'U') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.AccessPermission ap WHERE ap.PermissionCode = p.PermissionCode);
GO
