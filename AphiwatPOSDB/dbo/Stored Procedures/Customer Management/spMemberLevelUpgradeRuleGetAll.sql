CREATE PROCEDURE [dbo].[spMemberLevelUpgradeRuleGetAll] AS
BEGIN SELECT r.*,f.LevelName FromMemberLevelName,t.LevelName ToMemberLevelName FROM dbo.MemberLevelUpgradeRule r JOIN dbo.MemberLevel f ON f.MemberLevelId=r.FromMemberLevelId JOIN dbo.MemberLevel t ON t.MemberLevelId=r.ToMemberLevelId ORDER BY r.IsActive DESC,r.MemberLevelUpgradeRuleId; END
