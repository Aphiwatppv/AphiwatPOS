CREATE TABLE [dbo].[AccessUserPermission]
(
    [UserId] INT NOT NULL,
    [PermissionId] INT NOT NULL,
    CONSTRAINT [PK_AccessUserPermission] PRIMARY KEY CLUSTERED ([UserId] ASC, [PermissionId] ASC),
    CONSTRAINT [FK_AccessUserPermission_AccessUser] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [FK_AccessUserPermission_AccessPermission] FOREIGN KEY ([PermissionId]) REFERENCES [dbo].[AccessPermission] ([PermissionId])
);
