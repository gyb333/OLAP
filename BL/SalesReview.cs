namespace Wilmar.KDSA.Service.Controllers
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Dapper;
    using Wilmar.Interface.Sync.OLAP.Criterias;
    using Wilmar.SSAS.MDXBuilder;
    using Wilmar.SSAS.MDXBuilder.Axis;
    using Wilmar.Interface.Sync.OLAP;

    /// <summary>
    /// 销售回顾
    /// </summary>
    public static class SalesReview
    {

        /// <summary>
        /// DTO于多维模型属性映射关系
        /// </summary>
        private static Dictionary<string, string> reviewMaps = new Dictionary<string, string>()
        {
            { "[Dim_Orgs].[ParentOrg].[ParentOrg].[MEMBER_CAPTION]", "SalesRegionName" },
            { "[Dim_Orgs].[Org].[Org].[MEMBER_CAPTION]", "SalesOrgName" },
            { "[Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].[MEMBER_CAPTION]", "DistributorCode" },
            { "[Dim_CompBranches].[DistributorContract].[DistributorContract].[MEMBER_CAPTION]", "DistributorName" },
            { "[Dim_Products].[ProdLine].[ProdLine].[MEMBER_CAPTION]", "ProductLineName" },
            { "[Dim_Products].[ProdBrand].[ProdBrand].[MEMBER_CAPTION]", "ProductBrandName" },
             { "[Dim_Products].[ProdCategory].[ProdCategory].[MEMBER_CAPTION]", "ProductCategoryName" },
            { "[Dim_Products].[ProdCode].[ProdCode].[MEMBER_CAPTION]", "ProductCode" },
                { "[Dim_Products].[Id].[Id].[MEMBER_CAPTION]", "ProductName" },
            { "[Measures].[QueryTimeSalesQty]","QueryTimeSalesQty" },
            { "[Measures].[SamePeriodPrevYearSalesQty]","SamePeriodPrevYearSalesQty" },
            { "[Measures].[SalesQtyYOY]","SalesQtyYOY" },
            { "[Measures].[SamePeriodPrevMonthSalesQty]","SamePeriodPrevMonthSalesQty" },
            { "[Measures].[SalesQtyMOM]","SalesQtyMOM" },
            { "[Measures].[QueryTimeSalesWeight]","QueryTimeSalesWeight" },
            { "[Measures].[SamePeriodPrevYearSalesWeight]","SamePeriodPrevYearSalesWeight" },
            { "[Measures].[SalesWeightYOY]","SalesWeightYOY" },
            { "[Measures].[SamePeriodPrevMonthSalesWeight]","SamePeriodPrevMonthSalesWeight" },
            { "[Measures].[SalesWeightMOM]","SalesWeightMOM" }
        };


        private static Dictionary<string, string> chartMaps = new Dictionary<string, string>()
        {
            { "[Dim_ICBillDates].[YMD].[Month].[MEMBER_CAPTION]", "Dimission" },
            { "[Dim_Orgs].[Org].[Org].[MEMBER_CAPTION]", "Dimission" },
            { "[Measures].[QueryTimeTotal]","Measure1" },
            { "[Measures].[SamePeriodTotal]","Measure2" }
        };

        private static string buildMdx(string rowAxis)
        {
            var mdx = @"WITH MEMBER [Measures].[QueryTimeSalesQty] AS  [Measures].[BaseUnitQty]

                                            MEMBER [Measures].[SamePeriodPrevYearSalesQty] AS
                                                    SUM(
                                                     {
                                                                ParallelPeriod([Dim_ICBillDates].[YMD].[Year],
		                                                 1,
		                                                 STRTOMEMBER('[Dim_ICBillDates].[YMD].[Day].&[{0}]', CONSTRAINED)
	                                                   ):
	                                                  ParallelPeriod([Dim_ICBillDates].[YMD].[Year],
		                                                 1,
		                                                 STRTOMEMBER('[Dim_ICBillDates].[YMD].[Day].&[{1}]', CONSTRAINED)
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
                                                   , ([Measures].[QueryTimeSalesQty]/[Measures].[SamePeriodPrevYearSalesQty]-1)
                                                 )

                                             MEMBER [Measures].[SamePeriodPrevMonthSalesQty] AS
                                               SUM(
                                                 {
                                                        ParallelPeriod([Dim_ICBillDates].[YMD].[Month],
		                                             1,
		                                             STRTOMEMBER('[Dim_ICBillDates].[YMD].[Day].&[{0}]', CONSTRAINED)
	                                               ):
	                                              ParallelPeriod([Dim_ICBillDates].[YMD].[Month],
		                                             1,
		                                             STRTOMEMBER('[Dim_ICBillDates].[YMD].[Day].&[{1}]', CONSTRAINED)
	                                               )
	                                              },
	                                              [Measures].[BaseUnitQty]
	                                            )

                                             MEMBER [Measures].[SalesQtyMOM] AS
                                                Iif(
                                               IsEmpty([Measures].[QueryTimeSalesQty])
                                               OR IsEmpty([Measures].[SamePeriodPrevMonthSalesQty])
                                               OR[Measures].[SamePeriodPrevMonthSalesQty] = 0
                                               , NULL
                                               , ([Measures].[QueryTimeSalesQty]/[Measures].[SamePeriodPrevMonthSalesQty]-1)
                                             )

                                            MEMBER [Measures].[QueryTimeSalesWeight] AS [Measures].[Weight]

                                            MEMBER [Measures].[SamePeriodPrevYearSalesWeight] AS
                                                SUM(
                                                {
                                                    ParallelPeriod([Dim_ICBillDates].[YMD].[Year],
		                                            1,
		                                            STRTOMEMBER('[Dim_ICBillDates].[YMD].[Day].&[{0}]', CONSTRAINED)
      	                                            ):
	                                            ParallelPeriod([Dim_ICBillDates].[YMD].[Year],
		                                            1,
		                                            STRTOMEMBER('[Dim_ICBillDates].[YMD].[Day].&[{1}]', CONSTRAINED)
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
                                                , ([Measures].[QueryTimeSalesWeight]/[Measures].[SamePeriodPrevYearSalesWeight]-1)
                                            )

                                             MEMBER [Measures].[SamePeriodPrevMonthSalesWeight] AS
                                               SUM(
                                                 {
                                                        ParallelPeriod([Dim_ICBillDates].[YMD].[Month],
		                                             1,
		                                             STRTOMEMBER('[Dim_ICBillDates].[YMD].[Day].&[{0}]', CONSTRAINED)
	                                               ):
	                                              ParallelPeriod([Dim_ICBillDates].[YMD].[Month],
		                                             1,
		                                             STRTOMEMBER('[Dim_ICBillDates].[YMD].[Day].&[{1}]', CONSTRAINED)
	                                               )
	                                              },
	                                              [Measures].[Weight]
	                                            )

                                             MEMBER [Measures].[SalesWeightMOM] AS
                                                        Iif(
                                               IsEmpty([Measures].[QueryTimeSalesWeight])
                                               OR IsEmpty([Measures].[SamePeriodPrevMonthSalesWeight])
                                               OR[Measures].[SamePeriodPrevMonthSalesWeight] = 0
                                               , NULL
                                               , ([Measures].[QueryTimeSalesWeight]/[Measures].[SamePeriodPrevMonthSalesWeight]-1)
                                             )
                                             SELECT  { 
		                                            [Measures].[QueryTimeSalesQty],
		                                            [Measures].[SamePeriodPrevYearSalesQty],
		                                            [Measures].[SalesQtyYOY], 
		                                            [Measures].[SamePeriodPrevMonthSalesQty],
		                                            [Measures].[SalesQtyMOM],
		                                            [Measures].[QueryTimeSalesWeight],
		                                            [Measures].[SamePeriodPrevYearSalesWeight],
		                                            [Measures].[SalesWeightYOY], 
		                                            [Measures].[SamePeriodPrevMonthSalesWeight],
		                                            [Measures].[SalesWeightMOM]
                                                } ON COLUMNS,
                                              NONEMPTY(
		                                            ({ROW_AXIS}),[Measures].[BaseUnitQty]
                                              ) ON ROWS ";

            return mdx.Replace("{ROW_AXIS}", rowAxis);
        }


        private static string buildChartsMdx(string rowAxis,string colAxis)
        {
            var mdx = @"WITH MEMBER [Measures].[QueryTimeTotal] AS  {COL_AXIS}

                                            MEMBER [Measures].[SamePeriodTotal] AS
                                                    SUM(
                                                     {
                                                                ParallelPeriod([Dim_ICBillDates].[YMD].[Year],
		                                                 1,
		                                                 STRTOMEMBER('[Dim_ICBillDates].[YMD].[Month].&[{0}]', CONSTRAINED)
	                                                   ):
	                                                  ParallelPeriod([Dim_ICBillDates].[YMD].[Year],
		                                                 1,
		                                                 STRTOMEMBER('[Dim_ICBillDates].[YMD].[Month].&[{0}]', CONSTRAINED)
	                                                   )
	                                                  },

                                                      {COL_AXIS}
	                                                )
                                             SELECT  { 
		                                            [Measures].[QueryTimeTotal],
		                                            [Measures].[SamePeriodTotal]
                                                } ON COLUMNS,
                                              NONEMPTY(
		                                            ({ROW_AXIS}),{COL_AXIS}
                                              ) ON ROWS ";

            return mdx.Replace("{ROW_AXIS}", rowAxis).Replace("{COL_AXIS}", colAxis);
        }


        /// <summary>
        /// 获取经销商品牌销售回顾报表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDistributorProdBrandSalesData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {
            //System.Diagnostics.Debugger.Break();

            var mdx = buildMdx(@"[Dim_Orgs].[ParentOrg].[ParentOrg].ALLMEMBERS 
                        * [Dim_Orgs].[Org].[Org].ALLMEMBERS 
		                * [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
		                * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
		                * [Dim_Products].[ProdLine].[ProdLine].ALLMEMBERS 
		                * [Dim_Products].[ProdBrand].[ProdBrand].ALLMEMBERS");
            return Utility.Query<T, SalesReviewCriteria>(
                con, 
                request,
                user, 
                mdx, 
                reviewMaps
                );
        }


        /// <summary>
        /// 获取经销商品类销售回顾报表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDistributorProdCategorySalesData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = buildMdx(@"[Dim_Orgs].[ParentOrg].[ParentOrg].ALLMEMBERS
                *[Dim_Orgs].[Org].[Org].ALLMEMBERS
                * [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS
                * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
                * [Dim_Products].[ProdCategory].[ProdCategory].ALLMEMBERS");
            return Utility.Query<T, SalesReviewCriteria>(
                con,
                request,
                user,
                mdx,
                reviewMaps
                );
        }


        /// <summary>
        /// 获取经销商产品线销售回顾报表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDistributorProdLineSalesData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {
            var mdx = buildMdx(@"[Dim_Orgs].[ParentOrg].[ParentOrg].ALLMEMBERS
		        *[Dim_Orgs].[Org].[Org].ALLMEMBERS
		        * [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
		        * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
		        * [Dim_Products].[ProdLine].[ProdLine].ALLMEMBERS");

            return Utility.Query<T, SalesReviewCriteria>(
                con,
                request,
                user,
                mdx,
                reviewMaps
                );
        }


        /// <summary>
        /// 获取经销商单品销售回顾报表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDistributorProductSalesData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = buildMdx(@"[Dim_Orgs].[ParentOrg].[ParentOrg].ALLMEMBERS
		        *[Dim_Orgs].[Org].[Org].ALLMEMBERS
		        * [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
		        * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
		        * [Dim_Products].[ProdLine].[ProdLine].ALLMEMBERS 
                * [Dim_Products].[ProdCategory].[ProdCategory].ALLMEMBERS
                * [Dim_Products].[ProdBrand].[ProdBrand].ALLMEMBERS
                * [Dim_Products].[ProdCode].[ProdCode].ALLMEMBERS
                * [Dim_Products].[Id].[Id].ALLMEMBERS");

            return Utility.Query<T, SalesReviewCriteria>(
                con,
                request,
                user,
                mdx,
                reviewMaps
                );
        }





        /// <summary>
        /// 获取经销商品牌销售回顾报表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDistributorProdBrandSalesData_Distributor<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {
            //System.Diagnostics.Debugger.Break();

            var mdx = buildMdx(@"
		                [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
		                * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
		                * [Dim_Products].[ProdLine].[ProdLine].ALLMEMBERS 
		                * [Dim_Products].[ProdBrand].[ProdBrand].ALLMEMBERS");
            return Utility.Query<T, SalesReviewCriteria>(
                con,
                request,
                user,
                mdx,
                reviewMaps
                );
        }


        /// <summary>
        /// 获取经销商品类销售回顾报表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDistributorProdCategorySalesData_Distributor<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = buildMdx(@"
                [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS
                * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
                * [Dim_Products].[ProdCategory].[ProdCategory].ALLMEMBERS");
            return Utility.Query<T, SalesReviewCriteria>(
                con,
                request,
                user,
                mdx,
                reviewMaps
                );
        }


        /// <summary>
        /// 获取经销商产品线销售回顾报表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDistributorProdLineSalesData_Distributor<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {
            var mdx = buildMdx(@"
		        [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
		        * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
		        * [Dim_Products].[ProdLine].[ProdLine].ALLMEMBERS");

            return Utility.Query<T, SalesReviewCriteria>(
                con,
                request,
                user,
                mdx,
                reviewMaps
                );
        }


        /// <summary>
        /// 获取经销商单品销售回顾报表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDistributorProductSalesData_Distributor<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = buildMdx(@"
		         [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
		        * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
		        * [Dim_Products].[ProdLine].[ProdLine].ALLMEMBERS 
                * [Dim_Products].[ProdCategory].[ProdCategory].ALLMEMBERS
                * [Dim_Products].[ProdBrand].[ProdBrand].ALLMEMBERS
                * [Dim_Products].[ProdCode].[ProdCode].ALLMEMBERS
                * [Dim_Products].[Id].[Id].ALLMEMBERS");

            return Utility.Query<T, SalesReviewCriteria>(
                con,
                request,
                user,
                mdx,
                reviewMaps
                );
        }


        /// <summary>
        /// 获取经销商销售数量分析图表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <param name="user"></param>
        /// <returns></returns>

        public static IEnumerable<T> GetDistributorProductSalesQtyForChartsData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = buildChartsMdx(@"[Dim_ICBillDates].[YMD].[Month]", "[Measures].[BaseUnitQty]");
            return Utility.Query<T, DistributorSalesCriteria>(
                con,
                request,
                user,
                mdx,
                chartMaps
                );
        }

        /// <summary>
        /// 获取经销商销售重量分析图表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <param name="user"></param>
        /// <returns></returns>

        public static IEnumerable<T> GetDistributorProductSalesWeightForChartsData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = buildChartsMdx(@"[Dim_ICBillDates].[YMD].[Month]", "[Measures].[Weight]");
            return Utility.Query<T, DistributorSalesCriteria>(
                con,
                request,
                user,
                mdx,
                chartMaps
                );
        }


        /// <summary>
        /// 获取分公司销售数量分析图表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <param name="user"></param>
        /// <returns></returns>

        public static IEnumerable<T> GetOrgProductSalesQtyForChartsData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = buildChartsMdx(@"[Dim_Orgs].[Org].[Org]", "[Measures].[BaseUnitQty]");
            return Utility.Query<T, SalesRankCriteria>(
                con,
                request,
                user,
                mdx,
                chartMaps
                );
        }


        /// <summary>
        /// 获取分公司销售重量分析图表数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetOrgProductSalesWeightForChartsData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = buildChartsMdx(@"[Dim_Orgs].[Org].[Org]", "[Measures].[Weight]");
            return Utility.Query<T, SalesRankCriteria>(
                con,
                request,
                user,
                mdx,
                chartMaps
                );
        }
    }
}
