CREATE PROCEDURE [dbo].[spProductCategoryUpdate] @CategoryId INT, @CategoryCode NVARCHAR(50), @CategoryName NVARCHAR(100), @Description NVARCHAR(500), @DisplayOrder INT, @IsActive BIT, @UpdatedByUserId INT AS
BEGIN SET NOCOUNT ON;
    UPDATE [dbo].[ProductCategory] SET CategoryCode=@CategoryCode, CategoryName=@CategoryName, Description=ISNULL(@Description,N''), DisplayOrder=@DisplayOrder, IsActive=@IsActive, UpdatedByUserId=NULLIF(@UpdatedByUserId,0), UpdatedDate=SYSUTCDATETIME() WHERE CategoryId=@CategoryId;
END;
