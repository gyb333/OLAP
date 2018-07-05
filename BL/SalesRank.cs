
namespace Wilmar.KDSA.Service.Controllers
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Dapper;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Wilmar.Interface.Sync.OLAP.Criterias;
    using Wilmar.SSAS.MDXBuilder;
    using Wilmar.Interface.Sync.OLAP;


    /// <summary>
    /// 销售排名
    /// </summary>
    public static class SalesRank
    {


        /// <summary>
        /// DTO于多维模型属性映射关系
        /// </summary>
        private static Dictionary<string, string> propertyMap = new Dictionary<string, string>()
        {
            { "[Dim_ICBillDates].[Month].[Month].[MEMBER_CAPTION]", "SalesDate" },
            { "[Dim_Orgs].[ParentOrg].[ParentOrg].[MEMBER_CAPTION]", "SalesRegionName" },
            { "[Dim_Orgs].[Org].[Org].[MEMBER_CAPTION]", "SalesOrgName" },
            { "[Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].[MEMBER_CAPTION]", "DistributorCode" },
            { "[Dim_CompBranches].[DistributorContract].[DistributorContract].[MEMBER_CAPTION]", "DistributorName" },
            { "[Dim_Products].[ProdLine].[ProdLine].[MEMBER_CAPTION]", "ProductLineName" },
            { "[Dim_Products].[ProdBrand].[ProdBrand].[MEMBER_CAPTION]", "ProductBrandName" },
            { "[Dim_Products].[ProdCategory].[ProdCategory].[MEMBER_CAPTION]", "ProductCategoryName" },
            { "[Dim_Products].[ProdCode].[ProdCode].[MEMBER_CAPTION]", "ProductCode" },
            { "[Dim_Products].[Id].[Id].[MEMBER_CAPTION]", "ProductName" },
            { "[Measures].[CurMonthSalesQty]","CurMonthSalesQty" },
            { "[Measures].[CurMonthSalesWeight]","CurMonthSalesWeight" },
            { "[Measures].[CurQuarterSalesQty]","CurQuarterSalesQty" },
            { "[Measures].[CurQuarterSalesWeight]","CurQuarterSalesWeight" },
            { "[Measures].[CurYearSalesQty]","CurYearSalesQty" },
            { "[Measures].[CurYearSalesWeight]","CurYearSalesWeight" },
            { "[Measures].[CurMonthRank]","CurMonthRank" },
            { "[Measures].[CurQuarterRank]","CurQuarterRank" },
            { "[Measures].[CurYearRank]","CurYearRank" }
        };


        private static Dictionary<string, string> chartPropertyMap = new Dictionary<string, string>()
        {
            { "[Dim_CompBranches].[DistributorContract].[DistributorContract].[MEMBER_CAPTION]", "Dimission" },
            { "[Dim_Products].[ProdBrand].[ProdBrand].[MEMBER_CAPTION]", "Dimission" },
            { "[Dim_Products].[Id].[Id].[MEMBER_CAPTION]", "Dimission" },
            { "[Dim_Customers].[CustChannelType].[CustChannelType].[MEMBER_CAPTION]", "Dimission" },
            { "[Dim_Customers].[CustChannel].[CustChannel].[MEMBER_CAPTION]", "Dimission" },
            { "[Measures].[CurMonthSalesQty]","Measure1" },
            { "[Measures].[PrevOneMonthSalesQty]","Measure2" },
            { "[Measures].[PrevTwoMonthSalesQty]","Measure3" },
            { "[Measures].[BaseUnitQty]","Measure1" },
            { "[Measures].[Weight]","Measure1" }
        };


        

        private static string buildMdx(string rankset, string setcurmember)
        {
            var mdx = @"WITH 
                SET [SalesRankSet] AS
                      NonEmpty({RANK_SET}
                            , ( [Measures].[BaseUnitQty] )
                      )
                MEMBER [Measures].[CurMonthSalesQty] AS  
	                [Measures].[BaseUnitQty]

                MEMBER [Measures].[CurMonthSalesWeight] AS  
	                [Measures].[Weight]

                MEMBER [Measures].[CurQuarterSalesQty] AS
	                AGGREGATE(
	                PERIODSTODATE([Dim_ICBillDates].[YQMD].[Quarter],
	                [Dim_ICBillDates].[YQMD].CurrentMember
	                ),
	                ([Measures].[BaseUnitQty])
                )

                MEMBER [Measures].[CurQuarterSalesWeight] AS 
	                 AGGREGATE(
	                PERIODSTODATE([Dim_ICBillDates].[YQMD].[Quarter],
	                [Dim_ICBillDates].[YQMD].CurrentMember
	                ),
	                ([Measures].[Weight])
	                )

                MEMBER [Measures].[CurYearSalesQty] AS 
	                AGGREGATE(
	                PERIODSTODATE([Dim_ICBillDates].[YQMD].[Year],
	                [Dim_ICBillDates].[YQMD].CurrentMember
	                ),
	                ([Measures].[BaseUnitQty])
	                )

                 MEMBER [Measures].[CurYearSalesWeight] AS 
	                AGGREGATE(
	                PERIODSTODATE([Dim_ICBillDates].[YQMD].[Year],
	                [Dim_ICBillDates].[YQMD].CurrentMember
	                ),
	                ([Measures].[Weight])
	                )

                 MEMBER [Measures].[CurMonthRank] AS
                   RANK( ({RANK_CURRENTMEMBER} ),
		                  ORDER(  [SalesRankSet] , [Measures].[CurMonthSalesWeight], BDESC)
                   )

                 MEMBER [Measures].[CurQuarterRank] AS
                   RANK( ({RANK_CURRENTMEMBER} ),
		                  ORDER(  [SalesRankSet] , [Measures].[CurQuarterSalesWeight], BDESC)
                   )

                 MEMBER [Measures].[CurYearRank] AS
                   RANK( ({RANK_CURRENTMEMBER} ),
		                  ORDER(  [SalesRankSet] , [Measures].[CurYearSalesWeight], BDESC)
                   )


                 SELECT { [Measures].[CurMonthSalesQty],
		                [Measures].[CurMonthSalesWeight],
		                [Measures].[CurMonthRank],
		                [Measures].[CurQuarterSalesQty],
		                [Measures].[CurQuarterSalesWeight],
			            [Measures].[CurQuarterRank],
		                [Measures].[CurYearSalesQty],
		                [Measures].[CurYearSalesWeight],
                        [Measures].[CurYearRank]
                 } ON COLUMNS, 
                ORDER([SalesRankSet],[Measures].[BaseUnitQty],BDESC) ON ROWS ";

            return mdx.Replace("{RANK_SET}", rankset).Replace("{RANK_CURRENTMEMBER}", setcurmember);
        }



        /// <summary>
        /// 获取经销商单品销售排名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDistributorProdRankData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = buildMdx(@"[Dim_ICBillDates].[Month].[Month].ALLMEMBERS
                    *[Dim_Orgs].[ParentOrg].[ParentOrg].ALLMEMBERS 
                    * [Dim_Orgs].[Org].[Org].ALLMEMBERS 
                    * [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
                    * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
                    * [Dim_Products].[ProdLine].[ProdLine].ALLMEMBERS 
                    * [Dim_Products].[ProdCategory].[ProdCategory].ALLMEMBERS 
                    * [Dim_Products].[ProdBrand].[ProdBrand].ALLMEMBERS 
                    * [Dim_Products].[Id].[Id].ALLMEMBERS 
                    * [Dim_Products].[ProdCode].[ProdCode].ALLMEMBERS", @"[Dim_ICBillDates].[Month].CurrentMember
                    ,[Dim_Orgs].[ParentOrg].CurrentMember 
                    , [Dim_Orgs].[Org].CurrentMember
                    , [Dim_CompBranches].[DistriContractCodeSAP].CurrentMember 
                    , [Dim_CompBranches].[DistributorContract].CurrentMember
                    , [Dim_Products].[ProdLine].CurrentMember 
                    , [Dim_Products].[ProdCategory].CurrentMember
                    , [Dim_Products].[ProdBrand].CurrentMember
                    , [Dim_Products].[Id].CurrentMember 
                    , [Dim_Products].[ProdCode].CurrentMember");

            return Utility.Query<T, SalesRankCriteria>(
                con,
                request,
                user,
                mdx,
            propertyMap);
        }


        /// <summary>
        /// 获取经销商品牌销售排名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDistributorProdBrandRankData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = buildMdx(@"[Dim_ICBillDates].[Month].[Month].ALLMEMBERS
                    *[Dim_Orgs].[ParentOrg].[ParentOrg].ALLMEMBERS 
                    * [Dim_Orgs].[Org].[Org].ALLMEMBERS 
                    * [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
                    * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
                    * [Dim_Products].[ProdBrand].[ProdBrand].ALLMEMBERS"
            , @"[Dim_ICBillDates].[Month].CurrentMember
                    ,[Dim_Orgs].[ParentOrg].CurrentMember 
                    , [Dim_Orgs].[Org].CurrentMember
                    , [Dim_CompBranches].[DistriContractCodeSAP].CurrentMember 
                    , [Dim_CompBranches].[DistributorContract].CurrentMember
                    , [Dim_Products].[ProdBrand].CurrentMember");

            return Utility.Query<T, SalesRankCriteria>(
                con,
                request,
                user,
                mdx,
            propertyMap);
        }


        /// <summary>
        /// 经销商品类销售排名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDistributorProdCategoryRankData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {
            var mdx = buildMdx(@"[Dim_ICBillDates].[Month].[Month].ALLMEMBERS
                    *[Dim_Orgs].[ParentOrg].[ParentOrg].ALLMEMBERS 
                    * [Dim_Orgs].[Org].[Org].ALLMEMBERS 
                    * [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
                    * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
                    * [Dim_Products].[ProdCategory].[ProdCategory].ALLMEMBERS",
            @"[Dim_ICBillDates].[Month].CurrentMember
                    ,[Dim_Orgs].[ParentOrg].CurrentMember 
                    , [Dim_Orgs].[Org].CurrentMember
                    , [Dim_CompBranches].[DistriContractCodeSAP].CurrentMember 
                    , [Dim_CompBranches].[DistributorContract].CurrentMember
                    , [Dim_Products].[ProdCategory].CurrentMember");

            return Utility.Query<T, SalesRankCriteria>(
                con,
                request,
                user,
                mdx,
            propertyMap);


        }


        /// <summary>
        /// 经销商产品线销售排名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDistributorProdLineRankData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = buildMdx(@"[Dim_ICBillDates].[Month].[Month].ALLMEMBERS
                    *[Dim_Orgs].[ParentOrg].[ParentOrg].ALLMEMBERS 
                    * [Dim_Orgs].[Org].[Org].ALLMEMBERS 
                    * [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
                    * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
                    * [Dim_Products].[ProdLine].[ProdLine].ALLMEMBERS"
         , @"[Dim_ICBillDates].[Month].CurrentMember
                    ,[Dim_Orgs].[ParentOrg].CurrentMember 
                    , [Dim_Orgs].[Org].CurrentMember
                    , [Dim_CompBranches].[DistriContractCodeSAP].CurrentMember 
                    , [Dim_CompBranches].[DistributorContract].CurrentMember 
                    , [Dim_Products].[ProdLine].CurrentMember");

            return Utility.Query<T, SalesRankCriteria>(
                con,
                request,
                user,
                mdx,
            propertyMap);


        }



        /// <summary>
        /// 获取经销商销售排名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDistributorRankData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = buildMdx(@"[Dim_ICBillDates].[Month].[Month].ALLMEMBERS
                    *[Dim_Orgs].[ParentOrg].[ParentOrg].ALLMEMBERS 
                    * [Dim_Orgs].[Org].[Org].ALLMEMBERS 
                    * [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
                    * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS"
       , @"[Dim_ICBillDates].[Month].CurrentMember
                    ,[Dim_Orgs].[ParentOrg].CurrentMember 
                    , [Dim_Orgs].[Org].CurrentMember
                    , [Dim_CompBranches].[DistriContractCodeSAP].CurrentMember 
                    , [Dim_CompBranches].[DistributorContract].CurrentMember");

            return Utility.Query<T, SalesRankCriteria>(
                con,
                request,
                user,
                mdx,
            propertyMap);


        }










        /// <summary>
        /// 获取经销商单品销售排名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDistributorProdRankData_Distributor<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = buildMdx(@"[Dim_ICBillDates].[Month].[Month].ALLMEMBERS
                    * [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
                    * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
                    * [Dim_Products].[ProdLine].[ProdLine].ALLMEMBERS 
                    * [Dim_Products].[ProdCategory].[ProdCategory].ALLMEMBERS 
                    * [Dim_Products].[ProdBrand].[ProdBrand].ALLMEMBERS 
                    * [Dim_Products].[Id].[Id].ALLMEMBERS 
                    * [Dim_Products].[ProdCode].[ProdCode].ALLMEMBERS", 
                    @"[Dim_ICBillDates].[Month].CurrentMember
                    , [Dim_CompBranches].[DistriContractCodeSAP].CurrentMember 
                    , [Dim_CompBranches].[DistributorContract].CurrentMember
                    , [Dim_Products].[ProdLine].CurrentMember 
                    , [Dim_Products].[ProdCategory].CurrentMember
                    , [Dim_Products].[ProdBrand].CurrentMember
                    , [Dim_Products].[Id].CurrentMember 
                    , [Dim_Products].[ProdCode].CurrentMember");

            return Utility.Query<T, SalesRankCriteria>(
                con,
                request,
                user,
                mdx,
            propertyMap);
        }


        /// <summary>
        /// 获取经销商品牌销售排名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDistributorProdBrandRankData_Distributor<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = buildMdx(@"[Dim_ICBillDates].[Month].[Month].ALLMEMBERS
                    * [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
                    * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
                    * [Dim_Products].[ProdBrand].[ProdBrand].ALLMEMBERS"
            , @"[Dim_ICBillDates].[Month].CurrentMember
                    , [Dim_CompBranches].[DistriContractCodeSAP].CurrentMember 
                    , [Dim_CompBranches].[DistributorContract].CurrentMember
                    , [Dim_Products].[ProdBrand].CurrentMember");

            return Utility.Query<T, SalesRankCriteria>(
                con,
                request,
                user,
                mdx,
            propertyMap);
        }


        /// <summary>
        /// 经销商品类销售排名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDistributorProdCategoryRankData_Distributor<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {
            var mdx = buildMdx(@"[Dim_ICBillDates].[Month].[Month].ALLMEMBERS
                    * [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
                    * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
                    * [Dim_Products].[ProdCategory].[ProdCategory].ALLMEMBERS",
            @"[Dim_ICBillDates].[Month].CurrentMember
                    , [Dim_CompBranches].[DistriContractCodeSAP].CurrentMember 
                    , [Dim_CompBranches].[DistributorContract].CurrentMember
                    , [Dim_Products].[ProdCategory].CurrentMember");

            return Utility.Query<T, SalesRankCriteria>(
                con,
                request,
                user,
                mdx,
            propertyMap);


        }


        /// <summary>
        /// 经销商产品线销售排名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDistributorProdLineRankData_Distributor<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = buildMdx(@"[Dim_ICBillDates].[Month].[Month].ALLMEMBERS
                    * [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
                    * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS
                    * [Dim_Products].[ProdLine].[ProdLine].ALLMEMBERS"
                    , @"[Dim_ICBillDates].[Month].CurrentMember
                    , [Dim_CompBranches].[DistriContractCodeSAP].CurrentMember 
                    , [Dim_CompBranches].[DistributorContract].CurrentMember 
                    , [Dim_Products].[ProdLine].CurrentMember");

            return Utility.Query<T, SalesRankCriteria>(
                con,
                request,
                user,
                mdx,
            propertyMap);


        }



        /// <summary>
        /// 获取经销商销售排名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDistributorRankData_Distributor<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = buildMdx(@"[Dim_ICBillDates].[Month].[Month].ALLMEMBERS
                    * [Dim_CompBranches].[DistriContractCodeSAP].[DistriContractCodeSAP].ALLMEMBERS 
                    * [Dim_CompBranches].[DistributorContract].[DistributorContract].ALLMEMBERS"
       , @"[Dim_ICBillDates].[Month].CurrentMember
                    , [Dim_CompBranches].[DistriContractCodeSAP].CurrentMember 
                    , [Dim_CompBranches].[DistributorContract].CurrentMember");

            return Utility.Query<T, SalesRankCriteria>(
                con,
                request,
                user,
                mdx,
            propertyMap);


        }




        /// <summary>
        /// 获取经销商销售数量前20名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDistributorSalesQtyTop20Data<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = @"SELECT {[Measures].[BaseUnitQty]} ON COLUMNS,
                               TopCount( NONEMPTY( [Dim_Products].[Id].children), 20, ([Measures].[BaseUnitQty]) ) ON ROWS";


            return Utility.Query<T, SalesReviewCriteria>(
               con,
               request,
               user,
               mdx,
           chartPropertyMap);
        }


        /// <summary>
        /// 获取经销商销售重量前20名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDistributorSalesWeightTop20Data<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {


            var mdx = @"SELECT {[Measures].[Weight]} ON COLUMNS,
                               TopCount(NONEMPTY( [Dim_Products].[Id].children), 20, ([Measures].[Weight]) ) ON ROWS";


            return Utility.Query<T, SalesReviewCriteria>(
               con,
               request,
               user,
               mdx,
           chartPropertyMap);

        }



        /// <summary>
        /// 获取品牌销量数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetProdBrandSalesQtyData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {


            var mdx = @"WITH MEMBER [Measures].[CurMonthSalesQty] AS   
                 SUM(
                    [Dim_ICBillDates].[YMD].[Month].&[{0}],
	                    [Measures].[BaseUnitQty]
	                )
                    MEMBER [Measures].[PrevOneMonthSalesQty] AS
                    SUM(
                        {
                            ParallelPeriod([Dim_ICBillDates].[YMD].[Month],
		                    1,
		                    [Dim_ICBillDates].[YMD].[Month].&[{0}]
	                    )
	                    },
	                    [Measures].[BaseUnitQty]
	                )

                    MEMBER [Measures].[PrevTwoMonthSalesQty] AS
                    SUM(
                        {
                            ParallelPeriod([Dim_ICBillDates].[YMD].[Month],
		                    2,
		                    [Dim_ICBillDates].[YMD].[Month].&[{0}]
	                    )
	                    },
	                    [Measures].[BaseUnitQty]
	                )
                    SELECT  { 
		                [Measures].[CurMonthSalesQty],
		                [Measures].[PrevOneMonthSalesQty],
		                [Measures].[PrevTwoMonthSalesQty]
                    } ON COLUMNS,
                    NON EMPTY { 
		                (
		                    [Dim_Products].[ProdBrand].[ProdBrand].MEMBERS  )
                    } ON ROWS ";


            return Utility.Query<T, SalesRankCriteria>(
               con,
               request,
               user,
               mdx,
           chartPropertyMap);

        }


        private static Dictionary<string, string> chartProdBrandPropertyMap = new Dictionary<string, string>()
        {
       
            { "[Dim_Products].[ProdBrand].[ProdBrand].[MEMBER_CAPTION]", "Dimission" },        
            { "[Measures].[CurMonthSalesWeight]","Measure1" },
            { "[Measures].[PrevOneMonthSalesWeight]","Measure2" },
            { "[Measures].[PrevTwoMonthSalesWeight]","Measure3" },
 
        };


        /// <summary>
        /// 获取品牌销量数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetProdBrandSalesWeightData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {


            var mdx = @" WITH MEMBER [Measures].[CurMonthSalesWeight] AS   
                 [Measures].[Weight]
                    
					MEMBER [Measures].[PrevOneMonthSalesWeight] AS
                    SUM(
                        {
                            ParallelPeriod([Dim_ICBillDates].[YMD].[Month],
		                    1,
		                    [Dim_ICBillDates].[YMD].CurrentMember
	                    )
	                    },
	                    [Measures].[Weight]
	                )

                    MEMBER [Measures].[PrevTwoMonthSalesWeight] AS
                    SUM(
                        {
                            ParallelPeriod([Dim_ICBillDates].[YMD].[Month],
		                    2,
		                    [Dim_ICBillDates].[YMD].CurrentMember
	                    )
	                    },
	                    [Measures].[Weight]
	                )
                    SELECT  { 
		                [Measures].[CurMonthSalesWeight],
		                [Measures].[PrevOneMonthSalesWeight],
		                [Measures].[PrevTwoMonthSalesWeight]
                    } 
                    ON COLUMNS,
                    NON EMPTY { 
		                (
		                    [Dim_Products].[ProdBrand].[ProdBrand].MEMBERS 
                            * [Dim_ICBillDates].[YMD].[Month].ALLMEMBERS  
                            )
                    } ON ROWS ";


            return Utility.Query<T, SalesRankCriteria>(
               con,
               request,
               user,
               mdx,
           chartProdBrandPropertyMap);

        }




        /// <summary>
        /// 获取销量按客户类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetCustomerChanelSalesQtyData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = @"SELECT {[Measures].[BaseUnitQty]} ON COLUMNS,
                            NON EMPTY {[Dim_Customers].[CustChannel].children } ON ROWS";


            return Utility.Query<T, SalesRankCriteria>(
               con,
               request,
               user,
               mdx,
           chartPropertyMap);

        }


        /// <summary>
        /// 获取销量按客户渠道
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetCustomerCustChannelTypeSalesQtyData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = @"SELECT {[Measures].[BaseUnitQty]} ON COLUMNS,
                            NON EMPTY {[Dim_Customers].[CustChannelType].children } ON ROWS";


            return Utility.Query<T, SalesRankCriteria>(
               con,
               request,
               user,
               mdx,
           chartPropertyMap);

        }



        /// <summary>
        /// 获取销售重量按客户类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetCustomerChanelSalesWeightData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = @"SELECT {[Measures].[Weight]} ON COLUMNS,
                            NON EMPTY {[Dim_Customers].[CustChannel].children } ON ROWS";


            return Utility.Query<T, SalesRankCriteria>(
               con,
               request,
               user,
               mdx,
           chartPropertyMap);


        }


        /// <summary>
        /// 获取销售重量按客户渠道
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetCustomerCustChannelTypeSalesWeightData<T>(DbConnection con, HttpRequestMessage request, UserInfoDTO user)
        {

            var mdx = @"SELECT {[Measures].[Weight]} ON COLUMNS,
                            NON EMPTY {[Dim_Customers].[CustChannelType].children } ON ROWS";


            return Utility.Query<T, SalesRankCriteria>(
               con,
               request,
               user,
               mdx,
           chartPropertyMap);
        }
    }

}
