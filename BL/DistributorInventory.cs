namespace Wilmar.KDSA.Service.Controllers
{
    using Dapper;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using System.Net.Http;
    using Wilmar.Interface.Sync.OLAP;
    using Wilmar.Interface.Sync.OLAP.Criterias;
    using Wilmar.SSAS.MDXBuilder;

    /// <summary>
    /// 经销商经销存
    /// </summary>
    public static class DistributorInventory
    {

        /// <summary>
        /// DTO于多维模型属性映射关系
        /// </summary>
        private static Dictionary<string, string> propertyMap = new Dictionary<string, string>()
        {
            { "[Dim_Orgs].[ParentOrg].[ParentOrg].[MEMBER_CAPTION]", "SalesRegionName" },
            { "[Dim_Orgs].[Org].[Org].[MEMBER_CAPTION]", "SalesOrgName" },
            { "[Dim_Distrbutors].[DistriContractCodeSAP].[DistriContractCodeSAP].[MEMBER_CAPTION]", "DistributorCode" },
            { "[Dim_Distrbutors].[DistributorContract].[DistributorContract].[MEMBER_CAPTION]", "DistributorName" },
            { "[Measures].[POStockInDetls_BaseUnitQty]","InQty" },
            { "[Measures].[BaseUnitQty]","OutQty" },
            { "[Measures].[EndICBaseUnitQty]","EndQty" },
            { "[Measures].[FactPOStockInDetls_Weight]","InWeight" },
            { "[Measures].[Weight]","OutWeight" },
            { "[Measures].[EndICWeight]","EndWeight" }
        };

        private static string buildMdx()
        {
            var mdx = @"with MEMBER [Measures].[EndICBaseUnitQty] AS
                      ([Dim_BillDates].[YMD].[Day].&[{1}].Parent.Lag(2),[Measures].[EndBaseUnitQty]) 
                        + 
                      Aggregate({[Dim_BillDates].[YMD].[Day].&[{1}].Parent.Lag(1).FirstChild
                     :[Dim_BillDates].[YMD].[Day].&[{1}].Lag(1)}, [Measures].[ICVoucherDetls_BaseUnitQty]) 

                      MEMBER [Measures].[EndICWeight] AS
                      ([Dim_BillDates].[YMD].[Day].&[{1}].Parent.Lag(2),[Measures].[EndWeight]) 
                      + Aggregate({[Dim_BillDates].[YMD].[Day].&[{1}].Parent.Lag(1).FirstChild
                     :[Dim_BillDates].[YMD].[Day].&[{1}].Lag(1)}, [Measures].[ICVoucherDetls_Weight])  

                       SELECT NON EMPTY { 
                       [Measures].[POStockInDetls_BaseUnitQty],
                       [Measures].[BaseUnitQty],
                       [Measures].[EndICBaseUnitQty],
                       [Measures].[FactPOStockInDetls_Weight],
                       [Measures].[Weight],
                       [Measures].[EndICWeight]
                    } ON COLUMNS,
                    NONEMPTY(
                        ([Dim_Orgs].[ParentOrg].[ParentOrg].ALLMEMBERS 
                        * [Dim_Orgs].[Org].[Org].ALLMEMBERS 	                
                        *[Dim_Distrbutors].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS
                        * [Dim_Distrbutors].[DistributorContract].[DistributorContract].ALLMEMBERS
                    ),[Measures].[BaseUnitQty]) ON ROWS  ";

            return mdx;
        }


        /// <summary>
        /// 获取经销商进销存生意报表报表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDistributorInventoryData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {
            //System.Diagnostics.Debugger.Break();

            var mdx = buildMdx();
            return Utility.Query<T, DistributorInventoryCriteria>(
                con,
                request,
                user,
                mdx,
            propertyMap);
        }
        

    }
}
