 

namespace Wilmar.Interface.Sync.OLAP.Criterias
{
    using Wilmar.SSAS.MDXBuilder.Criteria;
    /// <summary>
    /// 经销商库存报表条件
    /// </summary>
    public class DistributorStockCriteria : AbstractCriteria
    {
        /// <summary>
        /// 单据日期
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_ICBillDates].[YMD].[Day]", Opration = CriteriaOprations.Range, Type = CriteriaBuildType.SubCube, Order = 1
            ,Conditions = CriteriaConditions.QueryCondition)]
        public int?[] BillDates { get; set; }

        /// <summary>
        /// 大区
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_Orgs].[ParentOrg]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2)]
        public int[] RegionIDs { get; set; }


        /// <summary>
        /// 签约经销商ID
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_Distrbutors].[DistributorContract]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2)]
        public int[] DistributorContractIDs { get; set; }






        /// <summary>
        /// 产品线ID
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_Products].[ProdLine]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2)]
        public int[] ProdLineIDs { get; set; }


        /// <summary>
        /// 品类ID
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_Products].[ProdCategory]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2)]
        public int[] ProdCategoryIDs { get; set; }

        /// <summary>
        /// 产品品牌ID
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_Products].[ProdBrand]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2)]
        public int[] ProdBrandIDs { get; set; }


        /// <summary>
        /// 客户类型
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_Customers].[CustChannelType]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2)]
        public int[] CustChannelTypeIDs { get; set; }





        /// <summary>
        /// 账号ID
        /// </summary>
        /// 或者[Dim_Distrbutors].[Company]
        [CriteriaProperty(Dimission = "[Dim_Distrbutors].[Company]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2
            , Conditions = CriteriaConditions.QueryKDSPermissionsCondition, Role = UserRole.Common, UserFlag =  UserData.CompanyID)]
        public new int[] CompanyIDs { get; set; }

        /// <summary>
        /// 经销商ID
        /// </summary>
        /// 或者 [Dim_Distrbutors].[DistributorID]
        [CriteriaProperty(Dimission = "[Dim_Distrbutors].[Distributor]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2
           , Conditions = CriteriaConditions.QueryKDSPermissionsCondition, Role = UserRole.User, EntityID = 606)]
        public new int[] DistributorIDs { get; set; }


        



    }
}
