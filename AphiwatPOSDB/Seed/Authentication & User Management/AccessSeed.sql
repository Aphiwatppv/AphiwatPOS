IF NOT EXISTS (SELECT 1 FROM [dbo].[AccessRole] WHERE [RoleName] = N'Admin')
    INSERT INTO [dbo].[AccessRole] ([RoleCode], [RoleName], [Description])
    VALUES (N'ADMIN', N'Admin', N'Full system administration');
ELSE
    UPDATE [dbo].[AccessRole]
    SET [RoleCode] = ISNULL(NULLIF([RoleCode], N''), N'ADMIN')
    WHERE [RoleName] = N'Admin';

IF NOT EXISTS (SELECT 1 FROM [dbo].[AccessRole] WHERE [RoleName] = N'Cashier')
    INSERT INTO [dbo].[AccessRole] ([RoleCode], [RoleName], [Description])
    VALUES (N'CASHIER', N'Cashier', N'Point of sale operations');
ELSE
    UPDATE [dbo].[AccessRole]
    SET [RoleCode] = ISNULL(NULLIF([RoleCode], N''), N'CASHIER')
    WHERE [RoleName] = N'Cashier';

IF NOT EXISTS (SELECT 1 FROM [dbo].[AccessRole] WHERE [RoleName] = N'Manager')
    INSERT INTO [dbo].[AccessRole] ([RoleCode], [RoleName], [Description])
    VALUES (N'MANAGER', N'Manager', N'Approvals and store management');
ELSE
    UPDATE [dbo].[AccessRole]
    SET [RoleCode] = ISNULL(NULLIF([RoleCode], N''), N'MANAGER')
    WHERE [RoleName] = N'Manager';

IF NOT EXISTS (SELECT 1 FROM [dbo].[AccessPermission] WHERE [PermissionCode] = N'DISCOUNT_APPROVE')
    INSERT INTO [dbo].[AccessPermission] ([PermissionCode], [PermissionName], [ModuleName], [Description])
    VALUES (N'DISCOUNT_APPROVE', N'Approve discount', N'Sales', N'Allow approval of protected discounts');

IF NOT EXISTS (SELECT 1 FROM [dbo].[AccessPermission] WHERE [PermissionCode] = N'REFUND_PROCESS')
    INSERT INTO [dbo].[AccessPermission] ([PermissionCode], [PermissionName], [ModuleName], [Description])
    VALUES (N'REFUND_PROCESS', N'Process refund', N'Sales', N'Allow refund processing');

IF NOT EXISTS (SELECT 1 FROM [dbo].[AccessPermission] WHERE [PermissionCode] = N'STOCK_ADJUST')
    INSERT INTO [dbo].[AccessPermission] ([PermissionCode], [PermissionName], [ModuleName], [Description])
    VALUES (N'STOCK_ADJUST', N'Adjust stock', N'Inventory', N'Allow inventory stock adjustments');

IF NOT EXISTS (SELECT 1 FROM [dbo].[AccessPermission] WHERE [PermissionCode] = N'REPORT_VIEW')
    INSERT INTO [dbo].[AccessPermission] ([PermissionCode], [PermissionName], [ModuleName], [Description])
    VALUES (N'REPORT_VIEW', N'View reports', N'Reports', N'Allow access to business reports');

IF NOT EXISTS (SELECT 1 FROM [dbo].[AccessPermission] WHERE [PermissionCode] = N'SALES_CHECKOUT')
    INSERT INTO [dbo].[AccessPermission] ([PermissionCode], [PermissionName], [ModuleName], [Description])
    VALUES (N'SALES_CHECKOUT', N'Use POS checkout', N'Sales', N'Allow sales checkout and held sales');

IF NOT EXISTS (SELECT 1 FROM [dbo].[AccessPermission] WHERE [PermissionCode] = N'SALES_DISCOUNT')
    INSERT INTO [dbo].[AccessPermission] ([PermissionCode], [PermissionName], [ModuleName], [Description])
    VALUES (N'SALES_DISCOUNT', N'Apply sales discount', N'Sales', N'Allow controlled item and order discounts');

