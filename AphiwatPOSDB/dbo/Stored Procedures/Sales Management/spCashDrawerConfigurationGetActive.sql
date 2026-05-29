CREATE PROCEDURE [dbo].[spCashDrawerConfigurationGetActive]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1
        [ConfigurationId],
        [PrinterName],
        [CashDrawerEnabled],
        [DrawerKickCommand],
        [DrawerPin],
        [OpenDrawerAfterReceiptPrint],
        [AllowManualOpenDrawer],
        [IsActive],
        [CreatedByUserId],
        [UpdatedByUserId],
        [CreatedDate],
        [UpdatedDate]
    FROM [dbo].[CashDrawerConfiguration]
    WHERE [IsActive] = 1
    ORDER BY [ConfigurationId] DESC;
END;
