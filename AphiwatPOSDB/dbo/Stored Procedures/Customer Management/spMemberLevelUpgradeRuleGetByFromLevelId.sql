CREATE PROCEDURE [dbo].[spMemberLevelUpgradeRuleGetByFromLevelId] @FromMemberLevelId INT AS
BEGIN SELECT r.*,f.LevelName FromMemberLevelName,t.LevelName ToMemberLevelName FROM dbo.MemberLevelUpgradeRule r JOIN dbo.MemberLevel f ON f.MemberLevelId=r.FromMemberLevelId JOIN dbo.MemberLevel t ON t.MemberLevelId=r.ToMemberLevelId WHERE r.FromMemberLevelId=@FromMemberLevelId AND r.IsActive=1; END