IF NOT EXISTS (SELECT 1 FROM [dbo].[AccessPermission] WHERE [PermissionCode] = N'SALES_REFUND')
    INSERT INTO [dbo].[AccessPermission] ([PermissionCode], [PermissionName], [ModuleName], [Description])
    VALUES (N'SALES_REFUND', N'Process sales refund', N'Sales', N'Allow sales returns and refunds');

IF NOT EXISTS (SELECT 1 FROM [dbo].[AccessPermission] WHERE [PermissionCode] = N'SALES_VOID')
    INSERT INTO [dbo].[AccessPermission] ([PermissionCode], [PermissionName], [ModuleName], [Description])
    VALUES (N'SALES_VOID', N'Void completed sale', N'Sales', N'Allow voiding completed sales');

IF NOT EXISTS (SELECT 1 FROM [dbo].[AccessPermission] WHERE [PermissionCode] = N'INVENTORY_VIEW')
    INSERT INTO [dbo].[AccessPermission] ([PermissionCode], [PermissionName], [ModuleName], [Description])
    VALUES (N'INVENTORY_VIEW', N'View inventory', N'Inventory', N'Allow inventory dashboards and stock lookup');

IF NOT EXISTS (SELECT 1 FROM [dbo].[AccessPermission] WHERE [PermissionCode] = N'INVENTORY_ADJUST')
    INSERT INTO [dbo].[AccessPermission] ([PermissionCode], [PermissionName], [ModuleName], [Description])
    VALUES (N'INVENTORY_ADJUST', N'Adjust inventory', N'Inventory', N'Allow stock adjustment, count, and transfer workflows');

IF NOT EXISTS (SELECT 1 FROM [dbo].[AccessPermission] WHERE [PermissionCode] = N'PRODUCT_MANAGE')
    INSERT INTO [dbo].[AccessPermission] ([PermissionCode], [PermissionName], [ModuleName], [Description])
    VALUES (N'PRODUCT_MANAGE', N'Manage products', N'Products', N'Allow product, brand, category, and unit maintenance');

IF NOT EXISTS (SELECT 1 FROM [dbo].[AccessPermission] WHERE [PermissionCode] = N'CUSTOMER_MANAGE')
    INSERT INTO [dbo].[AccessPermission] ([PermissionCode], [PermissionName], [ModuleName], [Description])
    VALUES (N'CUSTOMER_MANAGE', N'Manage customers', N'Customers', N'Allow customer, loyalty, and credit workflows');

IF NOT EXISTS (SELECT 1 FROM [dbo].[AccessPermission] WHERE [PermissionCode] = N'CASH_CLOSING')
    INSERT INTO [dbo].[AccessPermission] ([PermissionCode], [PermissionName], [ModuleName], [Description])
    VALUES (N'CASH_CLOSING', N'Close daily sales', N'Sales', N'Allow end-of-day sales closing and counted-cash reconciliation');

IF NOT EXISTS (SELECT 1 FROM [dbo].[AccessPermission] WHERE [PermissionCode] = N'CASH_DRAWER_OPEN')
    INSERT INTO [dbo].[AccessPermission] ([PermissionCode], [PermissionName], [ModuleName], [Description])
    VALUES (N'CASH_DRAWER_OPEN', N'Open cash drawer', N'Sales', N'Allow manual cash drawer opening with reason logging');

IF NOT EXISTS (SELECT 1 FROM [dbo].[AccessPermission] WHERE [PermissionCode] = N'CASH_DRAWER_MANAGE')
    INSERT INTO [dbo].[AccessPermission] ([PermissionCode], [PermissionName], [ModuleName], [Description])
    VALUES (N'CASH_DRAWER_MANAGE', N'Manage cash drawer', N'Sales', N'Allow drawer tests, cash out approval, and shift review');

INSERT INTO [dbo].[AccessRolePermission] ([RoleId], [PermissionId])
SELECT r.[RoleId], p.[PermissionId]
FROM [dbo].[AccessRole] r
CROSS JOIN [dbo].[AccessPermission] p
WHERE r.[RoleName] = N'Admin'
  AND NOT EXISTS
  (
      SELECT 1
      FROM [dbo].[AccessRolePermission] rp
      WHERE rp.[RoleId] = r.[RoleId]
        AND rp.[PermissionId] = p.[PermissionId]
  );

