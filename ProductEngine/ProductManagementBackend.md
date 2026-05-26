# Product Management Backend

## 1. Database Tables

The full SQL script is in `AphiwatPOSDB/Script/ProductManagement.sql`.

Tables included:

- `dbo.ProductCategory`
- `dbo.ProductBrand`
- `dbo.ProductUnit`
- `dbo.ProductUnitConversion`
- `dbo.Product`
- `dbo.ProductPriceHistory`

## 2. Constraints and Indexes

The script includes primary keys, foreign keys, unique business-code constraints, filtered unique barcode index, product filter/search indexes, unit conversion duplicate protection, and price history lookup indexes.

## 3. Stored Procedures

The script includes all requested CRUD, lookup, duplicate-check, paged, low-stock, barcode, unit conversion, and price history procedures. `dbo.spProductUpdatePrice` updates `dbo.Product` and inserts `dbo.ProductPriceHistory` in a single transaction.

## 4. C# Models

Models are defined in `ProductEngine/Models/ProductModels.cs`.

## 5. Service Interfaces

Interfaces are defined in `ProductEngine/Services/ProductServiceInterfaces.cs`.

## 6. Service Implementations

Implementations are defined in `ProductEngine/Services/ProductServices.cs`. They inject `IAccessService`, call stored procedures, use `async`/`await`, pass `CancellationToken`, and use parameter objects.

## 7. Dependency Injection

`AphiwatPOS/Program.cs` registers:

- `IProductCategoryService`
- `IProductBrandService`
- `IProductUnitService`
- `IProductUnitConversionService`
- `IProductService`
- `IProductPriceHistoryService`

## 8. Validation Rules

- Codes: required, trim, uppercase in UI/service caller if desired, max 50, unique per entity.
- Names: required, trim, max 100 for category, brand, unit; max 200 for product.
- Barcode: optional, trim, max 100, unique when present.
- Prices: `CostPrice >= 0`, `SellingPrice >= 0`, `SellingPrice >= CostPrice` if the business disallows negative margin.
- TaxRate: `0 <= TaxRate <= 100`.
- Stock: `MinimumStockLevel >= 0`, `CurrentStock >= 0`.
- Unit conversion: `FromUnitId != ToUnitId`, `ConversionRate > 0`, active pair must be unique.
- Product status: restrict to known values such as `Active`, `Inactive`, `Discontinued`, `Draft`.
- Foreign keys: selected category and unit must be active; brand may be null but should be active when supplied.
- Price updates: require a change reason for manual admin changes; always call `spProductUpdatePrice` instead of direct product price edits when auditing is required.

## 9. Future Backend Expansion

Recommended future tables:

- `InventoryLocation`: branches, warehouses, shelves, or stores.
- `InventoryStockBalance`: product, unit, location, quantity on hand, reserved quantity.
- `InventoryStockMovement`: product, unit, location, movement type, quantity, reference module, reference id, movement date, created by.
- `InventoryStockAdjustment`: adjustment header/detail tables for manual stock corrections.
- `PurchaseOrder` and `PurchaseOrderItem`: supplier purchasing and receiving integration.
- `GoodsReceipt` and `GoodsReceiptItem`: receiving stock into inventory.
- `SalesOrder` or `SalesTransaction` and detail lines: POS checkout integration.
- `ProductSupplier`: supplier-specific SKU, cost, lead time, preferred supplier flag.
- `ProductBarcode`: multiple barcodes per product/unit/package.
- `ProductPriceList` and `ProductPriceListItem`: branch, customer group, promotion, or date-range pricing.
- `ProductCostHistory`: separate inventory costing history if moving-average or FIFO costing is added.
