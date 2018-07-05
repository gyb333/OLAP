namespace Wilmar.Interface.Sync.OLAP.Criterias
{
    using Wilmar.SSAS.MDXBuilder.Criteria;

    /// <summary>
    /// 销售排名报表条件
    /// </summary>
    public class SalesRankCriteria : AbstractCriteria
    {
        /// <summary>
        /// 单据日期
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_ICBillDates].[YQMD].[Month]", Opration = CriteriaOprations.Range, Type = CriteriaBuildType.SubCube, Order = 1)]
        public int?[] BillDates { get; set; }

        /// <summary>
        /// 大区
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_Orgs].[ParentOrg]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2)]
        public int[] RegionIDs { get; set; }

        



        /// <summary>
        /// 签约经销商ID
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_CompBranches].[DistributorContract]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2)]
        public int[] DistributorContractIDs { get; set; }


        
        /// <summary>
        /// 产线
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_Products].[ProdLine]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2)]
        public int[] ProdLineIDs { get; set; }



        /// <summary>
        /// 品牌
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_Products].[ProdBrand]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2)]
        public int[] ProdBrandIDs { get; set; }


        /// <summary>
        /// 品类
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_Products].[ProdCategory]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2)]
        public int[] ProdCategoryIDs { get; set; }

        /// <summary>
        /// 客户类型
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_Customers].[CustChannelType]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2)]
        public int[] CustChannelTypeIDs { get; set; }



    }
}