INSERT INTO [dbo].[AccessRolePermission] ([RoleId], [PermissionId])
SELECT r.[RoleId], p.[PermissionId]
FROM [dbo].[AccessRole] r
CROSS JOIN [dbo].[AccessPermission] p
WHERE r.[RoleName] = N'Manager'
  AND p.[PermissionCode] IN (N'DISCOUNT_APPROVE', N'REFUND_PROCESS', N'STOCK_ADJUST', N'REPORT_VIEW', N'SALES_CHECKOUT', N'SALES_DISCOUNT', N'SALES_REFUND', N'SALES_VOID', N'INVENTORY_VIEW', N'INVENTORY_ADJUST', N'PRODUCT_MANAGE', N'CUSTOMER_MANAGE', N'CASH_CLOSING', N'CASH_DRAWER_OPEN', N'CASH_DRAWER_MANAGE')
  AND NOT EXISTS
  (
      SELECT 1
      FROM [dbo].[AccessRolePermission] rp
      WHERE rp.[RoleId] = r.[RoleId]
        AND rp.[PermissionId] = p.[PermissionId]
  );

INSERT INTO [dbo].[AccessRolePermission] ([RoleId], [PermissionId])
SELECT r.[RoleId], p.[PermissionId]
FROM [dbo].[AccessRole] r
CROSS JOIN [dbo].[AccessPermission] p
WHERE r.[RoleName] = N'Cashier'
  AND p.[PermissionCode] IN (N'SALES_CHECKOUT', N'CUSTOMER_MANAGE', N'CASH_DRAWER_OPEN')
  AND NOT EXISTS
  (
      SELECT 1
      FROM [dbo].[AccessRolePermission] rp
      WHERE rp.[RoleId] = r.[RoleId]
        AND rp.[PermissionId] = p.[PermissionId]
  );

IF NOT EXISTS (SELECT 1 FROM [dbo].[AccessUser] WHERE [Username] = N'admin')
BEGIN
    INSERT INTO [dbo].[AccessUser] ([Username], [DisplayName], [Email], [PasswordHash], [IsActive])
    VALUES
    (
        N'admin',
        N'System Administrator',
        N'',
        N'AQAAAAIAAYagAAAAELN1IZA33yPhrxmd3a5xuvmQIre0hbyWi6UcYVlQ/e2YYK65NNFfwMFod8u+XX7gYA==',
        1
    );

    INSERT INTO [dbo].[AccessUserRole] ([UserId], [RoleId])
    SELECT u.[UserId], r.[RoleId]
    FROM [dbo].[AccessUser] u
    CROSS JOIN [dbo].[AccessRole] r
    WHERE u.[Username] = N'admin'
      AND r.[RoleName] = N'Admin';
END;

IF NOT EXISTS (SELECT 1 FROM [dbo].[AccessUser] WHERE [Username] = N'Aphiwat')
BEGIN
    INSERT INTO [dbo].[AccessUser] ([Username], [DisplayName], [Email], [PasswordHash], [IsActive])
    VALUES
    (
        N'Aphiwat',
        N'Aphiwat Administrator',
        N'',
        N'AQAAAAIAAYagAAAAEFJEwzdAhREjtfmYH8yHPY8ppIvTOEyZFP62T2SkW57w8TlaPnfjOxOtyawXgXkZqA==',
        1
    );

    INSERT INTO [dbo].[AccessUserRole] ([UserId], [RoleId])
    SELECT u.[UserId], r.[RoleId]
    FROM [dbo].[AccessUser] u
    CROSS JOIN [dbo].[AccessRole] r
    WHERE u.[Username] = N'Aphiwat'
      AND r.[RoleName] = N'Admin'
      AND NOT EXISTS
      (
          SELECT 1
          FROM [dbo].[AccessUserRole] ur
          WHERE ur.[UserId] = u.[UserId]
            AND ur.[RoleId] = r.[RoleId]
      );
END;
