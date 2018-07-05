namespace Wilmar.Interface.Sync.OLAP.Criterias
{
    using Wilmar.SSAS.MDXBuilder.Criteria;

    /// <summary>
    /// 经销商进销存报表条件
    /// </summary>
    public class DistributorInventoryCriteria : AbstractCriteria
    {
        /// <summary>
        /// 单据日期
        /// </summary>
        [CriteriaProperty(Dimission = "[Dim_ICBillDates].[YMD].[Day]", Opration = CriteriaOprations.Range, Type = CriteriaBuildType.SubCube, Order = 1)]
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


        
    }
}
