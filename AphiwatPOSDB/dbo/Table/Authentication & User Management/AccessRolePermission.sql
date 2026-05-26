CREATE TABLE [dbo].[AccessRolePermission]
(
    [RoleId] INT NOT NULL,
    [PermissionId] INT NOT NULL,
    CONSTRAINT [PK_AccessRolePermission] PRIMARY KEY CLUSTERED ([RoleId] ASC, [PermissionId] ASC),
    CONSTRAINT [FK_AccessRolePermission_AccessRole] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AccessRole] ([RoleId]),
    CONSTRAINT [FK_AccessRolePermission_AccessPermission] FOREIGN KEY ([PermissionId]) REFERENCES [dbo].[AccessPermission] ([PermissionId])
);
