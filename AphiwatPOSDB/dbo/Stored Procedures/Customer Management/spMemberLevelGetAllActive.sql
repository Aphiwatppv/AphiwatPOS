CREATE PROCEDURE [dbo].[spMemberLevelGetAllActive] AS BEGIN SELECT * FROM dbo.MemberLevel WHERE IsActive=1 ORDER BY DisplayOrder,LevelName; END
