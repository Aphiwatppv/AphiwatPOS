CREATE PROCEDURE [dbo].[spMemberLevelGetAll] AS BEGIN SELECT * FROM dbo.MemberLevel ORDER BY DisplayOrder,LevelName; END
