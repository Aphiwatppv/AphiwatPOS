SET NOCOUNT ON;

DECLARE @ThaiUnits TABLE
(
    UnitCode NVARCHAR(50) NOT NULL,
    UnitName NVARCHAR(100) NOT NULL,
    UnitSymbol NVARCHAR(30) NOT NULL,
    AllowDecimal BIT NOT NULL,
    IsBaseUnit BIT NOT NULL,
    Description NVARCHAR(500) NOT NULL
);

INSERT INTO @ThaiUnits (UnitCode, UnitName, UnitSymbol, AllowDecimal, IsBaseUnit, Description)
VALUES
    (N'UNIT-0', N'ขวด', N'ขวด', 0, 1, N'หน่วยขายแบบขวด'),
    (N'UNIT-1', N'ลัง', N'ลัง', 0, 0, N'หน่วยขายแบบลัง'),
    (N'UNIT-2', N'ถุง', N'ถุง', 0, 0, N'หน่วยขายแบบถุง'),
    (N'PCS', N'ชิ้น', N'ชิ้น', 0, 1, N'หน่วยขายทั่วไปเป็นชิ้น'),
    (N'BOX', N'กล่อง', N'กล่อง', 0, 0, N'หน่วยขายแบบกล่อง'),
    (N'PACK', N'แพ็ค', N'แพ็ค', 0, 0, N'หน่วยขายแบบแพ็ค'),
    (N'CAN', N'กระป๋อง', N'กระป๋อง', 0, 0, N'หน่วยขายแบบกระป๋อง'),
    (N'CUP', N'แก้ว', N'แก้ว', 0, 0, N'หน่วยขายแบบแก้ว'),
    (N'BAG', N'ห่อ', N'ห่อ', 0, 0, N'หน่วยขายแบบห่อ'),
    (N'DOZEN', N'โหล', N'โหล', 0, 0, N'หน่วยขายแบบโหล'),
    (N'KG', N'กิโลกรัม', N'กก.', 1, 0, N'หน่วยน้ำหนักกิโลกรัม รองรับทศนิยม'),
    (N'G', N'กรัม', N'กรัม', 1, 0, N'หน่วยน้ำหนักกรัม รองรับทศนิยม'),
    (N'L', N'ลิตร', N'ลิตร', 1, 0, N'หน่วยปริมาตรลิตร รองรับทศนิยม'),
    (N'ML', N'มิลลิลิตร', N'มล.', 1, 0, N'หน่วยปริมาตรมิลลิลิตร รองรับทศนิยม'),
    (N'M', N'เมตร', N'เมตร', 1, 0, N'หน่วยความยาวเมตร รองรับทศนิยม');

UPDATE target
SET
    target.UnitName = source.UnitName,
    target.UnitSymbol = source.UnitSymbol,
    target.AllowDecimal = source.AllowDecimal,
    target.IsBaseUnit = source.IsBaseUnit,
    target.Description = source.Description,
    target.IsActive = 1,
    target.UpdatedDate = SYSUTCDATETIME()
FROM dbo.ProductUnit AS target
INNER JOIN @ThaiUnits AS source ON source.UnitCode = target.UnitCode;

INSERT INTO dbo.ProductUnit (UnitCode, UnitName, UnitSymbol, AllowDecimal, IsBaseUnit, Description)
SELECT source.UnitCode, source.UnitName, source.UnitSymbol, source.AllowDecimal, source.IsBaseUnit, source.Description
FROM @ThaiUnits AS source
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.ProductUnit AS target
    WHERE target.UnitCode = source.UnitCode
);
