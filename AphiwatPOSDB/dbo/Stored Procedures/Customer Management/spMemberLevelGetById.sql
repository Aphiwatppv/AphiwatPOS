CREATE PROCEDURE [dbo].[spMemberLevelGetById] @MemberLevelId INT AS BEGIN SELECT * FROM dbo.MemberLevel WHERE MemberLevelId=@MemberLevelId; END
