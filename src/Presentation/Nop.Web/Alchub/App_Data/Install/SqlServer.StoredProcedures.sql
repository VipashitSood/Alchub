-- =============================================
-- Description:	<Split function>
-- =============================================
CREATE OR ALTER FUNCTION [dbo].[Split](@String varchar(MAX), @Delimiter char(1))       
returns @temptable TABLE (items varchar(MAX))       
as       
begin      
    declare @idx int       
    declare @slice varchar(8000)       

    select @idx = 1       
        if len(@String)<1 or @String is null  return       

    while @idx!= 0       
    begin       
        set @idx = charindex(@Delimiter,@String)       
        if @idx!=0       
            set @slice = left(@String,@idx - 1)       
        else       
            set @slice = @String       

        if(len(@slice)>0)  
            insert into @temptable(Items) values(@slice)       

        set @String = right(@String,len(@String) - @idx)       
        if len(@String) = 0 break       
    end   
return 
end
GO


-- =============================================
-- Author:		<Author,,Bhautik>
-- Create date: <Create Date,, 18-07-23>
-- Description:	<Description,, Get api filter base data & sub categories data>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ApiFilterLoadBaseAndSubCategories]
(
	@Categoryid INT,	--a root categoryid
	@SubCategoryids Varchar(125), -- sub category ids
	@VendorIds VARCHAR(500) -- vendorids
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SELECT id,[name],parentgroupedproductId,IsMaster,VisibleIndividually INTO #tmpProductdata
	FROM Product WITH(NOLOCK)
	WHERE Product.IsMaster = 1 AND Published=1 AND Deleted=0 AND ProductTypeId = 5 AND VisibleIndividually=1
	AND UPCCode IN (SELECT DISTINCT UPCCode FROM Product WITH(NOLOCK) WHERE Product.IsMaster <> 1 AND Published=1 AND Deleted=0 AND StockQuantity>0 AND Vendorid in (select Items from [dbo].[Split](CASE WHEN @VendorIds='' THEN CAST(Vendorid AS VARCHAR) ELSE @VendorIds END,',')))
	
	--Add Group Product ALSO
	SET IDENTITY_INSERT #tmpProductdata ON;
	INSERT INTO #tmpProductdata(id,name,parentgroupedproductId,IsMaster,VisibleIndividually)
	SELECT id,name,parentgroupedproductId,IsMaster,VisibleIndividually FROM PRODUCT
	WHERE Product.IsMaster = 1 AND Published=1 AND Deleted=0 AND ProductTypeId = 10
	AND id IN (SELECT ParentGroupedProductId FROM #tmpProductdata WHERE ParentGroupedProductId<>0)
	SET IDENTITY_INSERT #tmpProductdata OFF;
	
	SELECT DISTINCT id,Name AS MainFilter,EntityName,DisplayOrder AS MainFilterDisplayOrder 
	INTO #tmpFilter FROM(
	SELECT -4 AS Id,'Category' AS Name,'Category' AS EntityName,-4 AS DisplayOrder
	UNION ALL
	SELECT -3 AS Id,'Filter By Price' AS Name,'Price' AS EntityName,-3 AS DisplayOrder
	UNION ALL
	SELECT -2 AS Id,'Brands' AS Name,'Manufacturer' AS EntityName,-2 AS DisplayOrder
	UNION ALL
	SELECT -1 AS Id,'Vendor' AS Name,'Vendor' AS EntityName,-1 AS DisplayOrder
	UNION ALL
	SELECT SA.id,SA.Name,'SpecificationAttribute' AS EntityName,SA.DisplayOrder AS DisplayOrder
	FROM Product_SpecificationAttribute_Mapping PSAM WITH(NOLOCK)
	INNER JOIN #tmpProductdata P ON PSAM.ProductId=P.id
	INNER JOIN SpecificationAttributeOption SAP WITH(NOLOCK) ON SAP.Id=PSAM.SpecificationAttributeOptionId
	INNER JOIN SpecificationAttribute SA WITH(NOLOCK) ON SA.Id=SAP.SpecificationAttributeId
	WHERE PSAM.AllowFiltering=1
	)TF;
	--select * from Category where Deleted=0 AND Published=1
	SELECT distinct PCM.CategoryId AS CategoryId
	,PC.[name] AS CategoryName
	,COUNT(1) As CategoryItemCount
	,PC.DisplayOrder AS DisplayOrder
	INTO #tmpMasterData
	FROM #tmpProductdata P
	INNER JOIN Product_Category_Mapping PCM WITH(NOLOCK) ON P.Id = PCM.ProductId 
	INNER JOIN Category PC WITH(NOLOCK) ON PC.id=PCM.CategoryId AND PC.Deleted=0 AND PC.Published=1
	WHERE PC.ParentCategoryId=@Categoryid AND CategoryId in (select Items from [dbo].[Split](CASE WHEN @SubCategoryids='' THEN CAST(CategoryId AS VARCHAR) ELSE @SubCategoryids END,','))
	GROUP BY CategoryId,PC.[name],PC.DisplayOrder
	
	--Get Final Data
	SELECT F.*,C.* FROM #tmpFilter F
	LEFT JOIN #tmpMasterData C ON F.id = -4--Category
	ORDER BY F.MainFilterDisplayOrder,C.DisplayOrder,C.CategoryName
	
	DROP TABLE #tmpFilter
	DROP TABLE #tmpProductdata
	DROP TABLE #tmpMasterData
END
GO


-- =============================================
-- Manufacturer SP
-- Author:		<Author,,Bhautik>
-- Create date: <Create Date, 19-07-23>
-- Description:	<Description, Get api filter manufacturer data>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ApiFilterLoadManufacturers]
(
	@Categoryid INT,	--a root categoryid
	@SubCategoryids Varchar(125), -- sub category ids
	@VendorIds VARCHAR(500), -- vendorids
	@ManufactureIds VARCHAR(125)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
		
	--GEt Product List
	SELECT id,parentgroupedproductId,IsMaster,VisibleIndividually INTO #tmpProductListdata
	FROM Product WITH(NOLOCK)
	WHERE Product.IsMaster = 1 AND Published=1 AND Deleted=0 AND ProductTypeId = 5 AND VisibleIndividually=1
	AND UPCCode IN (SELECT DISTINCT UPCCode FROM Product WITH(NOLOCK) WHERE Product.IsMaster <> 1 AND Published=1 AND Deleted=0 AND StockQuantity>0 AND Vendorid in (select Items from [dbo].[Split](CASE WHEN @VendorIds='' THEN CAST(Vendorid AS VARCHAR) ELSE @VendorIds END,',')))
	
	--Add Group Product ALSO
	SET IDENTITY_INSERT #tmpProductListdata ON;
	INSERT INTO #tmpProductListdata(id,parentgroupedproductId,IsMaster,VisibleIndividually)
	SELECT id,parentgroupedproductId,IsMaster,VisibleIndividually FROM PRODUCT
	WHERE Product.IsMaster = 1 AND Published=1 AND Deleted=0 AND ProductTypeId = 10
	AND id IN (SELECT ParentGroupedProductId FROM #tmpProductListdata WHERE ParentGroupedProductId<>0)
	SET IDENTITY_INSERT #tmpProductListdata OFF;
	
	SELECT P.id,PMM.ManufacturerId INTO #tmpProductdata
	FROM #tmpProductListdata P WITH(NOLOCK) 
	INNER JOIN Product_Category_Mapping PCM WITH(NOLOCK) ON P.Id = PCM.ProductId 
	INNER JOIN Category PC WITH(NOLOCK) ON PC.id=PCM.CategoryId AND PC.Deleted=0 AND PC.Published=1
	INNER JOIN Product_Manufacturer_Mapping PMM WITH(NOLOCK) ON P.Id = PMM.ProductId AND ManufacturerId in (select Items from [dbo].[Split](CASE WHEN @ManufactureIds='' THEN CAST(ManufacturerId AS VARCHAR) ELSE @ManufactureIds END,','))
	WHERE PC.ParentCategoryId=@Categoryid AND CategoryId in (select Items from [dbo].[Split](CASE WHEN @SubCategoryids='' THEN CAST(CategoryId AS VARCHAR) ELSE @SubCategoryids END,','))
	
	SELECT DISTINCT id,Name AS MainFilter,EntityName,DisplayOrder AS MainFilterDisplayOrder 
	INTO #tmpFilter FROM(
	SELECT -4 AS Id,'Category' AS Name,'Category' AS EntityName,-4 AS DisplayOrder
	UNION ALL
	SELECT -3 AS Id,'Filter By Price' AS Name,'Price' AS EntityName,-3 AS DisplayOrder
	UNION ALL
	SELECT -2 AS Id,'Brands' AS Name,'Manufacturer' AS EntityName,-2 AS DisplayOrder
	UNION ALL
	SELECT -1 AS Id,'Vendor' AS Name,'Vendor' AS EntityName,-1 AS DisplayOrder
	UNION ALL
	SELECT SA.id,SA.Name,'SpecificationAttribute' AS EntityName,SA.DisplayOrder AS DisplayOrder
	FROM Product_SpecificationAttribute_Mapping PSAM WITH(NOLOCK)
	INNER JOIN #tmpProductdata P ON PSAM.ProductId=P.id
	INNER JOIN SpecificationAttributeOption SAP WITH(NOLOCK) ON SAP.Id=PSAM.SpecificationAttributeOptionId
	INNER JOIN SpecificationAttribute SA WITH(NOLOCK) ON SA.Id=SAP.SpecificationAttributeId
	WHERE PSAM.AllowFiltering=1
	)TF;
	
	SELECT distinct P.ManufacturerId AS ManufactureId,MAN.[name] AS Manufacturer
	,COUNT(1) As ManufacturerCount,MAN.DisplayOrder AS DisplayOrder
	INTO #tmpMasterData
	FROM #tmpProductdata P
	INNER JOIN Manufacturer MAN WITH(NOLOCK) ON MAN.Id = P.ManufacturerId AND Published=1 AND Deleted=0
	GROUP BY P.ManufacturerId,MAN.[name],MAN.DisplayOrder
	
	--Get Final Data
	SELECT F.*,TMD.* FROM #tmpFilter F
	LEFT JOIN #tmpMasterData TMD ON F.id= -2 --Brands
	ORDER BY F.MainFilterDisplayOrder,TMD.DisplayOrder,TMD.Manufacturer
	
	DROP TABLE #tmpFilter
	DROP TABLE #tmpProductdata
	DROP TABLE #tmpMasterData
	DROP TABLE #tmpProductListdata
END
GO


-- =============================================
-- Vendor SP
-- Author:		<Author,,Bhautik>
-- Create date: <Create Date, 19-07-23>
-- Description:	<Description, Get api filter vendor data>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ApiFilterLoadVendors]
(
	@Categoryid INT,	--a root categoryid
	@SubCategoryids Varchar(125), -- sub category ids
	@VendorIds VARCHAR(500), -- vendorids
	@ManufactureIds VARCHAR(125) -- manufacturer ids
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
		
	--GEt Product List
	SELECT id,parentgroupedproductId,IsMaster,VisibleIndividually,VendorId INTO #tmpProductListdata
	FROM Product WITH(NOLOCK)
	WHERE Product.IsMaster = 1 AND Published=1 AND Deleted=0 AND ProductTypeId = 5 AND VisibleIndividually=1
	AND UPCCode IN (SELECT DISTINCT UPCCode FROM Product WITH(NOLOCK) WHERE Product.IsMaster <> 1 AND Published=1 AND Deleted=0 AND StockQuantity>0 AND Vendorid in (select Items from [dbo].[Split](CASE WHEN @VendorIds='' THEN CAST(Vendorid AS VARCHAR) ELSE @VendorIds END,',')))
	
	--Add Group Product ALSO
	SET IDENTITY_INSERT #tmpProductListdata ON;
	INSERT INTO #tmpProductListdata(id,parentgroupedproductId,IsMaster,VisibleIndividually,VendorId)
	SELECT id,parentgroupedproductId,IsMaster,VisibleIndividually,VendorId FROM PRODUCT
	WHERE Product.IsMaster = 1 AND Published=1 AND Deleted=0 AND ProductTypeId = 10
	AND id IN (SELECT ParentGroupedProductId FROM #tmpProductListdata WHERE ParentGroupedProductId<>0)
	SET IDENTITY_INSERT #tmpProductListdata OFF;
	
	SELECT DISTINCT P.id,P.VendorId INTO #tmpProductdata
	FROM Product P WITH(NOLOCK) 
	INNER JOIN Product_Category_Mapping PCM ON P.Id = PCM.ProductId 
	INNER JOIN Category PC WITH(NOLOCK) ON PC.id=PCM.CategoryId AND PC.Deleted=0 AND PC.Published=1
	INNER JOIN Product_Manufacturer_Mapping PMM WITH(NOLOCK) ON P.Id = PMM.ProductId AND ManufacturerId in (select Items from [dbo].[Split](CASE WHEN @ManufactureIds='' THEN CAST(ManufacturerId AS VARCHAR) ELSE @ManufactureIds END,','))
	WHERE PC.ParentCategoryId=@Categoryid AND CategoryId in (select Items from [dbo].[Split](CASE WHEN @SubCategoryids='' THEN CAST(CategoryId AS VARCHAR) ELSE @SubCategoryids END,','))
	AND P.IsMaster <> 1 AND P.Published=1 AND P.Deleted=0
	AND UPCCode IN (SELECT DISTINCT UPCCode FROM #tmpProductListdata)
	 
	
	SELECT DISTINCT id,Name AS MainFilter,EntityName,DisplayOrder AS MainFilterDisplayOrder 
	INTO #tmpFilter FROM(
	SELECT -4 AS Id,'Category' AS Name,'Category' AS EntityName,-4 AS DisplayOrder
	UNION ALL
	SELECT -3 AS Id,'Filter By Price' AS Name,'Price' AS EntityName,-3 AS DisplayOrder
	UNION ALL
	SELECT -2 AS Id,'Brands' AS Name,'Manufacturer' AS EntityName,-2 AS DisplayOrder
	UNION ALL
	SELECT -1 AS Id,'Vendor' AS Name,'Vendor' AS EntityName,-1 AS DisplayOrder
	UNION ALL
	SELECT SA.id,SA.Name,'SpecificationAttribute' AS EntityName,SA.DisplayOrder AS DisplayOrder
	FROM Product_SpecificationAttribute_Mapping PSAM WITH(NOLOCK)
	INNER JOIN #tmpProductdata P ON PSAM.ProductId=P.id
	INNER JOIN SpecificationAttributeOption SAP WITH(NOLOCK) ON SAP.Id=PSAM.SpecificationAttributeOptionId
	INNER JOIN SpecificationAttribute SA WITH(NOLOCK) ON SA.Id=SAP.SpecificationAttributeId
	WHERE PSAM.AllowFiltering=1
	)TF;
	
	SELECT P.VendorId AS VendorId,V.Name AS Vendor,COUNT(1) As VendorCount,V.DisplayOrder AS DisplayOrder
	INTO #tmpMasterData
	FROM #tmpProductdata P
	INNER JOIN vendor V WITH(NOLOCK) ON P.VendorId = V.id AND V.Active=1 AND V.Deleted=0
	GROUP BY P.VendorId,V.Name,V.DisplayOrder
	
	--Get Final Data
	SELECT F.*,TMD.* FROM #tmpFilter F
	LEFT JOIN #tmpMasterData TMD ON F.id = -1--Vendor
	ORDER BY F.MainFilterDisplayOrder,TMD.DisplayOrder,TMD.Vendor
	
	DROP TABLE #tmpFilter
	DROP TABLE #tmpProductdata
	DROP TABLE #tmpMasterData
	DROP TABLE #tmpProductListdata
END
GO


-- =============================================
-- Specification options SP
-- Author:		<Author,,Bhautik>
-- Create date: <Create Date, 19-07-23>
-- Description:	<Description, Get api filter specifiaction options data>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[ApiFilterLoadSpecificationOptions]
(
	@Categoryid INT,	--a root categoryid
	@SubCategoryids Varchar(125), -- sub category ids
	@VendorIds VARCHAR(500), -- vendorids
	@ManufactureIds VARCHAR(125), -- manufacturer ids
	@CallFor_SpecificationAttrId INT, -- specification Attribute Id
	@SpecificationOptionIds VARCHAR(500) -- specification option ids
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
		
	--GEt Product List
	SELECT id,parentgroupedproductId,IsMaster,VisibleIndividually,VendorId INTO #tmpProductListdata
	FROM Product WITH(NOLOCK)
	WHERE Product.IsMaster = 1 AND Published=1 AND Deleted=0 AND ProductTypeId = 5 AND VisibleIndividually=1
	AND UPCCode IN (SELECT DISTINCT UPCCode FROM Product WITH(NOLOCK) WHERE Product.IsMaster <> 1 AND Published=1 AND Deleted=0 AND StockQuantity>0 AND Vendorid in (select Items from [dbo].[Split](CASE WHEN @VendorIds='' THEN CAST(Vendorid AS VARCHAR) ELSE @VendorIds END,',')))
	
	--Add Group Product ALSO
	SET IDENTITY_INSERT #tmpProductListdata ON;
	INSERT INTO #tmpProductListdata(id,parentgroupedproductId,IsMaster,VisibleIndividually,VendorId)
	SELECT id,parentgroupedproductId,IsMaster,VisibleIndividually,VendorId FROM PRODUCT
	WHERE Product.IsMaster = 1 AND Published=1 AND Deleted=0 AND ProductTypeId = 10
	AND id IN (SELECT ParentGroupedProductId FROM #tmpProductListdata WHERE ParentGroupedProductId<>0)
	SET IDENTITY_INSERT #tmpProductListdata OFF;
	
	SELECT DISTINCT P.id INTO #tmpProductdata
	FROM Product P WITH(NOLOCK) 
	INNER JOIN Product_Category_Mapping PCM ON P.Id = PCM.ProductId 
	INNER JOIN Category PC WITH(NOLOCK) ON PC.id=PCM.CategoryId AND PC.Deleted=0 AND PC.Published=1
	INNER JOIN Product_Manufacturer_Mapping PMM WITH(NOLOCK) ON P.Id = PMM.ProductId AND ManufacturerId in (select Items from [dbo].[Split](CASE WHEN @ManufactureIds='' THEN CAST(ManufacturerId AS VARCHAR) ELSE @ManufactureIds END,','))
	WHERE PC.ParentCategoryId=@Categoryid AND CategoryId in (select Items from [dbo].[Split](CASE WHEN @SubCategoryids='' THEN CAST(CategoryId AS VARCHAR) ELSE @SubCategoryids END,','))
	AND P.IsMaster = 1 AND P.Published=1 AND P.Deleted=0
	AND UPCCode IN (SELECT DISTINCT UPCCode FROM #tmpProductListdata)
	
	SELECT DISTINCT Id,Name AS MainFilter,MainFilterDisplayOrder
	INTO #tmpFilter FROM(
	SELECT -4 AS Id,'Category' AS Name,-4 AS MainFilterDisplayOrder
	UNION ALL
	SELECT -3 AS Id,'Filter By Price' AS Name,-3 AS MainFilterDisplayOrder
	UNION ALL
	SELECT -2 AS Id,'Brands' AS Name,-2 AS MainFilterDisplayOrder
	UNION ALL
	SELECT -1 AS Id,'Vendor' AS Name,-1 AS MainFilterDisplayOrder
	UNION ALL
	SELECT SA.id,SA.Name,SA.DisplayOrder AS MainFilterDisplayOrder
	FROM Product_SpecificationAttribute_Mapping PSAM WITH(NOLOCK)
	INNER JOIN #tmpProductdata P ON PSAM.ProductId=P.id
	INNER JOIN SpecificationAttributeOption SAP WITH(NOLOCK) ON SAP.Id=PSAM.SpecificationAttributeOptionId
	INNER JOIN SpecificationAttribute SA WITH(NOLOCK) ON SA.Id=SAP.SpecificationAttributeId
	WHERE PSAM.AllowFiltering=1
	)TF;
	
	SELECT SA.id,SAP.Id AS SpecificationOptionId,SAP.Name AS SpecificationOptionName,SAP.DisplayOrder,COUNT(1) AS ItemCount 
	INTO #tmpMasterdata
	FROM #tmpProductdata P
	INNER JOIN Product_SpecificationAttribute_Mapping PSAM WITH(NOLOCK) ON PSAM.ProductId=P.id
	INNER JOIN SpecificationAttributeOption SAP WITH(NOLOCK) ON SAP.Id=PSAM.SpecificationAttributeOptionId
	INNER JOIN SpecificationAttribute SA WITH(NOLOCK) ON SA.Id=SAP.SpecificationAttributeId 
	WHERE PSAM.AllowFiltering=1 AND SA.id=@CallFor_SpecificationAttrId AND SAP.Name<>''
	AND SA.id in (select Items from [dbo].[Split](CASE WHEN @SpecificationOptionIds='' THEN CAST(SA.id AS VARCHAR) ELSE @SpecificationOptionIds END,','))
	GROUP BY SA.id,SAP.Id,SAP.Name,SAP.DisplayOrder
	
	--Get Final Data
	SELECT DISTINCT F.Id,F.MainFilter,F.MainFilterDisplayOrder,FMD.SpecificationOptionId,FMD.SpecificationOptionName
	,FMD.ItemCount,FMD.DisplayOrder
	FROM #tmpFilter F
	LEFT OUTER JOIN #tmpMasterdata FMD ON F.id=FMD.id AND F.id=@CallFor_SpecificationAttrId
	ORDER BY F.MainFilterDisplayOrder,FMD.SpecificationOptionName,FMD.DisplayOrder
	
	DROP TABLE #tmpFilter
	DROP TABLE #tmpProductdata
	DROP TABLE #tmpProductListdata
	DROP TABLE #tmpMasterdata
END
GO