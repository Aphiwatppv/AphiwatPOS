SET NOCOUNT ON;

DECLARE @ThaiCategories TABLE
(
    CategoryCode NVARCHAR(50) NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    DisplayOrder INT NOT NULL
);

INSERT INTO @ThaiCategories (CategoryCode, CategoryName, Description, DisplayOrder)
VALUES
    (N'CATEGORY-00', N'เครื่องดื่ม', N'น้ำดื่ม น้ำอัดลม ชา กาแฟ และเครื่องดื่มพร้อมดื่ม', 5),
    (N'CATEGORY-01', N'ขนมและของทานเล่น', N'ขนมขบเคี้ยว ลูกอม ช็อกโกแลต และของทานเล่นทั่วไป', 10),
    (N'CATEGORY-02', N'อาหารสำเร็จรูป', N'บะหมี่กึ่งสำเร็จรูป อาหารกระป๋อง และอาหารพร้อมรับประทาน', 15),
    (N'CATEGORY-03', N'นมและผลิตภัณฑ์นม', N'นมสด นมกล่อง โยเกิร์ต และผลิตภัณฑ์จากนม', 20),
    (N'CATEGORY-04', N'กาแฟ ชา และผงชงดื่ม', N'กาแฟสำเร็จรูป ชา โกโก้ และเครื่องดื่มชนิดผง', 25),
    (N'CATEGORY-05', N'เครื่องปรุงและวัตถุดิบ', N'น้ำปลา ซอส น้ำตาล แป้ง และวัตถุดิบสำหรับทำอาหาร', 30),
    (N'CATEGORY-06', N'ของใช้ส่วนตัว', N'สบู่ แชมพู ยาสีฟัน กระดาษทิชชู่ และของใช้ส่วนบุคคล', 35),
    (N'CATEGORY-07', N'ของใช้ในบ้าน', N'น้ำยาล้างจาน ผงซักฟอก น้ำยาทำความสะอาด และของใช้ในครัวเรือน', 40),
    (N'CATEGORY-08', N'สุขภาพและยา', N'ยาเบื้องต้น เวชภัณฑ์ หน้ากากอนามัย และสินค้าดูแลสุขภาพ', 45),
    (N'CATEGORY-09', N'อาหารแช่เย็นและแช่แข็ง', N'อาหารแช่เย็น อาหารแช่แข็ง ไอศกรีม และสินค้าที่ต้องควบคุมอุณหภูมิ', 50),
    (N'CATEGORY-10', N'เบเกอรี่', N'ขนมปัง เค้ก แซนด์วิช และเบเกอรี่พร้อมขาย', 55),
    (N'CATEGORY-11', N'สินค้าอื่นๆ', N'สินค้าทั่วไปที่ไม่อยู่ในหมวดหลัก', 99);

UPDATE target
SET
    target.CategoryName = source.CategoryName,
    target.Description = source.Description,
    target.DisplayOrder = source.DisplayOrder,
    target.IsActive = 1,
    target.UpdatedDate = SYSUTCDATETIME()
FROM dbo.ProductCategory AS target
INNER JOIN @ThaiCategories AS source ON source.CategoryCode = target.CategoryCode;

INSERT INTO dbo.ProductCategory (CategoryCode, CategoryName, Description, DisplayOrder)
SELECT source.CategoryCode, source.CategoryName, source.Description, source.DisplayOrder
FROM @ThaiCategories AS source
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.ProductCategory AS target
    WHERE target.CategoryCode = source.CategoryCode
);
