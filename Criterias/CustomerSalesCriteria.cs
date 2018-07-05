namespace Wilmar.Interface.Sync.OLAP.Criterias
{
    using Wilmar.SSAS.MDXBuilder.Criteria;

    /// <summary>
    /// 终端销量报表条件
    /// </summary>
    public class CustomerSalesCriteria : AbstractCriteria
    {
        [CriteriaProperty(Dimission = "[Dim_ICBillDates].[YMD].[Month]", Opration = CriteriaOprations.Range, Type = CriteriaBuildType.SubCube, Order = 1)]
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
        /// 客户类型
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_Customers].[CustChannelType]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2)]
        public int[] CustChannelTypeIDs { get; set; }


        /// <summary>
        /// 客户渠道
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_Customers].[CustChannel]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2)]
        public int[] CustChannelIDs { get; set; }


        /// <summary>
        /// 终端
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_Customers].[Id]", Opration = CriteriaOprations.In, Type = CriteriaBuildType.SubCube, Order = 2)]
        public int[] CustomerIDs { get; set; }


        


    }
}
