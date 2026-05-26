CREATE TABLE [dbo].[AccessUserRole]
(
    [UserId] INT NOT NULL,
    [RoleId] INT NOT NULL,
    CONSTRAINT [PK_AccessUserRole] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_AccessUserRole_AccessUser] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AccessUser] ([UserId]),
    CONSTRAINT [FK_AccessUserRole_AccessRole] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AccessRole] ([RoleId])
);
