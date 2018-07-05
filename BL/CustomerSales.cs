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
    /// 终端销量
    /// </summary>
    public static class CustomerSales
    {

        /// <summary>
        /// DTO于多维模型属性映射关系
        /// </summary>
        private static Dictionary<string, string> propertyMap = new Dictionary<string, string>()
        {
            { "[Dim_Orgs].[ParentOrg].[ParentOrg].[MEMBER_CAPTION]", "SalesRegionName" },
            { "[Dim_Orgs].[Org].[Org].[MEMBER_CAPTION]", "SalesOrgName" },
            { "[Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].[MEMBER_CAPTION]", "DistributorCode" },
            { "[Dim_CompBranches].[DistributorContract].[DistributorContract].[MEMBER_CAPTION]", "DistributorName" },
            { "[Dim_Customers].[CustChannelType].[CustChannelType].[MEMBER_CAPTION]", "CustChannelTypeName" },
            { "[Dim_Customers].[CustChannel].[CustChannel].[MEMBER_CAPTION]", "CustChannelName" },
            { "[Dim_Customers].[Code].[Code].[MEMBER_CAPTION]", "CustomerCode" },
            { "[Dim_Customers].[Id].[Id].[MEMBER_CAPTION]", "CustomerName" },
            { "[Measures].[QueryTimeSalesQty]","QueryTimeSalesQty" },
            { "[Measures].[SamePeriodPrevYearSalesQty]","SamePeriodPrevYearSalesQty" },
            { "[Measures].[SalesQtyYOY]","SalesQtyYOY" },
            { "[Measures].[QueryTimeSalesWeight]","QueryTimeSalesWeight" },
            { "[Measures].[SamePeriodPrevYearSalesWeight]","SamePeriodPrevYearSalesWeight" },
            { "[Measures].[SalesWeightYOY]","SalesWeightYOY" }
        };


        private static string buildMdx(string rowAxis)
        {
            var mdx = @"WITH MEMBER [Measures].[QueryTimeSalesQty] AS  [Measures].[BaseUnitQty]

                                            MEMBER [Measures].[SamePeriodPrevYearSalesQty] AS
                                                    SUM(
                                                     {
                                                                ParallelPeriod([Dim_ICBillDates].[YMD].[Year],
		                                                 1,
		                                                 [Dim_ICBillDates].[YMD].[Month].&[{0}]
	                                                   )
	                                                  },

                                                      [Measures].[BaseUnitQty]
	                                                )

                                             MEMBER [Measures].[SalesQtyYOY] AS
                                                Iif(
                                                   IsEmpty([Measures].[QueryTimeSalesQty])
                                                   OR IsEmpty([Measures].[SamePeriodPrevYearSalesQty])
                                                   OR[Measures].[SamePeriodPrevYearSalesQty] = 0
                                                   , NULL
                                                   , ([Measures].[QueryTimeSalesQty]/[Measures].[SamePeriodPrevYearSalesQty]-1)*100
                                                 )

                                            MEMBER [Measures].[QueryTimeSalesWeight] AS [Measures].[Weight]

                                            MEMBER [Measures].[SamePeriodPrevYearSalesWeight] AS
                                                SUM(
                                                {
                                                    ParallelPeriod([Dim_ICBillDates].[YMD].[Year],
		                                            1,
		                                           [Dim_ICBillDates].[YMD].[Month].&[{0}]
      	                                            )
	                                            },
	                                            [Measures].[Weight]
	                                        )

                                            MEMBER [Measures].[SalesWeightYOY]  AS
                                                Iif(
                                                IsEmpty([Measures].[QueryTimeSalesWeight])
                                                OR IsEmpty([Measures].[SamePeriodPrevYearSalesWeight])
                                                OR [Measures].[SamePeriodPrevYearSalesWeight] = 0
                                                , NULL
                                                , ([Measures].[QueryTimeSalesWeight]/[Measures].[SamePeriodPrevYearSalesWeight]-1)*100
                                            )
                                             SELECT  { 
		                                        [Measures].[QueryTimeSalesQty],
		                                        [Measures].[SamePeriodPrevYearSalesQty],
		                                        [Measures].[SalesQtyYOY], 
		                                        [Measures].[QueryTimeSalesWeight],
		                                        [Measures].[SamePeriodPrevYearSalesWeight],
		                                        [Measures].[SalesWeightYOY]
                                                } ON COLUMNS,
                                              NONEMPTY(
		                                            ({ROW_AXIS}), [Measures].[BaseUnitQty]
                                              ) ON ROWS ";

            return mdx.Replace("{ROW_AXIS}", rowAxis);
        }




        /// <summary>
        /// 获取终端销量报表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetSalesForCustomerType<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = buildMdx(@"[Dim_Orgs].[ParentOrg].[ParentOrg].ALLMEMBERS 
                * [Dim_Orgs].[Org].[Org].ALLMEMBERS 
                * [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
                * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
                * [Dim_Customers].[CustChannel].[CustChannel].ALLMEMBERS 
                * [Dim_Customers].[CustChannelType].[CustChannelType].ALLMEMBERS ");

            return Utility.Query<T, CustomerSalesCriteria>(
                con,
                request,
                user,
                mdx,
            propertyMap);
        }


        /// <summary>
        /// 获取单个终端销量报表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetSalesForCustomer<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {
            //System.Diagnostics.Debugger.Break();

            var mdx = buildMdx(@"[Dim_Orgs].[ParentOrg].[ParentOrg].ALLMEMBERS
                *[Dim_Orgs].[Org].[Org].ALLMEMBERS
                * [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
                * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
                * [Dim_Customers].[CustChannel].[CustChannel].ALLMEMBERS 
                * [Dim_Customers].[CustChannelType].[CustChannelType].ALLMEMBERS
                * [Dim_Customers].[Id].[Id].ALLMEMBERS
                * [Dim_Products].[ProdLine].[ProdLine].ALLMEMBERS 
                * [Dim_Products].[ProdCategory].[ProdCategory].ALLMEMBERS
                * [Dim_Products].[ProdBrand].[ProdBrand].ALLMEMBERS
                * [Dim_Products].[ProdCode].[ProdCode].ALLMEMBERS
                * [Dim_Products].[Id].[Id].ALLMEMBERS");

            return Utility.Query<T, CustomerSalesCriteria>(
                con,
                request,
                user,
                mdx,
            propertyMap);
        }


        /// <summary>
        /// 获取终端数量报表报表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetCustomerCount<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {
            //System.Diagnostics.Debugger.Break();
            var mdx = buildMdx(@"[Dim_Orgs].[ParentOrg].[ParentOrg].ALLMEMBERS
                *[Dim_Orgs].[Org].[Org].ALLMEMBERS
                * [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
                * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
                * [Dim_Customers].[CustChannel].[CustChannel].ALLMEMBERS 
                * [Dim_Customers].[CustChannelType].[CustChannelType].ALLMEMBERS");

            return Utility.Query<T, CustomerSalesCriteria>(
                con,
                request,
                user,
                mdx,
            propertyMap);
        }

    }
}
