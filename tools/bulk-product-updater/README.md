# AphiwatPOS Bulk Product Updater

Windows Forms utility for Thai product, image, and stock management.

## Main Screen

Page title: `จัดการสินค้าและสต็อกสินค้า`

The main grid supports:
- Search by product name, product code, barcode, active status, and unsynced image status
- Inline edit for product name, barcode, cost price, selling price, wholesale price, unit, and active status
- Product thumbnail display
- Validation before saving
- Confirmation before barcode or price edits
- Product update audit rows in `dbo.ProductBulkUpdateAudit`
- Barcode scanner workflow from the barcode search field

## Product Code Format

New product codes follow the same format as the web app:

```text
PRO-01
PRO-02
PRO-03
```

## Product Creation

Use `เพิ่มสินค้าใหม่`.

Barcode can be scanned/manually entered or generated with `สร้างบาร์โค้ดอัตโนมัติ`.

Generated barcodes use internal prefix `20` and an EAN-13 check digit.

## Product Images

Use `เพิ่มหรือเปลี่ยนรูปสินค้า`.

The app lets the user select an image from the computer. The image is resized and compressed to JPEG, then saved locally under:

```text
ProductImages\{ProductCode}\
```

Image metadata is stored in `dbo.ProductImageSync`:
- Local image path
- SHA-256 image hash
- Sync status: `Pending`, `Synced`, or `Failed`
- Uploaded date
- Uploaded employee/user id
- Last error message

Use `ซิงค์รูปภาพขึ้นฐานข้อมูล` to sync pending or failed images. Sync reuses `dbo.spProductUpdateImage` and updates `dbo.Product.ProductImageUrl`.

If sync fails, the local file remains and can be retried later. The app checks synced image hashes so the same product image is not uploaded again.

Use the `รูปยังไม่ซิงค์` filter to show products with pending or failed images.

## Barcode Printing

Use `พิมพ์บาร์โค้ด`.

The print dialog supports product name, barcode, selling price, label quantity, printer selection, label size, print preview, and direct print.

## Excel Stock Count

Use `ส่งออกไฟล์ Excel สำหรับนับสต็อก`.

The exported `.xlsx` sheet is named `ตรวจนับสต็อก` and contains:
- `รหัสสินค้า`
- `ชื่อสินค้า`
- `บาร์โค้ด`
- `ราคาทุน`
- `ราคาขายปลีก`
- `ราคาขายส่ง`
- `สต็อกปัจจุบัน`
- `สต็อกใหม่`
- `หมายเหตุ`

Staff should enter the physical count only in `สต็อกใหม่`.

Use `นำเข้า Excel และอัปเดตสต็อก` to validate the completed file. The preview shows ready, skipped, and error rows before `ยืนยันการอัปเดตสต็อก`.

Stock updates reuse `dbo.spInventoryMovementCreate` with `AdjustmentIn` / `AdjustmentOut` and `ReferenceType = StockCountImport`.

Import history is stored in:
- `dbo.ProductStockImportBatch`
- `dbo.ProductStockImportItem`

## Database Setup

The app creates missing helper objects when it opens. For controlled deployment, run:

```powershell
sqlcmd -S APHIWAT -d AphiwatPOSDB -E -i tools\bulk-product-updater\Database-BulkProductUpdater.sql
```

## Build And Publish

```powershell
powershell -ExecutionPolicy Bypass -File tools\bulk-product-updater\Build-BulkProductUpdater.ps1
```

Output:

```text
artifacts\bulk-product-updater\AphiwatPOS.BulkProductUpdater.exe
```

## Manual Test Cases

1. Create a product and confirm the product code uses `PRO-xx`.
2. Search by barcode and press Enter.
3. Edit product name and confirm the Thai success message.
4. Try a negative price and verify Thai validation.
5. Try a duplicate barcode and verify the save is blocked.
6. Select a product image and confirm a local file is created under `ProductImages`.
7. Confirm the grid shows a thumbnail and `Pending` image sync status.
8. Click `ซิงค์รูปภาพขึ้นฐานข้อมูล` and verify status becomes `Synced`.
9. Use `รูปยังไม่ซิงค์` to filter pending/failed image rows.
10. Print preview an existing barcode.
11. Export stock-count Excel and confirm `สต็อกใหม่` is blank.
12. Import a file with valid, blank, unchanged, negative, and unknown product rows.
13. Confirm only valid changed rows update stock.
14. Import the same completed file again and verify duplicate import is blocked.
15. Open import history and verify summary rows.
